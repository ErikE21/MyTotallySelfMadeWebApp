using PokeApiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppModels.Models;
public class AllThePokes
{
    public List<yourPokemon>? yourPokemon { get; set; }
    public List<Pokemon> inPagePokemon { get; set; }
}
