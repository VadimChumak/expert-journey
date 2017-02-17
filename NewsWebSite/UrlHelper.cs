using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace NewsWebSite
{
    public class UrlHelper
    {
        public static string ValiateUrl(string url)
        {
            return Regex.Replace(url, @"[\W \s]+", "-");
        }
    }
}