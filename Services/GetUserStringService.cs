using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorseCode.Services
{
    public static class GetUserStringService
    {
        public static string GetUserString()
        {
            Console.WriteLine("Enter text: ");
            string input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                input = input.ToLower();
            }

            return input;
        }
    }
}
