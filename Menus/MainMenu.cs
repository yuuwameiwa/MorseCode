using MorseCode.Menus;

namespace MorseCode.Menus
{
    public class MainMenu : Menu
    {
        public MainMenu()
        {
            Description = "Main menu";

            ChildrenArray = new Menu[]
            {
                new TextMorseMenu(this),
                new MorseAudioMenu(this),
                new AudioToText(this),
            };
        }
    }
}
