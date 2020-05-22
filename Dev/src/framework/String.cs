using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
    public class String
    {
        public static string ToUrl(string str)
        {
            return RemoveConsecutiveSpace(
                RemoveAccents(
                    ReplaceSpecialCaracters(WebUtility.HtmlDecode(str), '-')).ToLower(), new char[] { '-' });
        }

        /// <summary>
        /// Formater en une string utilisable dans une url.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string FormatForUrl(string str)
        {
            try
            {
                if (str == null)
                {
                    return string.Empty;
                }
                return RemoveAccents(ReplaceSpecialCaracters(str, '-')).ToLower();
            }
            catch
            {
                return str;
            }
        }

        public const char Separateur = ',';


        //***************************************************
        // Description :
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string trimStartEnd(string line, char[] charTrims)
        {
            try
            {
                return (line.TrimStart(charTrims)).TrimEnd(charTrims);
            }
            catch //(Exception e)
            {
                return "";
            }
        }

        //***************************************************
        // Description :
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string formatUrlString(string url)
        {
            try
            {
                if (url == null)
                    return "";
                return url.Replace("\\", "/");
            }
            catch //(Exception e)
            {
                return "";
            }
        }

        //***************************************************
        // Description :
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string reSize(string str, int intSize)
        {
            try
            {
                //Verification...
                if (str == null)
                    return "";
                if (str.Length <= intSize)
                    return str;
                return str.Substring(0, intSize) + "...";
            }
            catch //(Exception e)
            {
                return "";
            }
        }

        //***************************************************
        // Description :
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string toStrg(string[] strTab)
        {
            try
            {
                //Verification...
                if (strTab == null)
                    return null;
                string strg = "";
                foreach (string strElemt in strTab)
                    strg += (strElemt + Separateur);
                return strg;
            }
            catch //(Exception e)
            {
                return null;
            }
        }
        public static string toStrg(List<int> intTab)
        {
            try
            {
                //Verification...
                if (intTab == null)
                    return null;
                string strg = "";
                foreach (int strElemt in intTab)
                    strg += (strElemt.ToString() + Separateur);
                return strg;
            }
            catch //(Exception e)
            {
                return null;
            }
        }

        //***************************************************
        // Description :
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string[] toStrgArray(string str)
        {
            try
            {
                //Verification...
                if (str == null)
                    return null;
                return str.Split(Separateur);
            }
            catch //(Exception e)
            {
                return null;
            }
        }

        //***************************************************
        // Description :
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static List<int> toIntArray(string str)
        {
            List<int> intArry = null;
            try
            {
                string[] strngs = str.Split(Separateur);
                //Verification...
                if (strngs == null)
                    return null;
                intArry = new List<int>();
                foreach (string strng in strngs)
                {
                    try
                    {
                        intArry.Add(Convert.ToInt32(strng));
                    }
                    catch { }
                }
                return intArry;
            }
            catch //(Exception e)
            {
                return intArry;
            }
        }

        /// <summary>
        /// Retirer les caracteres accentué...
        /// </summary>
        /// <param name="sToClean"></param>
        /// <returns></returns>
        public static string RemoveAccents(string sToClean)
        {
            StringBuilder sOutString = new StringBuilder();

            // Vérification...
            if (string.IsNullOrEmpty(sToClean) == true)
            {
                return string.Empty;
            }

            sToClean = WebUtility.HtmlDecode(sToClean);
            for (int i = 0; i < sToClean.Length; i += 1)
            {
                Char c = sToClean[i];
                switch (c)
                {
                    case 'Ä':
                    case 'Â':
                    case 'À':
                    case 'Ã':
                        c = 'A';
                        break;

                    case 'Ë':
                    case 'Ê':
                    case 'È':
                    case 'É':
                        c = 'E';
                        break;

                    case 'Ü':
                    case 'Û':
                    case 'Ù':
                        c = 'U';
                        break;

                    case 'Ï':
                    case 'Î':
                    case 'Ì':
                        c = 'I';
                        break;

                    case 'Ö':
                    case 'Ô':
                    case 'Ò':
                    case 'Õ':
                        c = 'O';
                        break;

                    case 'Ñ':
                        c = 'N';
                        break;

                    case 'Ç':
                        c = 'c';
                        break;

                    case 'ä':
                    case 'â':
                    case 'à':
                    case 'ã':
                        c = 'a';
                        break;

                    case 'ë':
                    case 'ê':
                    case 'è':
                    case 'é':
                        c = 'e';
                        break;

                    case 'ü':
                    case 'û':
                    case 'ù':
                        c = 'u';
                        break;

                    case 'î':
                    case 'ï':
                    case 'ì':
                        c = 'i';
                        break;

                    case 'ö':
                    case 'ô':
                    case 'ò':
                    case 'õ':
                        c = 'o';
                        break;

                    case 'ç':
                        c = 'c';
                        break;

                    case 'ñ':
                        c = 'n';
                        break;

                    default:
                        break;
                }

                sOutString.Append(c);
            }

            return sOutString.ToString();
        }

        /// <summary>
        /// Decomposer sous la forme d'un meta tag keyword...
        /// </summary>
        /// <param name="sToClean"></param>
        /// <returns></returns>
        public static string AsKeyWord(string sToClean)
        {
            int iVal;
            try
            {
                // Vérification...
                if (string.IsNullOrEmpty(sToClean) == true)
                {
                    return string.Empty;
                }

                bool bFirst = true;
                string sOutput = string.Empty;
                string[] sKyWrds = RemoveSpecialCaracters(sToClean).Split(' ');
                foreach (string sKyWrd in sKyWrds)
                {
                    if (sKyWrd != null && sKyWrd.Length > 3 && int.TryParse(sKyWrd, out iVal) == false)
                    {
                        sOutput += ((bFirst == true) ? string.Empty : ", ") + sKyWrd.ToLower();
                        bFirst = false;
                    }
                }

                return sOutput;
            }
            catch //(Exception e)
            {
                return "";
            }
        }

        /// <summary>
        /// Retirer les caracteres spéciaux...
        /// </summary>
        /// <param name="sToClean"></param>
        /// <returns></returns>
        public static string RemoveSpecialCaracters(string sToClean)
        {
            return ReplaceSpecialCaracters(sToClean, null);
        }

        /// <summary>
        /// Remplacer les caracteres spéciaux...
        /// </summary>
        /// <param name="sToClean"></param>
        /// <param name="cNewChar"></param>
        /// <returns></returns>
        public static string ReplaceSpecialCaracters(string sToClean, char? cChar)
        {
            try
            {
                // Vérification...
                if (string.IsNullOrEmpty(sToClean) == true)
                {
                    return string.Empty;
                }

                StringBuilder sOutString = new StringBuilder();
                string sNormalized = WebUtility.HtmlDecode(sToClean);//.Normalize(NormalizationForm.FormD);
                                                                      /*string sNormalizedC = HttpUtility.HtmlDecode(sToClean).Normalize(NormalizationForm.FormC);
                                                                      string sNormalizedKC = HttpUtility.HtmlDecode(sToClean).Normalize(NormalizationForm.FormKC);
                                                                      string sNormalizedKD = HttpUtility.HtmlDecode(sToClean).Normalize(NormalizationForm.FormKD);*/
                for (int i = 0; i < sNormalized.Length; i += 1)
                {
                    Char c = sNormalized[i];
                    UnicodeCategory ucat = CharUnicodeInfo.GetUnicodeCategory(c);
                    switch (ucat)
                    {
                        //     Indicates that the character is an uppercase letter. Signified by the Unicode
                        //     designation "Lu" (letter, uppercase). The value is 0.
                        case UnicodeCategory.UppercaseLetter:
                        //     Indicates that the character is a lowercase letter. Signified by the Unicode
                        //     designation "Ll" (letter, lowercase). The value is 1.
                        case UnicodeCategory.LowercaseLetter:
                        //     Indicates that the character is a titlecase letter. Signified by the Unicode
                        //     designation "Lt" (letter, titlecase). The value is 2.
                        case UnicodeCategory.TitlecaseLetter:
                        //     Indicates that the character is a decimal digit, that is, in the range 0
                        //     through 9. Signified by the Unicode designation "Nd" (number, decimal digit).
                        //     The value is 8.
                        case UnicodeCategory.DecimalDigitNumber:
                        //     Indicates that the character is a number represented by a letter, instead
                        //     of a decimal digit, for example, the Roman numeral for five, which is "V".
                        //     The indicator is signified by the Unicode designation "Nl" (number, letter).
                        //     The value is 9.
                        case UnicodeCategory.LetterNumber:
                        //     Indicates that the character is a number that is neither a decimal digit
                        //     nor a letter number, for example, the fraction 1/2. The indicator is signified
                        //     by the Unicode designation "No" (number, other). The value is 10.
                        case UnicodeCategory.OtherNumber:
                            if ((cChar != null) || (i == (sNormalized.Length - 1)) || ((i + 1) < sNormalized.Length && sNormalized[i + 1] != '\''))
                            {
                                sOutString.Append(c);
                            }
                            break;

                        //     Indicates that the character is a space character, which has no glyph but
                        //     is not a control or format character. Signified by the Unicode designation
                        //     "Zs" (separator, space). The value is 11.
                        case UnicodeCategory.SpaceSeparator:
                        //     Indicates that the character is used to separate lines of text. Signified
                        //     by the Unicode designation "Zl" (separator, line). The value is 12.
                        case UnicodeCategory.LineSeparator:
                        //     Indicates that the character is used to separate paragraphs. Signified by
                        //     the Unicode designation "Zp" (separator, paragraph). The value is 13.
                        case UnicodeCategory.ParagraphSeparator:
                            //     Indicates that the character is a control code, with a Unicode value of U+007F
                            //     or in the range U+0000 through U+001F or U+0080 through U+009F. Signified
                            //     by the Unicode designation "Cc" (other, control). The value is 14.
                            if (cChar != null)
                            {
                                sOutString.Append(cChar);
                            }
                            else
                            {
                                sOutString.Append(' ');
                            }
                            break;

                        case UnicodeCategory.Control:
                        //     Indicates that the character is a format character, which is not normally
                        //     rendered but affects the layout of text or the operation of text processes.
                        //     Signified by the Unicode designation "Cf" (other, format). The value is 15.
                        case UnicodeCategory.Format:
                        //     Indicates that the character is a high surrogate or a low surrogate. Surrogate
                        //     code values are in the range U+D800 through U+DFFF. Signified by the Unicode
                        //     designation "Cs" (other, surrogate). The value is 16.
                        case UnicodeCategory.Surrogate:
                        //     Indicates that the character is a private-use character, with a Unicode value
                        //     in the range U+E000 through U+F8FF. Signified by the Unicode designation
                        //     "Co" (other, private use). The value is 17.
                        case UnicodeCategory.PrivateUse:
                        //     Indicates that the character is a connector punctuation, which connects two
                        //     characters. Signified by the Unicode designation "Pc" (punctuation, connector).
                        //     The value is 18.
                        case UnicodeCategory.ConnectorPunctuation:
                        //     Indicates that the character is a dash or a hyphen. Signified by the Unicode
                        //     designation "Pd" (punctuation, dash). The value is 19.
                        case UnicodeCategory.DashPunctuation:
                        //     Indicates that the character is the opening character of one of the paired
                        //     punctuation marks, such as parentheses, square brackets, and braces. Signified
                        //     by the Unicode designation "Ps" (punctuation, open). The value is 20.
                        case UnicodeCategory.OpenPunctuation:
                        //     Indicates that the character is the closing character of one of the paired
                        //     punctuation marks, such as parentheses, square brackets, and braces. Signified
                        //     by the Unicode designation "Pe" (punctuation, close). The value is 21.
                        case UnicodeCategory.ClosePunctuation:
                        //     Indicates that the character is an opening or initial quotation mark. Signified
                        //     by the Unicode designation "Pi" (punctuation, initial quote). The value is
                        //     22.
                        case UnicodeCategory.InitialQuotePunctuation:
                        //     Indicates that the character is a closing or final quotation mark. Signified
                        //     by the Unicode designation "Pf" (punctuation, final quote). The value is
                        //     23.
                        case UnicodeCategory.FinalQuotePunctuation:
                        //     Indicates that the character is a punctuation that is not a connector punctuation,
                        //     a dash punctuation, an open punctuation, a close punctuation, an initial
                        //     quote punctuation, or a final quote punctuation. Signified by the Unicode
                        //     designation "Po" (punctuation, other). The value is 24.
                        case UnicodeCategory.OtherPunctuation:
                        //     Indicates that the character is a mathematical symbol, such as "+" or "=
                        //     ". Signified by the Unicode designation "Sm" (symbol, math). The value is
                        //     25.
                        case UnicodeCategory.MathSymbol:
                        //     Indicates that the character is a currency symbol. Signified by the Unicode
                        //     designation "Sc" (symbol, currency). The value is 26.
                        case UnicodeCategory.CurrencySymbol:
                        //     Indicates that the character is a modifier symbol, which indicates modifications
                        //     of surrounding characters. For example, the fraction slash indicates that
                        //     the number to the left is the numerator and the number to the right is the
                        //     denominator. The indicator is signified by the Unicode designation "Sk" (symbol,
                        //     modifier). The value is 27.
                        case UnicodeCategory.ModifierSymbol:
                        //     Indicates that the character is a symbol that is not a mathematical symbol,
                        //     a currency symbol or a modifier symbol. Signified by the Unicode designation
                        //     "So" (symbol, other). The value is 28.
                        case UnicodeCategory.OtherSymbol:
                        //     Indicates that the character is not assigned to any Unicode category. Signified
                        //     by the Unicode designation "Cn" (other, not assigned). The value is 29.
                        case UnicodeCategory.OtherNotAssigned:
                        default:
                            if (cChar != null)
                            {
                                sOutString.Append(cChar);
                            }
                            break;
                    }
                }

                return sOutString.ToString();
            }
            catch //(Exception e)
            {
                return "";
            }
        }

        /// <summary>
        /// Retirer les espace consecutif...
        /// </summary>
        /// <param name="sToClean"></param>
        /// <returns></returns>
        public static string RemoveConsecutiveSpace(string sToClean)
        {
            return RemoveConsecutiveSpace(sToClean, new char[] { ' ', '\t', '\r', '\n' });
        }

        /// <summary>
        /// Retirer les caracters consecutif...
        /// </summary>
        /// <param name="sToClean"></param>
        /// <param name="cArr"></param>
        /// <returns></returns>
        public static string RemoveConsecutiveSpace(string sToClean, char[] cArr)
        {
            try
            {
                bool bFirst = true;

                // Vérification...
                if (string.IsNullOrEmpty(sToClean) == true)
                {
                    return string.Empty;
                }
                else if (cArr == null || cArr.Length == 0)
                {
                    return sToClean;
                }

                string sOutput = string.Empty;
                string[] sKyWrds = sToClean.Split(cArr);
                foreach (string sKyWrd in sKyWrds)
                {
                    if (sKyWrd.Length > 0)
                    {
                        if (bFirst == false)
                        {
                            sOutput += cArr[0];
                        }
                        sOutput += sKyWrd;
                        bFirst = false;
                    }
                }

                return sOutput;
            }
            catch //(Exception e)
            {
                return "";
            }
        }
    }
}
