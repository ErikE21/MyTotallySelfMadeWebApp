using Microsoft.AspNetCore.Mvc;
using MyTotallySelfMadeWebApp.Models;
using System.Diagnostics;
using PokeApiNet;
using System.Collections.ObjectModel;
using System.Drawing;
using Newtonsoft.Json;

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

        NamedApiResourceList<Pokemon> allPokes = await _pokeClient.GetNamedResourcePageAsync<Pokemon>(25, pagination);
        var tasks = allPokes.Results.Select(async namedResource =>
        {
            return await _pokeClient.GetResourceAsync<Pokemon>(namedResource.Name);
        }).ToList();
        var PokesList = (await Task.WhenAll(tasks)).ToList();

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
        ViewBag.TypeColors = TypeColors;
        ViewData["id"] = id;

        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<Pokemon> team;
        if (string.IsNullOrEmpty(teamJson))
        {
            team = new List<Pokemon>();
        }
        else
        {
            team = JsonConvert.DeserializeObject<List<Pokemon>>(teamJson);
        }
        ViewBag.Team = team;

        return View("AllPokemon", PokesList);
    }

    [HttpPost]
    public async Task<IActionResult> AddToTeam(int id, int pageId)
    {
        Pokemon poke = await _pokeClient.GetResourceAsync<Pokemon>(id);
        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<Pokemon> team;
        if (string.IsNullOrEmpty(teamJson))
        {
            team = new List<Pokemon>();
        }
        else
        {
            team = JsonConvert.DeserializeObject<List<Pokemon>>(teamJson);
        }

        if (team.Count < 6)
        {
            team.Add(poke);
            _httpContextAccessor.HttpContext.Session.SetString("team", JsonConvert.SerializeObject(team));
        }
        return RedirectToAction("GetPokemonData", new { id = pageId });
    }

    [HttpPost]
    public IActionResult RemoveFromTeam(int id, int pageId)
    {
        var teamJson = _httpContextAccessor.HttpContext.Session.GetString("team");
        List<Pokemon> team;
        team = JsonConvert.DeserializeObject<List<Pokemon>>(teamJson);
        team.Remove(team[id]);
        _httpContextAccessor.HttpContext.Session.SetString("team", JsonConvert.SerializeObject(team));
        return RedirectToAction("GetPokemonData", new { id = pageId });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
