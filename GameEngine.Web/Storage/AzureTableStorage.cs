using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GameEngine.Web.Storage;

public class AzureTableStorage<T, TTableEntity> : ITableStorage<T>
    where T : class, IStorable<TTableEntity, TableKey>
    where TTableEntity : class, ITableEntity, new()
{
    public delegate T EntityFactory(TTableEntity tableEntity);
    private readonly TableClient tableClient;
    private readonly EntityFactory entityFactory;

    public AzureTableStorage(TableClient tableClient, EntityFactory entityFactory)
    {
        this.tableClient = tableClient;
        this.entityFactory = entityFactory;
    }

    public async Task<bool> DeleteAsync(TableKey id)
    {
        await tableClient.DeleteEntityAsync(id.PartitionKey, id.RowKey);
        return true;
    }

    public async Task<StorageStatus<T>> LoadAsync(TableKey id)
    {
        var response = await tableClient.GetEntityAsync<TTableEntity>(id.PartitionKey, id.RowKey);
        if (response.GetRawResponse().Status != 200)
            return new StorageStatus<T>.Failure();
        return new StorageStatus<T>.Success(entityFactory(response.Value));
    }

    public async Task SaveAsync(TableKey id, T data)
    {
        var tableEntity = data.ToStorableEntity(id);
        await tableClient.UpsertEntityAsync(tableEntity);
    }

    public async IAsyncEnumerable<KeyValuePair<TableKey, T>> Query(Expression<Func<TableKey, T, bool>> filter)
    {
        var newParameter = Expression.Parameter(typeof(TTableEntity));
        var filterExpression = Expression.Lambda<Func<TTableEntity, bool>>(new QueryParameterRewriteVisitor(filter.Parameters, newParameter).Visit(filter.Body), newParameter);
        var pageable = tableClient.QueryAsync(filterExpression);
        await foreach(var entry in pageable)
        {
            yield return new KeyValuePair<TableKey, T>(new TableKey(entry.PartitionKey, entry.RowKey), entityFactory(entry));
        }
    }

    private class QueryParameterRewriteVisitor : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> parameters;
        private ParameterExpression newParameter;

        public QueryParameterRewriteVisitor(ReadOnlyCollection<ParameterExpression> parameters, ParameterExpression newParameter)
        {
            this.parameters = parameters;
            this.newParameter = newParameter;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node is { Member: System.Reflection.PropertyInfo { Name: string name }, Expression: ParameterExpression paramExpression } && parameters.Contains(paramExpression))
                return Expression.Property(newParameter, name);

            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (parameters.Contains(node))
                return newParameter;
            return base.VisitParameter(node);
        }
    }
}

