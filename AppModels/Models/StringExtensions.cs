using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppModels.Models;
public static class StringExtensions
{
    public static string Capitalize(this string input)
    {
        return input[0].ToString().ToUpper() + input.Substring(1);
    }
}
