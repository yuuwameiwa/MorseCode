using MorseCode.Models;
using MorseCode.Services;
using MorseCode.Menus;

namespace MorseCode
{
    public class Program
    {
        public static void Main() 
        {
            new MainMenu().Action();

            /*string input = GetUserStringService.GetUserString();

            Dictionary<char, string> morseData = MorseDictionaryModel.MorseData;

            string morse = StringToMorseTranslationService.TranslateStringToMorse(input, morseData);

            Console.WriteLine(morse);

            MorseToAudioCreationService.CreateMorseAudio(morse);*/
        }
    }
}