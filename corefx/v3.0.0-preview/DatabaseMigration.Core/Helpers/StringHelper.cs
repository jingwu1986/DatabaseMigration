using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class StringHelper
    {
        public static bool Convert2Bool(string value)
        {
            string upperValue = value?.ToUpper();
            return upperValue == "1" || upperValue == "TRUE" || upperValue == "YES" || upperValue=="Y";
        }

        public static int? Convert2Int(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return default(int?);
            }

            return Convert.ToInt32(value);
        }

        public static long? Convert2Long(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(long?);
            }

            return Convert.ToInt64(value);
        }

        public static string GetSingleQuotedString(params string[] values)
        {
            if(values!=null)
            {
                return string.Join(",", values.Select(item => $"'{item}'"));
            }
            return null;
        }

        public static string RemoveEmoji(string str)
        {
            return Regex.Replace(str, @"\p{Cs}", "");
        }

        public static string RawToGuid(string text)
        {
            byte[] bytes = ParseHex(text);
            Guid guid = new Guid(bytes);
            return guid.ToString("N").ToUpperInvariant();
        }

        public static string GuidToRaw(string text)
        {
            Guid guid = new Guid(text);
            return BitConverter.ToString(guid.ToByteArray()).Replace("-", "");
        }

        public static byte[] ParseHex(string text)
        {           
            byte[] ret = new byte[text.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
            }
            return ret;
        }
    }
}
