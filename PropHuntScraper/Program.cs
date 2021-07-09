using OpenQA.Selenium.Firefox;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;

namespace PropHuntScraper
{
    class Program
    {
        public static RedmineManager _rm;

        static void Main(string[] args)
        {
            Console.WriteLine("PropHuntScraper is being configured.");

            // Timer
            Timer Timer = new Timer(3600000);
            Timer.Elapsed += StartScraping;
            Timer.AutoReset = true;
            Timer.Enabled = true;

            // Redmine API
            _rm = new RedmineManager("https://redmine.koertlichtendonk.nl/", "f35cb859d29933894c7bd3e7ff006d262d9e420b", MimeFormat.Json, false);

            // Force first scraping process
            Console.WriteLine("PropHuntScraper is starting first scraping process.");
            StartScraping(null, null);
            Console.WriteLine("PropHuntScraper is ending the first scraping process.");
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

            Console.WriteLine("Navigate to first page to retreive amount of pages.");

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

            Console.WriteLine("There are a total of "+ pages +" pages.");

            List<HouseInfo> unownedHouses = new List<HouseInfo>();

            for (int i = 1; i < pages; i++)
            {
                Console.WriteLine("Starting to retreive houses from page " + i + ".");

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

                    Console.WriteLine(house.address + " was found for sale costing " + house.price + ".");
                    unownedHouses.Add(house);
                }

                Console.WriteLine("Finished to retreive houses from page " + i + ".");
            }

            Console.WriteLine("Begin to get all projects by identifier.");
            var AllProjects = RedmineHelper.GetAllProjectsByIdentifier();
            Console.WriteLine("End of getting all projects by identifier.");

            if (AllProjects.TryGetValue("unowned-houses", out Project project) == false)
            {
                Console.WriteLine("Can't find the Unowned Houses project in Redmine.");
                return;
            }

            Console.WriteLine("Begin to get all issues from project.");
            var AllIssuesFromProject = RedmineHelper.GetAllIssuesFromProject(true, project.Id.ToString());
            Console.WriteLine("End of getting all issues from project.");

            foreach (HouseInfo house in unownedHouses)
            {
                if(!AllIssuesFromProject.ContainsKey(house.address))
                {
                    Issue new_issue = new Issue();

                    new_issue.Project = IdentifiableName.Create<Project>(project.Id);
                    new_issue.Subject = house.address;
                    RedmineHelper.GetAllTrackers().TryGetValue("Todo", out Tracker tracker);
                    new_issue.Status = IdentifiableName.Create<Tracker>(tracker.Id);
                    new_issue.AssignedTo = IdentifiableName.Create<IdentifiableName>( RedmineHelper.GetAllUsers().First().Value.Id );
                    RedmineHelper.GetAllStatuses().TryGetValue("New", out IssueStatus issueStatus);
                    new_issue.Tracker = IdentifiableName.Create<IssueStatus>(issueStatus.Id);
                    new_issue.Description = "!"+house.image+"!\n\n*Price*: "+house.price+"\n*Custom Interior*: "+house.custom_interior;
                    new_issue.StartDate = DateTime.Now;

                    Console.WriteLine("Sending house "+ house.address +" to Redmine.");
                    Program._rm.CreateObject(new_issue);
                    Console.WriteLine("Sent house " + house.address + " to Redmine.");
                } else
                {
                    Console.WriteLine("House " + house.address + " is already on Redmine.");
                }
            }
        }
    }
}
