using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MorseCode.Services
{
    public class MorseToTextTranslationService
    {
        public static string TranslateMorseToText(string morseText, Dictionary<char, string> morseDictionary)
        {
            StringBuilder stringBuilder = new StringBuilder();

            string[] morseLetters = morseText.Split(" ");

            string decodedMorse = "";

            foreach (string word in morseLetters)
            {
                char letter = morseDictionary.FirstOrDefault(w => w.Value == word).Key;

                decodedMorse += letter;
            }

            return decodedMorse;
        }
    }
}
