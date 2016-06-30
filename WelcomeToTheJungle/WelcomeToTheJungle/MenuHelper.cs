using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace WelcomeToTheJungle
{
    static class MenuHelper
    {
        public static void CreateCheckBox(this Menu m, string displayName, string uniqueId, bool defaultValue = true)
        {
            try
            {
                m.Add(uniqueId, new CheckBox(displayName, defaultValue));
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error creating the checkbox with the uniqueID = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(uniqueId);
                Console.ResetColor();
            }
        }

        public static void CreateSlider(this Menu m, string displayName, string uniqueId, int defaultValue = 0, int minValue = 0, int maxValue = 100)
        {
            try
            {
                m.Add(uniqueId, new Slider(displayName, defaultValue, minValue, maxValue));
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error creating the slider with the uniqueID = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(uniqueId);
                Console.ResetColor();
            }
        }

        public static void CreateComboBox(this Menu m, string displayName, string uniqueId, List<string> options, int defaultValue = 0)
        {
            try
            {
                m.Add(uniqueId, new ComboBox(displayName, options, defaultValue));
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error creating the combobox with the uniqueID = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(uniqueId);
                Console.ResetColor();
            }
        }

        public static void CreateKeyBind(this Menu m, string displayName, string uniqueId, uint defaultKey1, uint defaultKey2,
            KeyBind.BindTypes bindtype = KeyBind.BindTypes.PressToggle, bool defaultValue = true)
        {
            try
            {
                m.Add(uniqueId, new KeyBind(displayName, defaultValue, bindtype, defaultKey1, defaultKey2));
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error creating the checkbox with the uniqueID = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(uniqueId);
                Console.ResetColor();
            }
        }

        public static bool GetCheckBoxValue(this Menu m, string uniqueId)
        {
            try
            {
                return m.Get<CheckBox>(uniqueId).CurrentValue;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error getting the checkbox with the uniqueID = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(uniqueId);
                Console.ResetColor();
            }
            return false;
        }

        public static int GetSliderValue(this Menu m, string uniqueId)
        {
            try
            {
                return m.Get<Slider>(uniqueId).CurrentValue;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error getting the slider with the uniqueID = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(uniqueId);
                Console.ResetColor();
            }
            return -1;
        }

        public static int GetComboBoxValue(this Menu m, string uniqueId)
        {
            try
            {
                return m.Get<ComboBox>(uniqueId).CurrentValue;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error getting the combobox with the uniqueID = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(uniqueId);
                Console.ResetColor();
            }
            return -1;
        }

        public static bool GetKeyBindValue(this Menu m, string uniqueId)
        {
            try
            {
                return m.Get<KeyBind>(uniqueId).CurrentValue;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Error getting the keybind with the uniqueID = ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(uniqueId);
                Console.ResetColor();
            }
            return false;
        }
    }
}