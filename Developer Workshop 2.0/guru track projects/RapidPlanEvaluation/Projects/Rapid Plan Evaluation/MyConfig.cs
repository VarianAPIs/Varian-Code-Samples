#region copyright
////////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
//////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Configuration;

namespace RapidPlanEvaluation
{
    public class Myconfig
    {
        public static DefaultMetricsConfiguration GetDefaultMetricsSection()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (new UMRO.Utils.ConfigurationHelper.ConfigResolveHelper(assembly))
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(assembly.Location);
                return (DefaultMetricsConfiguration)config.GetSection("DefaultMetrics");
            }
        }

        public static CustomMetricsConfiguration GetCustomMetricsSection()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (new UMRO.Utils.ConfigurationHelper.ConfigResolveHelper(assembly))
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(assembly.Location);
                return (CustomMetricsConfiguration)config.GetSection("CustomMetrics");
            }
        }

        public static SitesConfiguration GetSitesConfiguration()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (new UMRO.Utils.ConfigurationHelper.ConfigResolveHelper(assembly))
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(assembly.Location);
                return (SitesConfiguration)config.GetSection("SitesConfiguration");
            }
        }

        public static BioDoseConfiguration GetBioDoseConfiguration()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (new UMRO.Utils.ConfigurationHelper.ConfigResolveHelper(assembly))
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(assembly.Location);
                return (BioDoseConfiguration)config.GetSection("BioDoseConfiguration");
            }
        }

        public static string GetAppKey(string key)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (new UMRO.Utils.ConfigurationHelper.ConfigResolveHelper(assembly))
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(assembly.Location);
                return config.AppSettings.Settings[key].Value;
            }
        }
    }

    // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Custom Metrics Configuration Section
    public class CustomMetricsConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("Metrics", IsRequired = true, IsDefaultCollection = true)]
        public CustomMetricElementCollection Metrics
        {
            get { return (CustomMetricElementCollection)this["Metrics"]; }
            set { this["Metrics"] = value; }
        }
    }

    [ConfigurationCollection(typeof(CustomMetricElement))]
    public class CustomMetricElementCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "Metric";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }


        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomMetricElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CustomMetricElement)element).Name;
        }

    }

    public class CustomMetricElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("base", IsRequired = true)]
        public string Base
        {
            get { return (string)base["base"]; }
            set { base["base"] = value; }
        }

        [ConfigurationProperty("parameter", IsRequired = true)]
        public double Parameter
        {
            get { return (double)base["parameter"]; }
            set { base["parameter"] = value; }

        }

    }
    // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Default Metrics Configuration Section
    public class DefaultMetricsConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("Metrics", IsRequired = true, IsDefaultCollection = true)]
        public DefaultMetricElementCollection Metrics
        {
            get { return (DefaultMetricElementCollection)this["Metrics"]; }
            set { this["Metrics"] = value; }
        }
    }

    [ConfigurationCollection(typeof(DefaultMetricElement))]
    public class DefaultMetricElementCollection: ConfigurationElementCollection
    {
        internal const string PropertyName = "Metric";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }

            
        protected override ConfigurationElement CreateNewElement()
        {
            return new DefaultMetricElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DefaultMetricElement)element).Structure;
        }

    }

    public class DefaultMetricElement:ConfigurationElement
    {
        [ConfigurationProperty("structure",IsKey=true,IsRequired=true)]
        public string Structure
        {
            get{return (string)base["structure"];}
            set { base["structure"] = value; }
        }

        [ConfigurationProperty("metric",IsRequired=true)]
        public string Metric
        {
            get{return (string)base["metric"];}
            set { base["metric"] = value; }
        }
    }
    // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Sites Configuration Section
    public class SitesConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("Sites", IsRequired = true, IsDefaultCollection = true)]
        public SitesCollection Sites
        {
            get { return (SitesCollection)this["Sites"]; }
            set { this["Sites"] = value; }
        }
    }

    [ConfigurationCollection(typeof(Site))]
    public class SitesCollection : ConfigurationElementCollection
    {
        internal const string PropertyName = "Site";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }


        protected override ConfigurationElement CreateNewElement()
        {
            return new Site();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Site)element).Name;
        }

    }

    public class Site : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("structure", IsRequired = true)]
        public string Structure
        {
            get { return (string)base["structure"]; }
            set { base["structure"] = value; }
        }

        [ConfigurationProperty("metric", IsRequired = true)]
        public string Metric
        {
            get { return (string)base["metric"]; }
            set { base["metric"] = value; }
        }

        [ConfigurationProperty("goal", IsRequired = true)]
        public double Goal
        {
            get { return (double)base["goal"]; }
            set { base["goal"] = value; }
        }

        [ConfigurationProperty("alphabeta", IsRequired = true)]
        public double AlphaBeta
        {
            get { return (double)base["alphabeta"]; }
            set { base["alphabeta"] = value; }
        }

        [ConfigurationProperty("lkbn", IsRequired = true)]
        public double LKBn
        {
            get { return (double)base["lkbn"]; }
            set { base["lkbn"] = value; }
        }

        [ConfigurationProperty("lkbm", IsRequired = true)]
        public double LKBm
        {
            get { return (double)base["lkbm"]; }
            set { base["lkbm"] = value; }
        }

        [ConfigurationProperty("lkbd50", IsRequired = true)]
        public double LKBd50
        {
            get { return (double)base["lkbd50"]; }
            set { base["lkbd50"] = value; }
        }

    }
    // /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Bio Dose configuration section
    public class BioDoseConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("Target", DefaultValue = 2.5, IsRequired = false)]
        public double TargetAlphaBeta
        {
            get { return (double)base["Target"]; }
            set { base["Target"] = value; }
        }

        [ConfigurationProperty("Organ", DefaultValue = 10.0, IsRequired = false)]
        public double OrganAlphaBeta
        {
            get { return (double)base["Organ"]; }
            set { base["Organ"] = value; }
        }
    }
}
