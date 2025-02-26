using System.Globalization;
using System.Text.RegularExpressions;

namespace CookConsumer.Helpers{
    public static class FunctionHelpers{
        public static bool TryParseFraction(string input, out decimal result)
        {
            result = 0;

            // If it's just a normal number, parse it directly
            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                return true;
            }

            // Handle fraction (e.g., "3/4" → 0.75)
            if (Regex.IsMatch(input, @"^\d+/\d+$"))
            {
                var parts = input.Split('/');
                if (decimal.TryParse(parts[0], out var numerator) && decimal.TryParse(parts[1], out var denominator) && denominator != 0)
                {
                    result = numerator / denominator;
                    return true;
                }
            }

            // Handle mixed fraction (e.g., "1 1/2" → 1.5)
            if (Regex.IsMatch(input, @"^\d+\s\d+/\d+$"))
            {
                var parts = input.Split(' ');
                if (decimal.TryParse(parts[0], out var wholeNumber) && TryParseFraction(parts[1], out var fractionPart))
                {
                    result = wholeNumber + fractionPart;
                    return true;
                }
            }

            return false;
        }

    }
}
