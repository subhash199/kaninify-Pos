using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Services
{
    public class GeneralServices
    {

        public bool IsAllDigit(string text)
        {
            return text.All(char.IsDigit);
        }

        public DateTime GetDefaultExpiryDate()
        {
            return DateTime.UtcNow.AddDays(30);
        }

        public decimal ConvertStringToDecimal(string input)
        {
            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits))
            {
                return 0;
            }

            // Ensure at least 3 digits so we can insert the decimal before the last two
            digits = digits.PadLeft(3, '0');

            // Insert '.' before last two digits (cents)
            var withDot = digits.Insert(digits.Length - 2, ".");

            if (decimal.TryParse(withDot, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return 0;
        }

        public int RoundUpToNearestFive(int number)
        {
            if (number > 0)
            {
                return ((number + 4) / 5) * 5; // Round up to the nearest multiple of 5

            }
            return 1;

        }

        public string CapitalizeWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input; // Return input if it's null or whitespace

            // Split the input into words by whitespace
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (char.IsLetter(words[i][0])) // Only capitalize if the first character is a letter
                {
                    words[i] = char.ToUpper(words[i][0], CultureInfo.InvariantCulture) +
                               words[i].Substring(1).ToLower(CultureInfo.InvariantCulture);
                }
                // Numbers or symbols are left untouched
            }

            // Join the words back into a single string
            return string.Join(' ', words);
        }
    }
}
