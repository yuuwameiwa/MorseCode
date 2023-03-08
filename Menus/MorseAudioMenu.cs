using MorseCode.Models;
using MorseCode.Services;
using NAudio.SoundFont;

namespace MorseCode.Menus
{
    public class MorseAudioMenu : Menu
    {
        public MorseAudioMenu(Menu parent)
        {
            Description = "Text to Audio Morse";

            Parent = parent;
        }

        public override void Action()
        {
            string[] alphabetData = File.ReadAllLines(@"D:\projects\MorseCode\symbols.txt");

            string input = GetUserStringService.GetUserString();

            Dictionary<char, string> morseData = MorseDictionaryModel.MorseData;

            string morse = StringToMorseTranslationService.TranslateStringToMorse(input, morseData);

            string strData = morse.Trim().ToUpper();

            MorseToAudioCreationService generator = new MorseToAudioCreationService(alphabetData, strData, "output.wav");

            (WaveHeaderChunk header, WaveFormatChunk format, WaveDataChunk data) = generator.GenerateAudio();
            generator.WriteWavefile(header, format, data);
        }
    }
}
