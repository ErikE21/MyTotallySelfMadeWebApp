using AppModels.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;

namespace MyTotallySelfMadeWebApp.Controllers;
public class TeamController : Controller
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TeamController (IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Index()
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

    [HttpPost]
    public IActionResult RemoveFromTeam(int id)
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
        return RedirectToAction("Index");
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
        return RedirectToAction("Index");
    }
}
