using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class VepUrlRedirection
    {
        public static string Migrate(string url, StringBuilder log = null)
        {
            try
            {
                string[] route = null;

                // Checking...
                if (string.IsNullOrEmpty(url) == true)
                {
                    log?.Append($"INVALID URL {url}");
                    return null;
                }
                // Migrate the route...
                else if (url.Contains("ax=") == true)
                {
                    route = _ExtractRoute(url, log);
                    if (url.Contains("ax=information") == true)
                    {
                        // V1 info url...
                        if (route.Length == 5)
                        {
                            string[] v1route = route[4].Split(new char[] { '?' });
                            if ((v1route?.Length ?? 0) == 2)
                            {
                                //v1route[0]: titre
                                //v1route[1]: fr=2&mi=61&id=167&ax=information: id=167
                                string[] v1route2 = v1route[1]?.Split(new char[] { '&' });
                                if (v1route2 != null)
                                {
                                    foreach (string v1r2 in v1route2)
                                    {
                                        if (v1r2.ToLower().Contains("id=") == true)
                                        {
                                            return $"/{route[3]}/{v1route[0].Replace(".aspx", string.Empty)}/pg0/pt{v1r2.ToLower().Replace("id=", string.Empty)}";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (url.Contains("ax=calendrier") == true || url.Contains("ax=inscription") == true)
                    {
                        // V1 calendar url...
                        return "/calendrier/pg11";
                    }
                    // Unknow V1 url...
                    log?.Append($"UNKNOW V1 URL: {url}");
                    return null;
                }
                else if (url.Contains(".aspx") == true)
                {
                    route = _ExtractRoute(url, log);
                    // Home pages...
                    if (url.Contains("accueil.aspx") == true)
                    {
                        // V2 home url...
                        return "/";
                    }
                    else if (url.Contains("accueil-") == true)
                    {
                        // V2 region url...
                        //log?.Append($"V2 HOME REGION URL: {url}");
                        if ((route?.Length ?? 0) > 3)
                        {
                            return $"/{route[3]}";
                        }
                        log?.Append($"UNKNOW V2 HOME REGION URL: {url}");
                        return "/";
                    }
                    // Calendar page...
                    else if (url.Contains("calendrier-") == true)
                    {
                        //log?.Append($"V2 calendrier URL: {url}");
                        if (route[4] == "calendrier")
                        {
                            return $"/{route[3]}/calendrier/pg11";
                        }
                        else if (route[4] == "retraites-et-pelerinages")
                        {
                            return $"/{route[3]}/retraites-pelerinage/pg11/ct14/cldr";
                        }
                        log?.Append($"UNKNOW V2 CALENDAR URL: {url}");
                        return $"/calendrier/pg11";
                    }
                    // List pages...
                    else if (url.Contains("/actualite-") == true)
                    {
                        //log?.Append($"V2 actualite URL: {url}");
                        return $"/{route[3]}/derniers-articles/pg5" + _ExtractSkip(route[5]);
                    }
                    else if ((url.Contains("/informations-") == true || url.Contains("/informationsl-") == true) && url.Contains("/information-") == false)
                    {
                        //log?.Append($"V2 info list URL: {url}");
                        switch (route[4])
                        {
                            case "adoration":
                            case "annonces":
                                return $"/{route[3]}/mediatheque/pg6";

                            case "confession":
                                return $"/{route[3]}/confession/pg6/ct20";
                            case "autres-ecrits":
                                return $"/{route[3]}/autres-ecrits/pg6/ct24";
                            case "ecrits-de-saints":
                                return $"/{route[3]}/ecrits-de-saints/pg6/ct23";
                            case "enseignements":
                                return $"/{route[3]}/enseignement/pg6/ct16";
                            case "photos":
                                return $"/{route[3]}/photo/pg6/ct17";
                            case "prieres":
                                return $"/{route[3]}/prieres/pg6/ct19";
                            case "prieres-de-l-eglise":
                                return $"/{route[3]}/prieres-de-l-eglise/pg6/ct25";
                            case "avec-dieu-trinite":
                                return $"/{route[3]}/prieres-avec-dieu-trinite/pg6/ct30";
                            case "avec-l-esprit-saint":
                                return $"/{route[3]}/prieres-avec-l-esprit-saint/pg6/ct29";
                            case "avec-un-saint":
                                return $"/{route[3]}/prieres-avec-un-saint/pg6/ct27";
                            case "d-intercession":
                                return $"/{route[3]}/prieres-d-intercession/pg6/ct28";
                            case "litanies":
                                return $"/{route[3]}/litanies/pg6/ct32";
                            case "neuvaines":
                                return $"/{route[3]}/neuvaines/pg6/ct31";
                            ///prieres-avec-marie/pg6/ct26

                            case "temoignage":
                                return $"/{route[3]}/temoignage/pg6/ct21";

                            default:
                                log?.Append($"UNKNOW V2 INFO LIST URL: {url}");
                                return $"/{route[3]}/mediatheque/pg6";
                        }
                    }
                    else if (url.Contains("/oeuvre-") == true)
                    {
                        //log?.Append($"V2 oeuvre URL: {url}");
                        switch (route[4])
                        {
                            case "audio":
                                return $"/{route[3]}/louanges/pg6/ct35";
                            case "livres":
                                return $"/{route[3]}/livres/pg6/ct34";

                            default:
                                log?.Append($"UNKNOW V2 OEUVRES LIST URL: {url}");
                                return $"/{route[3]}/mediatheque/pg6";
                        }
                    }
                    // Post pages...
                    else if (url.Contains("/inscription-") == true)
                    {
                        //log?.Append($"V2 inscription URL: {url}");
                        return $"/{route[3]}/retraites-pelerinage/pg11/ct14/cldr";
                    }
                    else if (url.Contains("/information-") == true)
                    {
                        string[] targetV2Arr = route[5]?.Split(new char[] { '-' });
                        if ((targetV2Arr?.Length ?? 0) == 4)
                        {
                            return $"/{route[3]}/{route[4]}/pg0/pt{targetV2Arr[3]?.Replace(".aspx", string.Empty)}";
                        }
                        // Unknow V2 INFOD url...
                        log?.Append($"UNKNOW V2 INFO URL: {url}");
                        return null;
                    }
                    else if (url.Contains("/prieres.htm") == true)
                    {
                        return $"/prieres/pg6/ct19";
                    }
                    else if (url.Contains("/confession.htm") == true)
                    {
                        return $"/confession/pg6/ct20";
                    }
                    else
                    {
                        // Unknow url...
                        log?.Append($"UNKNOW {url}");
                        return null;
                    }
                }
                else
                {
                    log?.Append($"DONT PROCESS {url}");
                    return null;
                }
            }
            catch (Exception e)
            {
                log?.Append($"FAILED TO PROCESS {url}: {e.Message}");
                return null;
            }
        }

        private static string[] _ExtractRoute(string url, StringBuilder log)
        {
            string[] route = null;
            if (((route = url.Split(new char[] { '/' }))?.Length ?? 0) == 0)
            {
                log?.Append($"INVALID ROUTE {url}");
                return null;
            }
            else if (route.Length > 3 && route[3].StartsWith("paris-") == true)
                route[3] = "paris";
            return route;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        private static string _ExtractSkip(string route)
        {
            string skip = string.Empty;
            string[] v2route = route?.Split(new char[] { '?', '&' });
            if (v2route != null)
            {
                foreach (string v2r in v2route)
                {
                    if (v2r.ToLower().Contains("skip=") == true)
                    {
                        skip = "?" + v2r;
                        break;
                    }
                }
            }
            return skip;
        }
    }
}
