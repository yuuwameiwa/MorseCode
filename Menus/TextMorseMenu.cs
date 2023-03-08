using MorseCode.Models;
using MorseCode.Services;

namespace MorseCode.Menus
{
    public class TextMorseMenu : Menu
    {

        public TextMorseMenu(Menu parent)
        {
            Description = "Text to Morse";

            Parent = parent;
        }

        public override void Action()
        {
            string input = GetUserStringService.GetUserString();

            Dictionary<char, string> morseData = MorseDictionaryModel.MorseData;

            string morse = StringToMorseTranslationService.TranslateStringToMorse(input, morseData);

            using (StreamWriter writer = new StreamWriter(@"D:\projects\MorseCode\morse.txt", false))
            {
                writer.Write(morse);
            }
        }
    }
}
