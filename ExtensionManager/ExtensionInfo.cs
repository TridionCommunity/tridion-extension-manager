using System;

namespace TridionCommunity.Extensions
{
    public class ExtensionInfo : IComparable
    {
        public string Header { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Icon { get; internal set; }
        public string Author { get; internal set; }
        public Version Version { get; internal set; }
        public InstallState Status { get; internal set; }
        public bool Enabled { get; internal set; }

        internal ExtensionInfo()
        {
        }

        internal void UpdateFrom(ExtensionInfo source)
        {
            Header = source.Header;
            Name = source.Name;
            Description = source.Description;
            Icon = source.Icon;
            Author = source.Author;
            Version = source.Version;
            Status = source.Status;
            Enabled = source.Enabled;
        }

        public int CompareTo(object obj)
        {
            var other = obj as ExtensionInfo;
            if (other != null)
            {
                return Header.CompareTo(other.Header);
            }
            return -1;
        }
    }
}
