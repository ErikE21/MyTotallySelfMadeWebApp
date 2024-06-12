using PokeApiNet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AppModels.Models;
public class yourPokemon
{

    public Pokemon pokemon { get; set; }

    [StringLength(16, ErrorMessage = "Max {1} characters for their {0}")]
    public string nickname { get; set; }
    public string ability { get; set; }
    public List<string> moves { get; set; }
    public Dictionary<string, int> IVs { get; set; }
    public bool shiny { get; set; }
    public int teamPosition { get; set; }
}
