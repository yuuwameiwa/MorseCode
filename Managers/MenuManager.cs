namespace MorseCode.Managers
{
    /// <summary>
    /// Класс отвечает за:
    /// - Любой вывод чего-либо на экран 
    /// - Выбор пользователя какой-либо опции
    /// - Получение ввода пользователя с консоли
    /// </summary>
    public class MenuManager
    {
        private string[] Outputs;

        private int SelectedIdx;
        public bool OptionSelected { get; protected set; }

        public MenuManager(string[] outputs)
        {
            Outputs = outputs;
            SelectedIdx = 0;
            OptionSelected = false;
        }

        // Вывод на экрана Outputs
        public void PrintOutputs()
        {
            Console.CursorVisible = false;
            // Очистить меню.
            for (int i = 0; i < Console.WindowHeight; i++)
                for (int j = 0; j < Console.WindowWidth; j++)
                    Console.Write(' ');
            Console.CursorVisible = true;

            // Вывод на экран Output со стрелочкой
            foreach ((string output, int index) in Outputs.Select((value, index) => (value, index)))
            {
                Console.SetCursorPosition(0, index);

                if (index == SelectedIdx)
                    Console.WriteLine("> " + output.PadRight(50));
                else
                    Console.WriteLine(" " + output.PadRight(50));
            }
        }

        // Принять ввод от пользователя
        public int HandleInput(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (SelectedIdx > 0)
                        SelectedIdx--;
                    break;
                case ConsoleKey.DownArrow:
                    if (SelectedIdx < Outputs.Length - 1)
                        SelectedIdx++;
                    break;
                case ConsoleKey.Enter:
                    OptionSelected = true;
                    return SelectedIdx;
            }

            return SelectedIdx;
        }
    }
}
