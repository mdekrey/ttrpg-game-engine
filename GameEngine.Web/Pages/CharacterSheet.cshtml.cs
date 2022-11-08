using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace GameEngine.Web.Pages
{
    public class CharacterSheetModel : PageModel
    {
        public static readonly ConcurrentDictionary<string, FoundryApi.Actor> Characters = new(new Dictionary<string, FoundryApi.Actor>
        {
            ["Sample"] = System.Text.Json.JsonSerializer.Deserialize<FoundryApi.Actor>(@"{""name"":""Neala"",""level"":3,""class"":""Barbarian"",""totalXp"":2150000,""race"":""Minotaur"",""size"":""M"",""speed"":[{""type"":""racial"",""amount"":6},{""type"":""armor"",""amount"":-1},{""type"":""feat"",""amount"":1}],""abilities"":{""str"":-2,""con"":-1,""dex"":0,""int"":1,""wis"":2,""cha"":3},""defenses"":{""ac"":[{""type"":"""",""amount"":11},{""type"":""ability"",""amount"":3},{""type"":""armor"",""amount"":3},{""type"":""class"",""amount"":1},{""type"":""enhancement"",""amount"":1}],""acConditional"":[""+1 when bloodied""],""fort"":[{""type"":"""",""amount"":11},{""type"":""ability"",""amount"":3},{""type"":""class"",""amount"":1},{""type"":""enhancement"",""amount"":1}],""fortConditional"":[],""refl"":[{""type"":"""",""amount"":11},{""type"":""ability"",""amount"":3},{""type"":""class"",""amount"":1},{""type"":""enhancement"",""amount"":1}],""reflConditional"":[],""will"":[{""type"":"""",""amount"":11},{""type"":""ability"",""amount"":3},{""type"":""class"",""amount"":1},{""type"":""enhancement"",""amount"":1}],""willConditional"":[]},""maxHp"":45,""surgesPerDay"":8,""savingThrowModifiers"":[""+1 to saving throws""],""resistances"":[""fire 5""],""raceFeatures"":[""Goring Charge""],""classFeatures"":[""Unarmored Defense""],""feats"":[""Something""],""weaponArmorProficiencies"":[""Martial Weapons""],""languages"":[""Albion"",""West Wealden""],""skills"":[{""name"":""Social(Commonfolk)"",""modifiers"":[{""type"":""ranks"",""amount"":6}]},{""name"":""Conjuration"",""modifiers"":[{""type"":""ranks"",""amount"":6}]},{""name"":""Feywild Knowledge"",""modifiers"":[{""type"":""ranks"",""amount"":6}]}],""currency"":{""cp"":17,""sp"":47,""gp"":213,""pp"":0,""ad"":0},""equipment"":[""Backpack"",""Potion of Healing x2""]}")!,
        });

        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        public void OnGet()
        {
        }

        public FoundryApi.Actor? GetCharcterData() =>
            Id != null && Characters.TryGetValue(Id, out var actor) ? actor : null;
    }
}
