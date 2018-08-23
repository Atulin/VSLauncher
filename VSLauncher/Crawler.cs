using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VSLauncher
{
    class Crawler
    {
        public List<string> Locations { get; set; }

        public Crawler(List<string> locations)
        {
            Locations = locations.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        }

        public List<Project> Crawl()
        {
            List<Project> projects = new List<Project>();

            int i = 0;
            foreach (string l in Locations)
            {
                string[] dirs = Directory.GetDirectories(l);

                foreach (string d in dirs)
                {
                    i++;

                    var files = Directory.GetFiles(d, "*.sln");

                    foreach (string f in files)
                    {
                        projects.Add(new Project(f));
                    }
                }

            }

            return projects;
        }
    }


}
