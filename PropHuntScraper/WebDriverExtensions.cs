using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PropHuntScraper
{
    public static class WebDriverExtensions
    {
        public static string GetTrimmedBody(this FirefoxDriver Driver)
        {
            string source = Driver.PageSource.Replace("\n", "").Replace("\r", "").Replace(@"\", "").Replace("\\", "");

            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            source = regex.Replace(source, " ");

            Match body = Regex.Match(source, "<body(?:.*?)>");

            source = source.Substring(source.IndexOf(body.Value));
            source = source.Substring(0, source.IndexOf("</body>"));
            return source;
        }
    }
}
