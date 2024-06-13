using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AppModels.Models;
public class StartPageFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var colorsSession = context.HttpContext.Session.GetString("colors");
        if (!string.IsNullOrEmpty(colorsSession))
        {
            context.Result = new RedirectToActionResult("GetPokemonData", "Home", 1);
        }
    }
}