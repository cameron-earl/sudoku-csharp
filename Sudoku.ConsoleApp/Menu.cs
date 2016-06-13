using System;
using System.Text.RegularExpressions;
using static System.Console;

namespace Sudoku.ConsoleApp
{
    public class Menu
    {
        /// <summary>
        ///     Requests integer input from user between min and max
        /// </summary>
        /// <param name="min">Minimum acceptable return value</param>
        /// <param name="max">Maximum acceptable return value</param>
        /// <returns></returns>
        internal static int GetIntInput(int min, int max)
        {
            var input = int.MinValue;

            do
            {
                WriteLine($"Please enter a number between {min} and {max}.");
                var readLine = ReadLine();
                if (readLine == null) continue;
                readLine = new Regex("[\\D]").Replace(readLine, "");
                if (readLine != "") input = int.Parse(readLine);
            } while (input < min || input > max);
            return input;
        }

        /// <summary>
        ///     Queries user for character until valid character is entered.
        ///     Not case-sensitive. All answers converted to lowercase.
        /// </summary>
        /// <param name="r">A Regex representing all valid lowercase character choices</param>
        /// <returns></returns>
        internal static char GetCharacterInput(Regex r)
        {
            var hasValidInput = false;
            char choice;
            do
            {
                choice = (char) Read();
                ReadLine(); // clear input stream
                choice = char.ToLower(choice);
                if (r.IsMatch(choice.ToString()))
                {
                    hasValidInput = true;
                }
                else
                {
                    WriteLine("Input invalid, please enter one of the following:");
                    var count = 0;
                    for (var c = '0'; c < 'z'; c++)
                    {
                        if (!r.IsMatch($"{c}")) continue;
                        if (count > 0)
                        {
                            Write(", ");
                        }
                        Write(c);
                        count++;
                    }
                    WriteLine();
                }
            } while (!hasValidInput);
            return choice;
        }

        /// <summary>
        ///     Obtains yes or no input (true or false) from user
        /// </summary>
        /// <returns>boolean signifying a yes or a no</returns>
        internal static bool GetYesNoInput()
        {
            WriteLine("Enter y or n.");
            var answer = GetCharacterInput(new Regex("[ny]"));
            return answer == 'y';
        }

        internal static void PrintHeading(string heading)
        {
            ForegroundColor = ConsoleColor.White;
            WriteLine(heading);
            ResetColor();
        }
    }
}
