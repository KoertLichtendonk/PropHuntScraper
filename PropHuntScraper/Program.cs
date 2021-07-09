using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;

namespace PropHuntScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PropHuntScraper is being configured.");

            StartScraping(null, null);

            Timer Timer = new Timer(3600000);
            Timer.Elapsed += StartScraping;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        private static void StartScraping(object sender, ElapsedEventArgs e)
        {
            if (e != null)
            {
                Console.WriteLine("PropHuntScraper is starting the scraping process at {0:HH:mm:ss.fff}", e.SignalTime);
            }

            var service = FirefoxDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            FirefoxOptions options = new FirefoxOptions();

            options.AddArguments("-headless");

            FirefoxDriver Driver = new FirefoxDriver(service, options);
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromHours(1);

            Driver.Navigate().GoToUrl("https://prophunt.biz/unowned?page=1");

            string Body = Driver.GetTrimmedBody();

            MatchCollection matchCollection = Regex.Matches(Body, "<li class=\"page-item\"><a class=\"page-link\" href=\"(?:.*?)\">(.\\d+?)</a></li>");

            int pages = 1;
            foreach (Match match in matchCollection)
            {
                int page_limit_check = Convert.ToInt32(match.Groups[1].Value);
                if (page_limit_check > pages)
                {
                    pages = page_limit_check;
                }
            }

            List<HouseInfo> unownedHouses = new List<HouseInfo>();

            for (int i = 1; i < pages; i++)
            {
                Driver.Navigate().GoToUrl("https://prophunt.biz/unowned?page=" + i);

                string pageBody = Driver.GetTrimmedBody();

                MatchCollection pageMatchCollection = Regex.Matches(pageBody, "<div class=\"col-lg-4 col-md-6\"> <div class=\"member\"> <img src=\"(.*?)\" alt=\"\"> <h4>(.*?)</h4> <div class=\"social\"> <strong>Price:</strong> (.*?)<br> <strong>Custom Interior:</strong>(.*?)</div> </div> </div>");

                foreach (Match match in pageMatchCollection)
                {
                    HouseInfo house = new HouseInfo();

                    house.image = match.Groups[1].Value.Trim();
                    house.address = match.Groups[2].Value.Trim();
                    house.price = match.Groups[3].Value.Trim();
                    house.custom_interior = match.Groups[4].Value.Trim();

                    unownedHouses.Add(house);
                }
            }
        }
    }
}
