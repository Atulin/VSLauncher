using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VSLauncher
{
    public class Project : IEquatable<Project>
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastOpenedAt { get; set; }
        public DateTime LastEditedAt { get; set; }
        
        public Project(string uri)
        {
            Uri      = uri;
            Name     = Path.GetFileNameWithoutExtension(uri).SplitCamelCase();
            IsPinned = false;

            CreatedAt    = File.GetCreationTime(uri);
            LastOpenedAt = File.GetLastAccessTime(uri);
            LastEditedAt = File.GetLastWriteTime(uri);
        }

        public bool Equals(Project x, Project y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(Project obj)
        {
            int uri = Uri.GetHashCode();
            int ispinned = IsPinned.GetHashCode();

            return uri ^ ispinned;
        }

        public bool Equals(Project other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && string.Equals(Uri, other.Uri) && CreatedAt.Equals(other.CreatedAt);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Project) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Uri != null ? Uri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ CreatedAt.GetHashCode();
                return hashCode;
            }
        }
    }

    
    public class SProjects
    {
        public static bool Save(List<Project> projects, string file)
        {
            var json = JsonConvert.SerializeObject(projects, Formatting.Indented);
            File.WriteAllText(file, json);

            return File.Exists(file) && File.ReadAllBytes(file).Length > 0;
        }

        public static List<Project> Read(string file)
        {
            var text = File.ReadAllText(file);
            var p = JsonConvert.DeserializeObject<List<Project>>(text);
            return p;
        }
    }

}
