using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace PropHuntScraper
{
    public class RedmineHelper
    {
        private static Dictionary<string, Project> _AllProjects { get; set; } = null;
        public static Dictionary<string, Project> GetAllProjectsByIdentifier(bool force = false)
        {
            if (_AllProjects == null || force == true)
            {
                _AllProjects = new Dictionary<string, Project>();

                NameValueCollection parameters = new NameValueCollection { }; // *
                foreach (var Project in Program._rm.GetObjects<Project>(parameters))
                {
                    _AllProjects.Add(Project.Identifier, Project);
                }
            }
            return _AllProjects;
        }

        private static Dictionary<string, Issue> _AllIssuesFromProject { get; set; } = null;
        public static Dictionary<string, Issue> GetAllIssuesFromProject(bool force = false, string project_id = "*")
        {
            if (_AllIssuesFromProject == null || force == true)
            {
                _AllIssuesFromProject = new Dictionary<string, Issue>();

                NameValueCollection parameters = new NameValueCollection { { "project_id", project_id }, { "status_id", "*" } };
                foreach (var Issue in Program._rm.GetObjects<Issue>(parameters))
                {
                    _AllIssuesFromProject.Add(String.Format("{0}", Issue.Subject), Issue);
                }
            }
            return _AllIssuesFromProject;
        }

        private static Dictionary<string, IssueStatus> _AllStatuses { get; set; } = null;
        public static Dictionary<string, IssueStatus> GetAllStatuses()
        {
            if (_AllStatuses == null)
            {
                _AllStatuses = new Dictionary<string, IssueStatus>();

                foreach (var Status in Program._rm.GetObjects<IssueStatus>())
                {
                    _AllStatuses.Add(String.Format("{0}", Status.Name), Status);
                }
            }
            return _AllStatuses;
        }

        private static Dictionary<string, Tracker> _AllTrackers { get; set; } = null;
        public static Dictionary<string, Tracker> GetAllTrackers()
        {
            if (_AllTrackers == null)
            {
                _AllTrackers = new Dictionary<string, Tracker>();

                foreach (var Tracker in Program._rm.GetObjects<Tracker>())
                {
                    _AllTrackers.Add(String.Format("{0}", Tracker.Name), Tracker);
                }
            }
            return _AllTrackers;
        }

        private static Dictionary<string, User> _AllUsers { get; set; } = null;
        public static Dictionary<string, User> GetAllUsers(bool force = false)
        {

            if (_AllUsers == null || force == true)
            {
                _AllUsers = new Dictionary<string, User>();

                foreach (var User in Program._rm.GetObjects<User>())
                {
                    _AllUsers.Add(String.Format("{0} {1}", User.FirstName, User.LastName), User);
                }
            }
            return _AllUsers;
        }
    }
}
