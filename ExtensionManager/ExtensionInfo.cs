using System;

namespace TridionCommunity.Extensions
{
    /// <summary>
    /// Contains metadata about an extension. For use with the <see cref="ExtensionManager"/>.
    /// </summary>
    public class ExtensionInfo : IComparable
    {
        /// <summary>The header to use for grouping purposes, when displaying a list of extensions.</summary>
        public string Header { get; internal set; }
        /// <summary>The official name of the extension.</summary>
        public string Name { get; internal set; }
        /// <summary>The short description of what the extension does. May contain HTML markup.</summary>
        public string Description { get; internal set; }
        /// <summary>The icon to use when displaying the extension in a list.</summary>
        public string Icon { get; internal set; }
        /// <summary>The author of the extension (company or individual).</summary>
        public string Author { get; internal set; }
        /// <summary>The current version of the extension.</summary>
        public Version Version { get; internal set; }
        /// <summary>The current installation state of the extension.</summary>
        public InstallState Status { get; internal set; }
        /// <summary>Whether or not the extension is currently enabled.</summary>
        public bool Enabled { get; internal set; }

        internal ExtensionInfo()
        {
        }

        /// <summary>
        /// Updates all of the properties of the current ExtensionInfo with values from another.
        /// </summary>
        /// <param name="source">The source to copy the values from.</param>
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

        /// <summary>
        /// Compares the header of the current object to that of another of the same type. Mainly used for sorting.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>-1 if the current object should be sorted above the other. 0 if they are equal. 1 if the other object should come first.</returns>
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
