using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VSLauncher
{
    public class Project
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastOpenedAt { get; set; }
        public DateTime LastEditedAt { get; set; }
        public string CreatedAtS { get; set; }
        public string LastOpenedAtS { get; set; }
        public string LastEditedAtS { get; set; }
        
        public Project(string uri)
        {
            Uri      = uri;
            Name     = Path.GetFileNameWithoutExtension(uri).SplitCamelCase();
            IsPinned = false;

            CreatedAt    = File.GetCreationTime(uri);
            LastOpenedAt = File.GetLastAccessTime(uri);
            LastEditedAt = File.GetLastWriteTime(uri);

            CreatedAtS    = CreatedAt   .ToString("dd.MM.yyy HH:mm");
            LastOpenedAtS = LastOpenedAt.ToString("dd.MM.yyy HH:mm");
            LastEditedAtS = LastEditedAt.ToString("dd.MM.yyy HH:mm");
        }
    }
}
