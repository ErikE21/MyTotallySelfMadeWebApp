using PokeApiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AppModels.Models;
public class yourPokemon
{
    public Pokemon pokemon { get; set; }
    public string nickname { get; set; }
    public string ability { get; set; }
    public List<string> moves { get; set; }
    public void addMove(string move)
    {
        if (moves == null)
        {
            moves = new List<string> { move };
        }

        if (moves.Count >= 4)
        {
            moves.RemoveAt(0);
        }
        moves.Add(move);
    }
    public Dictionary<string, int> IVs { get; set; }
    public bool shiny { get; set; }
}
