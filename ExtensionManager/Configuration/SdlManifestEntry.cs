using System.Collections.Generic;

namespace TridionCommunity.Extensions.Configuration
{
    public class SdlManifestEntry
    {
        /// <summary>A unique ID for the manifest entry (e.g. 'cme')</summary>
        public string Id { get; set; }
        /// <summary>The English label to display in the menu.</summary>
        public string Title { get; set; }
        /// <summary>The URL to load when the user clicks on the entry.</summary>
        public string Url { get; set; }
        /// <summary>The icon to display in the menu. Should be mono-colored.</summary>
        public string Icon { get; set; }
        /// <summary>Translations of the label for every supported language. Keys: de, es, fr, js, nl.</summary>
        public Dictionary<string, string> Translations { get; set; }
    }
}
