using MorseCode.Models;
using MorseCode.Services;

namespace MorseCode.Menus
{
    public class AudioToText : Menu
    {
        public AudioToText(Menu parent)
        {
            Description = "Morse audio to text";

            Parent = parent;
        }

        public override void Action()
        {
            string[] alphabetData = File.ReadAllLines("symbols.txt");

            new Decoder().DecodeAsync("output.wav").GetAwaiter().GetResult();
        }
    }
}
