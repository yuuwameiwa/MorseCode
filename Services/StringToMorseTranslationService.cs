using System.Text;

namespace MorseCode.Services
{
    public static class StringToMorseTranslationService
    {
        public static string TranslateStringToMorse(string userString, Dictionary<char, string> morseDictionary)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char character in userString)
            {
                if (morseDictionary.TryGetValue(character, out string? value))
                {
                    stringBuilder.Append(value + " ");
                }
            }

            return stringBuilder.ToString();
        }
    }
}