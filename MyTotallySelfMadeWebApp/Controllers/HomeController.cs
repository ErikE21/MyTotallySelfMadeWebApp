using Microsoft.AspNetCore.Mvc;
using MyTotallySelfMadeWebApp.Models;
using System.Diagnostics;
using PokeApiNet;
using System.Drawing;
using Newtonsoft.Json;
using AppModels.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualBasic;

namespace MyTotallySelfMadeWebApp.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PokeApiClient _pokeClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HomeController(ILogger<HomeController> logger, PokeApiClient pokeClient, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _pokeClient = pokeClient;
        _httpContextAccessor = httpContextAccessor;
    }

    [StartPageFilter]
    public IActionResult Index()
    {
        Dictionary<string, Color> TypeColors = new Dictionary<string, Color>
        {
            { "normal", System.Drawing.ColorTranslator.FromHtml("#a0a2a0") },
            { "fighting", System.Drawing.ColorTranslator.FromHtml("#ff8100") },
            { "flying", System.Drawing.ColorTranslator.FromHtml("#82baef") },
            { "poison", System.Drawing.ColorTranslator.FromHtml("#913ecd") },
            { "ground", System.Drawing.ColorTranslator.FromHtml("#92501b") },
            { "rock", System.Drawing.ColorTranslator.FromHtml("#b0ab82") },
            { "bug", System.Drawing.ColorTranslator.FromHtml("#92a212") },
            { "ghost", System.Drawing.ColorTranslator.FromHtml("#713f71") },
            { "steel", System.Drawing.ColorTranslator.FromHtml("#60a2b9") },
            { "fire", System.Drawing.ColorTranslator.FromHtml("#e72324") },
            { "water", System.Drawing.ColorTranslator.FromHtml("#2481ef") },
            { "grass", System.Drawing.ColorTranslator.FromHtml("#3da224") },
            { "electric", System.Drawing.ColorTranslator.FromHtml("#fac100") },
            { "psychic", System.Drawing.ColorTranslator.FromHtml("#ef3f7a") },
            { "ice", System.Drawing.ColorTranslator.FromHtml("#3dd9ff") },
            { "dragon", System.Drawing.ColorTranslator.FromHtml("#4f61e2") },
            { "dark", System.Drawing.ColorTranslator.FromHtml("#4f3f3d") },
            { "fairy", System.Drawing.ColorTranslator.FromHtml("#f171f1") }
        };
        var colorsJson = _httpContextAccessor.HttpContext.Session.GetString("colors");
        if (string.IsNullOrEmpty(colorsJson))
        {
            _httpContextAccessor.HttpContext.Session.SetString("colors", JsonConvert.SerializeObject(TypeColors));
        }
        return View();
    }

    public async Task<IActionResult> GetPokemonData(int id)
    {
        int pagination;
        if (id == null)
        {
            pagination = 0;
        }
        else if (id > 40)
        {
            pagination = 40 * 25;
        }
        else if (id <= 0)
        {
            pagination = 0;
        }
        else
        {
            pagination = (id - 1) * 25;
        }
        var PokesList = new List<Pokemon>();

        NamedApiResourceList<Pokemon> allPokes = await _pokeClient.GetNamedResourcePageAsync<Pokemon>(25, pagination);
        var tasks = allPokes.Results.Select(async namedResource =>
        {
            return await _pokeClient.GetResourceAsync<Pokemon>(namedResource.Name);
        }).ToList();
        PokesList = (await Task.WhenAll(tasks)).ToList();

        var TypeColors = _httpContextAccessor.HttpContext.Session.GetString("colors");
        ViewBag.TypeColors = JsonConvert.DeserializeObject<Dictionary<string, Color>>(TypeColors);
        ViewData["id"] = id;
        if (TempData["TeamCap"] is not null)
            ViewData["TeamCap"] = TempData["TeamCap"].ToString();
        AllThePokes page = new();

        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<yourPokemon> team;
        if (string.IsNullOrEmpty(teamJson))
        {
            team = new List<yourPokemon>();
        }
        else
        {
            team = JsonConvert.DeserializeObject<List<yourPokemon>>(teamJson);
            page.yourPokemon = team;
        }

        page.inPagePokemon = PokesList;

        return View("AllPokemon", page);
    }

    [HttpPost]
    public async Task<IActionResult> AddToTeam(int id, int pageId)
    {
        Pokemon poke = await _pokeClient.GetResourceAsync<Pokemon>(id);
        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<yourPokemon> team;
        if (string.IsNullOrEmpty(teamJson))
        {
            team = new List<yourPokemon>();
        }
        else
        {
            team = JsonConvert.DeserializeObject<List<yourPokemon>>(teamJson);
        }

        if (team.Count < 6)
        {
            yourPokemon yourPoke = new yourPokemon();
            yourPoke.pokemon = poke;
            yourPoke.ability = poke.Abilities.FirstOrDefault().Ability.Name;
            var rnd = new Random();
            if (rnd.Next(0, 100) == 1)
            {
                yourPoke.shiny = true;
            }
            else { yourPoke.shiny = false; }
            Dictionary<string, int> IVs = new Dictionary<string, int> {
            { "HP", rnd.Next(0,31) },
            { "Attack", rnd.Next(0,31) },
            { "Defense", rnd.Next(0,31) },
            { "Sp. Atk", rnd.Next(0,31) },
            { "Sp. Def", rnd.Next(0,31) },
            { "Speed", rnd.Next(0,31) }
        };
            yourPoke.IVs = IVs;
            yourPoke.teamPosition = team.Count;
            yourPoke.moves = new List<string> { "", "", "", "" };
            team.Add(yourPoke);
            _httpContextAccessor.HttpContext.Session.SetString("team", JsonConvert.SerializeObject(team));
        } else { TempData["TeamCap"] = "You can't add more than 6 Pokemon to your team"; }
        return RedirectToAction("GetPokemonData", new { id = pageId });
    }

    [HttpPost]
    public IActionResult RemoveFromTeam(int id, int pageId)
    {
        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<yourPokemon> team;
        team = JsonConvert.DeserializeObject<List<yourPokemon>>(teamJson);
        if (team.Count > id)
            team.Remove(team[id]);
        int i = 0;
        foreach (var pokemon in team)
        {
            team[i].teamPosition = i;
            i++;
        }
        _httpContextAccessor.HttpContext.Session.SetString("team", JsonConvert.SerializeObject(team));
        return RedirectToAction("GetPokemonData", new { id = pageId });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
