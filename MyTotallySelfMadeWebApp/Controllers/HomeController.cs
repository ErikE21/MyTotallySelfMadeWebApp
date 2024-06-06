using Microsoft.AspNetCore.Mvc;
using MyTotallySelfMadeWebApp.Models;
using System.Diagnostics;
using PokeApiNet;
using System.Collections.ObjectModel;
using System.Drawing;

namespace MyTotallySelfMadeWebApp.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PokeApiClient _pokeClient;
    public HomeController(ILogger<HomeController> logger, PokeApiClient pokeClient)
    {
        _logger = logger;
        _pokeClient = pokeClient;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> GetPokemonData()
    {
        NamedApiResourceList<Pokemon> allPokes = await _pokeClient.GetNamedResourcePageAsync<Pokemon>(1025, 0);
        var tasks = allPokes.Results.Select(async namedResource =>
        {
            return await _pokeClient.GetResourceAsync<Pokemon>(namedResource.Name);
        }).ToList();
        var allPokesList = (await Task.WhenAll(tasks)).ToList();

        Dictionary<string, Color> TypeColors = new Dictionary<string, Color>
        {
            { "normal", System.Drawing.ColorTranslator.FromHtml("#A8A77A") },
            { "fire", System.Drawing.ColorTranslator.FromHtml("#EE8130") },
            { "water", System.Drawing.ColorTranslator.FromHtml("#6390F0") },
            { "electric", System.Drawing.ColorTranslator.FromHtml("#F7D02C") },
            { "grass", System.Drawing.ColorTranslator.FromHtml("#7AC74C") },
            { "ice", System.Drawing.ColorTranslator.FromHtml("#96D9D6") },
            { "fighting", System.Drawing.ColorTranslator.FromHtml("#C22E28") },
            { "poison", System.Drawing.ColorTranslator.FromHtml("#A33EA1") },
            { "ground", System.Drawing.ColorTranslator.FromHtml("#E2BF65") },
            { "flying", System.Drawing.ColorTranslator.FromHtml("#A98FF3") },
            { "psychic", System.Drawing.ColorTranslator.FromHtml("#F95587") },
            { "bug", System.Drawing.ColorTranslator.FromHtml("#A6B91A") },
            { "rock", System.Drawing.ColorTranslator.FromHtml("#B6A136") },
            { "ghost", System.Drawing.ColorTranslator.FromHtml("#735797") },
            { "dragon", System.Drawing.ColorTranslator.FromHtml("#6F35FC") },
            { "dark", System.Drawing.ColorTranslator.FromHtml("#705746") },
            { "steel", System.Drawing.ColorTranslator.FromHtml("#B7B7CE") },
            { "fairy", System.Drawing.ColorTranslator.FromHtml("#D685AD") }
        };
        ViewBag.TypeColors = TypeColors;
        return View("AllPokemon", allPokesList);
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
