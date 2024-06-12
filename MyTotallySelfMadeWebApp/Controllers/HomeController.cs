using Microsoft.AspNetCore.Mvc;
using MyTotallySelfMadeWebApp.Models;
using System.Diagnostics;
using PokeApiNet;
using System.Drawing;
using Newtonsoft.Json;
using AppModels.Models;
using Microsoft.AspNetCore.Components;

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

    public IActionResult Index()
    {
        Dictionary<string, Color> TypeColors = new Dictionary<string, Color>
        {
            { "normal", System.Drawing.ColorTranslator.FromHtml("#a0a2a0") },
            { "fire", System.Drawing.ColorTranslator.FromHtml("#e72324") },
            { "water", System.Drawing.ColorTranslator.FromHtml("#2481ef") },
            { "electric", System.Drawing.ColorTranslator.FromHtml("#fac100") },
            { "grass", System.Drawing.ColorTranslator.FromHtml("#3da224") },
            { "ice", System.Drawing.ColorTranslator.FromHtml("#3dd9ff") },
            { "fighting", System.Drawing.ColorTranslator.FromHtml("#ff8100") },
            { "poison", System.Drawing.ColorTranslator.FromHtml("#913ecd") },
            { "ground", System.Drawing.ColorTranslator.FromHtml("#92501b") },
            { "flying", System.Drawing.ColorTranslator.FromHtml("#82baef") },
            { "psychic", System.Drawing.ColorTranslator.FromHtml("#ef3f7a") },
            { "bug", System.Drawing.ColorTranslator.FromHtml("#92a212") },
            { "rock", System.Drawing.ColorTranslator.FromHtml("#b0ab82") },
            { "ghost", System.Drawing.ColorTranslator.FromHtml("#713f71") },
            { "dragon", System.Drawing.ColorTranslator.FromHtml("#4f61e2") },
            { "dark", System.Drawing.ColorTranslator.FromHtml("#4f3f3d") },
            { "steel", System.Drawing.ColorTranslator.FromHtml("#60a2b9") },
            { "fairy", System.Drawing.ColorTranslator.FromHtml("#f171f1") }
        };
        var colorsJson = _httpContextAccessor.HttpContext.Session.GetString("colors");
        if (string.IsNullOrEmpty(colorsJson))
        {
            _httpContextAccessor.HttpContext.Session.SetString("colors", JsonConvert.SerializeObject(TypeColors));
        }
        return View();
    }

    public IActionResult Team()
    {
        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<yourPokemon> team;
        if (string.IsNullOrEmpty(teamJson))
        {
            team = new List<yourPokemon>();
            _httpContextAccessor.HttpContext.Session.SetString("team", JsonConvert.SerializeObject(team));
        }
        else
        {
            team = JsonConvert.DeserializeObject<List<yourPokemon>>(teamJson);
        }
        var TypeColors = _httpContextAccessor.HttpContext.Session.GetString("colors");
        ViewBag.TypeColors = JsonConvert.DeserializeObject<Dictionary<string, Color>>(TypeColors);
        return View(team);
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

        NamedApiResourceList<Pokemon> allPokes = await _pokeClient.GetNamedResourcePageAsync<Pokemon>(25, pagination);
        var tasks = allPokes.Results.Select(async namedResource =>
        {
            return await _pokeClient.GetResourceAsync<Pokemon>(namedResource.Name);
        }).ToList();
        var PokesList = (await Task.WhenAll(tasks)).ToList();

        var TypeColors = _httpContextAccessor.HttpContext.Session.GetString("colors");
        ViewBag.TypeColors = JsonConvert.DeserializeObject<Dictionary<string, Color>>(TypeColors);
        ViewData["id"] = id;
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
        }
        return RedirectToAction("GetPokemonData", new { id = pageId });
    }

    [HttpPost]
    public IActionResult RemoveFromTeam(int id, int pageId, string returnPage)
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
        switch (returnPage)
        {
            case "team":
                return RedirectToAction("Team");
            case "list":
                return RedirectToAction("GetPokemonData", new { id = pageId });
                break;
            case null:
                return RedirectToAction("GetPokemonData", new { id = pageId });
                break;

        }
        return RedirectToAction("GetPokemonData", new { id = pageId });
    }

    public IActionResult EditPoke(int id)
    {
        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<yourPokemon> yourPokes = JsonConvert.DeserializeObject<List<yourPokemon>>(teamJson);
        var TypeColors = _httpContextAccessor.HttpContext.Session.GetString("colors");
        ViewBag.TypeColors = JsonConvert.DeserializeObject<Dictionary<string, Color>>(TypeColors);
        return View(yourPokes[id]);
    }

    public IActionResult SaveEdit(yourPokemon model)
    {
        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<yourPokemon> yourPokes = JsonConvert.DeserializeObject<List<yourPokemon>>(teamJson);
        var selectedPoke = yourPokes[model.teamPosition];
        if (model.nickname == "")
        {
            selectedPoke.nickname = null;
        }
        else
        {
            selectedPoke.nickname = model.nickname;
        }
        if (model.moves.Count == 4)
        {
            for (int i = 0; i < 4; i++)
                selectedPoke.moves[i] = model.moves[i];
        }
        _httpContextAccessor.HttpContext.Session.SetString("team", JsonConvert.SerializeObject(yourPokes));
        return RedirectToAction("Team");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
