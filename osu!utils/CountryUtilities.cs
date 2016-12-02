using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Osu.Utils
{
    /// <summary>
    /// Some country utilities
    /// </summary>
    public class CountryUtilities
    {
        /// <summary>
        /// The countries ordered by codes
        /// </summary>
        protected static Dictionary<string, string> countries;

        /// <summary>
        /// The codes ordered by country
        /// </summary>
        protected static Dictionary<string, string> codes;

        /// <summary>
        /// Initializes the list
        /// </summary>
        public static void Initialize()
        {
            countries = JsonConvert.DeserializeObject<Dictionary<string, string>>(Osu.Utils.Properties.Resources.Countries);
            codes = countries.ToDictionary(entry => entry.Value, entry => entry.Key);
        }

        /// <summary>
        /// Returns a country name based on its code
        /// </summary>
        /// <param name="code">the country code</param>
        /// <returns>the country name or null if this country doesn't exist</returns>
        public static string GetCountry(string code)
        {
            if (countries.ContainsKey(code))
                return countries[code];
            else
                return null;
        }

        /// <summary>
        /// Returns a country code based on its name
        /// </summary>
        /// <param name="country">the country name</param>
        /// <returns>the country code or null if this country doesn't exist</returns>
        public static string GetCode(string country)
        {
            if (codes.ContainsKey(country))
                return codes[country];
            else
                return null;
        }
    }
}
