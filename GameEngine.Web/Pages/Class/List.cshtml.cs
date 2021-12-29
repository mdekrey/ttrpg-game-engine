using GameEngine.Web.Api;
using GameEngine.Web.AsyncServices;
using GameEngine.Web.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameEngine.Web.Pages.Class
{
    public class ListModel : PageModel
    {
        private readonly ITableStorage<ClassDetails> classStorage;

        public ListModel(ITableStorage<ClassDetails> classStorage)
        {
            this.classStorage = classStorage;
        }

        public Dictionary<Guid, Api.ClassProfile>? Classes { get; private set; }

        public async Task OnGet()
        {
            Classes = await (from kvp in classStorage.Query((key, c) => c.ProgressState != AsyncServices.ProgressState.Deleted).Where(c => c.Value.ProgressState != AsyncServices.ProgressState.Deleted)
                             select new KeyValuePair<Guid, ClassDetails>(ClassDetails.IdFromTableKey(kvp.Key), kvp.Value)).ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value.ToApi());
        }
    }
}
