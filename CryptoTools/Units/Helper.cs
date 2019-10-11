using CryptoTools.API.Objects.Pairs;
using System.Collections.Generic;

namespace CryptoTools.Units
{
    public static class Helper
    {
        internal static string ToHttpPostString(this Dictionary<string, string> dictionary)
        {
            var output = string.Empty;
            foreach (var entry in dictionary)
            {
                var valueString = entry.Value as string;
                if (valueString == null)
                {
                    output += "&" + entry.Key + "=" + entry.Value;
                }
                else
                {
                    output += "&" + entry.Key + "=" + valueString.Replace(' ', '+');
                }
            }
            return output.Substring(1);
        }

        internal static double ToDouble(this string s, double defval)
        {
            double data;
            if (double.TryParse(s, out data))
                return data;
            else
                return defval;
        }

        internal static bool Supported(this Dictionary<int, ExchangePair> dictionary, int pairid)
        {
            if (dictionary.ContainsKey(pairid))
                if (dictionary[pairid].trade_supported)
                    return true;

            return false;
        }

        internal static string ToStringFull(this double value)
        {
            return value.ToString("0.####################");
        }
    }
}
