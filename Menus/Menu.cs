using MorseCode.Managers;

namespace MorseCode.Menus
{
    public abstract class Menu
    {
        public string Description { get; private protected set; }
        public int OptionChoice { get; private protected set; }

        protected Menu[] ChildrenArray;

        public Menu Parent { get; set; }

        public Menu() { }

        public Menu(Menu parent)
        {
            Parent = parent;
        }

        public virtual void Action()
        {
            // Записать в массив названия меню
            string[] names = ChildrenArray.Select(child => child.Description).ToArray();

            MenuManager menuManager = new MenuManager(names);

            while (menuManager.OptionSelected == false)
            {
                // Распечатать названия меню
                menuManager.PrintOutputs();

                ConsoleKey key = Console.ReadKey(true).Key;

                // Получить выбор меню от пользователя
                OptionChoice = menuManager.HandleInput(key);
            }

            // Вызвать Action() у выбранной опции
            ChildrenArray[OptionChoice].Action();
        }

        public void GoBack(Menu parent)
        {
            Console.Clear();

            Parent = parent;
            Parent.Action();
        }
    }
}
