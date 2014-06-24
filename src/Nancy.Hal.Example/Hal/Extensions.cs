namespace Nancy.Hal.Example.Hal
{
    using System.Globalization;
    using System.Reflection;

    public static class Extensions
    {
        public static string ToCamelCaseString(this string input)
        {
            if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0])) return input;

            string lowerCasedFirstChar =
                char.ToLower(input[0], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

            if (input.Length > 1) lowerCasedFirstChar = lowerCasedFirstChar + input.Substring(1);

            return lowerCasedFirstChar;
        }

        
    }
}