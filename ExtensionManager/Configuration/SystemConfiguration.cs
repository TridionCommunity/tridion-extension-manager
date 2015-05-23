using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TridionCommunity.Extensions.Configuration;
using TridionCommunity.Extensions.Properties;

namespace TridionCommunity.Extensions
{
    /// <summary>
    /// Represents the System.config file used by the Tridion Content Manager Explorer.
    /// </summary>
    internal class SystemConfiguration
    {
        protected XNamespace c = @"http://www.sdltridion.com/2009/GUI/Configuration";
        protected string filePath;
        protected XDocument configuration;
        protected bool loaded;

        /// <summary>
        /// Creates a new instance of the <see cref="SystemConfigration"/>.
        /// </summary>
        /// <param name="filePath"></param>
        protected SystemConfiguration(string filePath)
        {
            this.filePath = filePath;
            configuration = XDocument.Load(filePath);
            loaded = true;
        }

        /// <summary>
        /// Loads the configuration from the given path.
        /// </summary>
        /// <param name="filePath">The path to the System.config file, including the file name.</param>
        /// <returns>A new instance of the <see cref="SystemConfiguration"/> class.</returns>
        public static SystemConfiguration Load(string filePath)
        {
            return new SystemConfiguration(filePath);
        }

        /// <summary>
        /// Applies the changes made to the System.config file. Clears the browser cache of users automatically.
        /// </summary>
        public void Save()
        {
            if (!loaded) return;
            ClearBrowserCache();
            configuration.Save(filePath);
        }

        /// <summary>
        /// Checks if the configuration contains an entry for a given editor.
        /// </summary>
        /// <param name="editor">The editor to check for.</param>
        /// <exception cref="System.InvalidOperationException">If the configuration has not yet been loaded.</exception>
        /// <returns>true if the editor is present in the configuration; false otherwise.</returns>
        public bool HasEditor(SystemConfigSection editor)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            if (editor == null)
            {
                return false;
            }

            return GetExistingNodes(GetEditorRootNode(), "editor", editor.Name).Count > 0;
        }

        /// <summary>
        /// Checks if the configuration contains an entry for a given model.
        /// </summary>
        /// <param name="model">The model to check for.</param>
        /// <exception cref="System.InvalidOperationException">If the configuration has not yet been loaded.</exception>
        /// <returns>true if the model is present in the configuration; false otherwise.</returns>
        public bool HasModel(SystemConfigSection model)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            if (model == null)
            {
                return false;
            }

            return GetExistingNodes(GetModelRootNode(), "model", model.Name).Count > 0;
        }

        /// <summary>
        /// Adds the given editor to the configuration, with an absolute installation path.
        /// </summary>
        /// <param name="editor">The editor to add.</param>
        /// <param name="installationDirectory">The installation directory of the extension. Used to create the absolute installation path for the editor.</param>
        /// <exception cref="System.InvalidOperationException">If the configuration has not yet been loaded.</exception>
        public void AddEditor(SystemConfigSection editor, string installationDirectory)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            AddConfigurationInfo(GetEditorRootNode(), @"editor", editor, installationDirectory);
        }

        /// <summary>
        /// Adds the given model to the configuration, with an absolute installation path.
        /// </summary>
        /// <param name="model">The model to add.</param>
        /// <param name="installationDirectory">The installation directory of the extension. Used to create the absolute installation path for the model.</param>
        /// <exception cref="System.InvalidOperationException">If the configuration has not yet been loaded.</exception>
        public void AddModel(SystemConfigSection model, string installationDirectory)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            AddConfigurationInfo(GetModelRootNode(), @"model", model, installationDirectory);
        }

        /// <summary>
        /// Removes the given editor from the configuration.
        /// </summary>
        /// <param name="editor">The editor to remove.</param>
        /// <exception cref="System.InvalidOperationException">If the configuration has not yet been loaded.</exception>
        public void RemoveEditor(SystemConfigSection editor)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            RemoveExistingNodes(GetEditorRootNode(), @"editor", editor.Name);
        }

        /// <summary>
        /// Removes the given model from the configuration.
        /// </summary>
        /// <param name="model">The model to remove.</param>
        /// <exception cref="System.InvalidOperationException">If the configuration has not yet been loaded.</exception>
        public void RemoveModel(SystemConfigSection model)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            RemoveExistingNodes(GetModelRootNode(), @"model", model.Name);
        }


        /// <summary>
        /// Updates the server modification attribute in the configuration, resulting in an automatic browser cache invalidation for all users.
        /// </summary>
        protected void ClearBrowserCache()
        {
            var serverNode = configuration.Descendants(c + "server").FirstOrDefault();
            if (serverNode != null)
            {
                var modificationAttr = serverNode.Attribute("modification");
                if (modificationAttr != null)
                {
                    int currentValue = int.Parse(modificationAttr.Value);
                    modificationAttr.SetValue(++currentValue);
                }
            }
        }

        /// <summary>
        /// Gets the parent <code>editors</code> XML element from the configuration.
        /// </summary>
        /// <returns>The <see cref="XElement"/> which serves as a parent for all editor elements.</returns>
        /// <exception cref="ConfigurationException">If the required <code>editors</code> node was not found in the configuration.</exception>
        protected XElement GetEditorRootNode()
        {
            var result = configuration.Root.Element(c + @"editors");
            if (result == null)
            {
                throw new ConfigurationException(String.Format(CultureInfo.InvariantCulture, Resources.ErrMissingSectionInSystemConfig, @"editors"));
            }
            return result;
        }

        /// <summary>
        /// Gets the parent <code>models</code> XML element from the configuration.
        /// </summary>
        /// <returns>The <see cref="XElement"/> which serves as a parent for all model elements.</returns>
        /// <exception cref="ConfigurationException">If the required <code>models</code> node was not found in the configuration.</exception>
        protected XElement GetModelRootNode()
        {
            var result = configuration.Root.Element(c + @"models");
            if (result == null)
            {
                throw new ConfigurationException(String.Format(CultureInfo.InvariantCulture, Resources.ErrMissingSectionInSystemConfig, @"models"));
            }
            return result;
        }

        /// <summary>
        /// Adds a new section to the configuration based on the given <see cref="SystemConfigSection"/>.
        /// </summary>
        /// <param name="container">The parent element to add the section under.</param>
        /// <param name="elementName">The element name of the new section.</param>
        /// <param name="section">The <see cref="SystemConfigSection"/> to use as input for the new section.</param>
        /// <param name="installationDirectory">The installation directory of the extension. Used to create the absolute path to the editor or model.</param>
        protected void AddConfigurationInfo(XElement container, string elementName, SystemConfigSection section, string installationDirectory)
        {
            var result = new XElement(c + elementName);
            result.SetAttributeValue(@"name", section.Name);
            result.SetElementValue(c + @"installpath", Path.Combine(installationDirectory, section.Path));
            result.SetElementValue(c + @"configuration", section.ConfigurationFile);
            result.SetElementValue(c + @"vdir", section.VirtualDirectory);
            container.Add(result);
        }

        /// <summary>
        /// Gets any existing nodes with a given element name and the specified value in their <code>name</code> attributes.
        /// </summary>
        /// <param name="container">The parent element of the nodes.</param>
        /// <param name="elementName">The XML name of the elements to find.</param>
        /// <param name="name">The value of the <code>name</code> attribute that the elements must have in order to be returned.</param>
        /// <returns>A list of elements matching the specified parameters.</returns>
        protected List<XElement> GetExistingNodes(XElement container, string elementName, string name)
        {
            return container.Elements().Where(e => e.Name.Equals(c + elementName) && name.Equals(e.Attribute(@"name").Value, StringComparison.InvariantCulture)).ToList();
        }

        /// <summary>
        /// Removes all nodes with a given element name and the specified value in their <code>name</code> attributes.
        /// </summary>
        /// <param name="container">The parent element of the nodes.</param>
        /// <param name="elementName">The XML name of the elements to find.</param>
        /// <param name="name">The value of the <code>name</code> attribute that the elements must have in order to be returned.</param>
        protected void RemoveExistingNodes(XElement container, string elementName, string name)
        {
            if (container == null) return;

            var existingElements = GetExistingNodes(container, elementName, name);
            if (existingElements != null)
            {
                existingElements.ForEach(e => e.Remove());
            }
        }
    }
}
