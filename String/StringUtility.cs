using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace A_Common_Library.String
{
    public static class StringUtility
    {

        #region Validation and sanitation

        public static string Sanitize(this string me)
        {
            return new string(me.Where(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)).ToArray());
        }

        //useful resource http://www.regular-expressions.info/quickstart.html
        public static bool ValidateEmail(this string text)
        {
            var reg = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}$", RegexOptions.IgnoreCase);
            if (reg.IsMatch(text)) return true;

            return false;
        }

        //dd-mm-yyyy
        //https://ihateregex.io/expr/date/
        public static bool ValidateDate(this string text)
        {
            //var reg = new Regex(@"\b(((0?[469]|11)/(0?[1-9]|[12]\d|30)|(0?[13578]|1[02])/(0?[1-9]|[12]\d|3[01])|0?2/(0?[1-9]|1\d|2[0-8]))/([1-9]\d{3}|\d{2})|0?2/29/([1-9]\d)?([02468][048]|[13579][26]))\b$",
            //    RegexOptions.ECMAScript | RegexOptions.ExplicitCapture);
            //var reg = new Regex(@"^(0[1-9]|[12] [0-9]|3[01])([- /.])(0[1-9]|1[012])\2(19|20)\d\d$", RegexOptions.IgnoreCase);
            if (text.NotEmpty())
            {
                var reg = new Regex(@"(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[13-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})");
                if (reg.IsMatch(text)) return true;
            }
            return false;
        }

        public static bool ValidateNoSpecial(this string text)
        {
            if (text.NotEmpty())
            {
                Regex reg = new Regex(@"^[a-zA-Z0-9]*[^!@%~?:#$%^&*()0'][-]*$");
                if (reg.IsMatch(text)) return true;
            }

            return false;
        }

        public static bool ValidateAllowSpace(this string text)
        {
            if (text.NotEmpty())
            {
                Regex reg = new Regex(@"[^\w\s]");
                if (reg.IsMatch(text)) return true;
            }

            return false;
        }

        public static bool ValidateDatabaseName(this string text)
        {
            if (text.NotEmpty())
            {
                Regex reg = new Regex(@"^[a-zA-Z0-9_]*[^!@%~?:#$%^&*()0'][-]*$");
                if (reg.IsMatch(text)) return true;
            }

            return false;
        }

        public static bool ValidatePassword(this string text)
        {
            if (text.NotEmpty())
            {
                Regex reg = new Regex(@"^[a-zA-Z0-9!]*[^_@%~?:#$%^&*()0'][-]*$");
                if (reg.IsMatch(text)) return true;
            }

            return false;
        }

        #endregion //Validation and sanitation

        public static bool NotEmpty(this string me)
        {
            return (me != null && me.GetType() == typeof(string) && me.Length > 0);
        }

        public static int CountOccurringPattern(this string Me, string Pattern)
        {
            int count = -1;
            int index = 0;

            while (true)
            {
                count++;
                index = Me.IndexOf(Pattern, index);
                if (index == -1) break;
                index += Pattern.Length;
            }

            return count;
        }

        public static string MaxLength(this string word, int length)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;

            if (length <= 0) return word;

            if (word.Trim().Length > length) return word.Trim().Substring(0, length);

            return word;
        }

        public static string StreamToString(this Stream me)
        {
            using (StreamReader reader = new StreamReader(me))
            {
                return reader.ReadToEnd();
            }
        }

        public static int CompareConfidenceTo(this string sentence, string another_sentence)
        {
            if (string.IsNullOrEmpty(sentence) || string.IsNullOrEmpty(another_sentence)) return 0;

            int confidence = 0;
            string[] words = sentence.ToLower()
                .SanitizeToAlphaNumeric(" ")
                .Split(' ');

            another_sentence = another_sentence.ToLower()
                .SanitizeToAlphaNumeric(" ");

            return words.AsEnumerable()
                .Where(word => another_sentence.Contains(word)
                        || another_sentence.CompareTo(word) >0)
                .Count();
        }

        public static string SanitizeToNumeric(this string word, string allowed_special_chars = "", int max_length = -1)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;

            string new_word = new string(word.AsEnumerable<char>()
                .Where(c => char.IsDigit(c) || allowed_special_chars.Contains(c))
                .ToArray());

            if (max_length <= 0)
            {
                return new_word;
            }

            return new_word.Length > max_length ? new_word.Substring(0, max_length) : new_word;
        }

        public static string SanitizeToAlphaNumeric(this string word, string allowed_special_chars = "", int max_length = -1)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;

            string new_word = new string(word.AsEnumerable<char>()
                .Where(c => char.IsLetterOrDigit(c) || allowed_special_chars.Contains(c))
                .ToArray());

            if (max_length <= 0)
            {
                return new_word;
            }

            return new_word.Length > max_length ? new_word.Substring(0, max_length) : new_word;
        }

        public static DateTime ToDate(this string test_string)
        {
            if (string.IsNullOrEmpty(test_string)) return DateTime.MinValue;

            try
            {
                if (DateTime.TryParse(test_string, out DateTime legal_var))
                {
                    return legal_var;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static decimal ValidateDecimal(this string test_string, int precision, int scale, decimal default_value = default(decimal))
        {
            if (string.IsNullOrEmpty(test_string))
            {
                return default_value;
            }

            string test_decimal = test_string.SanitizeToAlphaNumeric("-.");

            string[] nums = test_decimal.Replace("-", "").Split('.');

            if (nums[0].Length > (precision - scale)
                && nums[1].Length > scale)
            {
                return default_value;
            }

            return test_decimal.ToDecimal(default_value);
        }

        public static decimal? ValidateDecimalNullable(this string test_string, int precision, int scale, decimal? default_value = null)
        {
            if (string.IsNullOrEmpty(test_string))
            {
                return default_value;
            }

            string test_decimal = test_string.SanitizeToAlphaNumeric("-.");

            string[] nums = test_decimal.Replace("-", "").Split('.');

            if (nums[0].Length > (precision - scale)
                && nums[1].Length > scale)
            {
                return default_value;
            }

            return test_decimal.ToDecimalNullable(default_value);
        }

        public static decimal? ToDecimalNullable(this string test_string, decimal? default_value = null)
        {
            if (string.IsNullOrEmpty(test_string)) return default_value;

            try
            {
                if (decimal.TryParse(test_string, out decimal legal_var))
                {
                    return legal_var;
                }
                else
                {
                    return default_value;
                }
            }
            catch
            {
                return default_value;
            }
        }

        public static decimal ToDecimal(this string test_string, decimal default_value = default(decimal))
        {
            if (string.IsNullOrEmpty(test_string)) return default_value;

            try
            {
                if (decimal.TryParse(test_string, out decimal legal_var))
                {
                    return legal_var;
                }
                else
                {
                    return default_value;
                }
            }
            catch
            {
                return default_value;
            }
        }

        public static int ToInt(this string test_string, int default_value = default(int))
        {
            if (string.IsNullOrEmpty(test_string)) return default_value;

            try
            {
                if (int.TryParse(test_string, out int legal_var))
                {
                    return legal_var;
                }
                else
                {
                    return default_value;
                }
            }
            catch
            {
                return default_value;
            }
        }

        public static double ToDouble(this string test_string, double default_value = default(double))
        {
            if (string.IsNullOrEmpty(test_string)) return default_value;

            try
            {
                if (double.TryParse(test_string, out double legal_var))
                {
                    return legal_var;
                }
                else
                {
                    return default_value;
                }
            }
            catch
            {
                return default_value;
            }
        }
    }
}