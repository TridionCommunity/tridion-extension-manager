using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TridionCommunity.Extensions.Properties;

namespace TridionCommunity.Extensions
{
    internal class SystemConfiguration
    {
        protected XNamespace c = @"http://www.sdltridion.com/2009/GUI/Configuration";
        protected string filePath;
        protected XDocument configuration;
        protected bool loaded;

        protected SystemConfiguration(string filePath)
        {
            this.filePath = filePath;
            configuration = XDocument.Load(filePath);
            loaded = true;
        }

        public static SystemConfiguration Load(string filePath)
        {
            return new SystemConfiguration(filePath);
        }

        public void Save()
        {
            if (!loaded) return;
            UpdateModNumber();
            configuration.Save(filePath);
        }

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

        public void AddEditor(SystemConfigSection editor, string installationDirectory)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            AddConfigurationInfo(GetEditorRootNode(), @"editor", editor, installationDirectory);
        }

        public void AddModel(SystemConfigSection model, string installationDirectory)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            AddConfigurationInfo(GetModelRootNode(), @"model", model, installationDirectory);
        }

        public void RemoveEditor(SystemConfigSection editor)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            RemoveExistingNodes(GetEditorRootNode(), @"editor", editor.Name);
        }

        public void RemoveModel(SystemConfigSection model)
        {
            if (!loaded)
            {
                throw new InvalidOperationException(Resources.ErrConfigurationNotLoaded);
            }

            RemoveExistingNodes(GetModelRootNode(), @"model", model.Name);
        }


        protected void UpdateModNumber()
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

        protected XElement GetEditorRootNode()
        {
            var result = configuration.Root.Element(c + @"editors");
            if (result == null)
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, Resources.ErrMissingSectionInSystemConfig, @"editors"));
            }
            return result;
        }

        protected XElement GetModelRootNode()
        {
            var result = configuration.Root.Element(c + @"models");
            if (result == null)
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, Resources.ErrMissingSectionInSystemConfig, @"models"));
            }
            return result;
        }

        protected void AddConfigurationInfo(XElement container, string elementName, SystemConfigSection section, string installationDirectory)
        {
            var result = new XElement(c + elementName);
            result.SetAttributeValue(@"name", section.Name);
            result.SetElementValue(c + @"installpath", Path.Combine(installationDirectory, section.Path));
            result.SetElementValue(c + @"configuration", section.ConfigurationFile);
            result.SetElementValue(c + @"vdir", section.VirtualDirectory);
            container.Add(result);
        }

        protected List<XElement> GetExistingNodes(XElement container, string elementName, string name)
        {
            return container.Elements().Where(e => e.Name.Equals(c + elementName) && name.Equals(e.Attribute(@"name").Value, StringComparison.InvariantCulture)).ToList();
        }

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
