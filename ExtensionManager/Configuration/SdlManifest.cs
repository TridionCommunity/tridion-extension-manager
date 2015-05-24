using System;
using System.Linq;
using System.Xml.Linq;
using TridionCommunity.Extensions.Properties;

namespace TridionCommunity.Extensions.Configuration
{
    /// <summary>
    /// Represents the manifest.xml file used to manage entries in the SDL slide-out navigation pane.
    /// </summary>
    public class SdlManifest
    {
        protected string filePath;
        protected XDocument configuration;

        /// <summary>
        /// Creates a new instance of the <see cref="SdlManifest"/> class.
        /// </summary>
        /// <param name="filePath">The full path to the manifest.xml file.</param>
        protected SdlManifest(string filePath)
        {
            this.filePath = filePath;
            configuration = XDocument.Load(filePath);
        }

        /// <summary>
        /// Loads the configuration from the given path.
        /// </summary>
        /// <param name="filePath">The full path to the manifest.xml file.</param>
        /// <returns>A new instance of the <see cref="SdlManifest"/> class.</returns>
        public static SdlManifest Load(string filePath)
        {
            return new SdlManifest(filePath);
        }

        /// <summary>
        /// Applies the changes made to the SDL manifest file.
        /// </summary>
        public void Save()
        {
            configuration.Save(filePath);
        }

        /// <summary>
        /// Adds a new entry to the SDL manifest file.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        public void AddEntry(SdlManifestEntry entry)
        {
            var parent = GetParentNode();
            RemoveExistingNodes(parent, entry.Id);
            var element = new XElement(@"applicationEntryPoint");
            element.SetAttributeValue(@"id", entry.Id);
            element.SetAttributeValue(@"title", entry.Title);
            element.SetAttributeValue(@"domainId", @"local");
            element.SetAttributeValue(@"url", "/WebUI/Editors/CME/Views/ProxyView/ProxyView.aspx?pagePath=" + entry.Url);
            element.SetAttributeValue(@"icon", entry.Icon);

            if (entry.Translations != null)
            {
                var translationsRootElement = new XElement(@"translations");
                element.Add(translationsRootElement);
                
                foreach (var translation in entry.Translations)
                {
                    var translationEntry = new XElement(@"title");
                    translationEntry.SetAttributeValue(@"lang", translation.Key);
                    translationEntry.Value = translation.Value;
                    translationsRootElement.Add(translationEntry);
                }
            }
        }

        /// <summary>
        /// Removes an entry from the SDL manifest file.
        /// </summary>
        /// <param name="entry">The entry to remove.</param>
        public void RemoveEntry(SdlManifestEntry entry)
        {
            var parent = GetParentNode();
            RemoveExistingNodes(parent, entry.Id);
        }


        /// <summary>
        /// Retrieves the parent node that contains the custom entries managed by this class.
        /// </summary>
        /// <returns>The parent node to insert entries under or remove child entries from.</returns>
        /// <exception cref="ConfigurationException">If the parent element could not be located in the configuration.</exception>
        protected XElement GetParentNode()
        {
            var wcmElem = configuration.Descendants(@"applicationEntryPointGroup").FirstOrDefault(e => @"wcm.cm".Equals((string)e.Attribute("id"), StringComparison.InvariantCulture));
            if (wcmElem != null)
            {
                var result = wcmElem.Element(@"applicationEntryPoints");
                if (result != null)
                {
                    return result;
                }
            }

            throw new ConfigurationException(Resources.ErrInvalidSdlManifestFile);
        }

        /// <summary>
        /// Removes all manifest entry nodes with a given ID.
        /// </summary>
        /// <param name="parent">The root node for entries, as returned by <code>GetParentNode</code></param>
        /// <param name="id">The ID of the entry to remove</param>
        protected void RemoveExistingNodes(XElement parent, string id)
        {
            var existingElements = parent.Elements(@"applicationEntryPoint").Where(e => id.Equals((string)e.Attribute("id"), StringComparison.InvariantCulture)).ToList();
            existingElements.ForEach(e => e.Remove());
        }
    }
}
