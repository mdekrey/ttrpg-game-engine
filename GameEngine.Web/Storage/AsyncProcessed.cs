
using System;

namespace GameEngine.Web.Storage;
public record AsyncProcessed<T>(T Original, bool InProgress, Guid CorrelationToken);
