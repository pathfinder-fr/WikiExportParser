namespace WikiExportParser.Wiki
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Serialization;

    [XmlType("wikiPage")]
    [XmlRoot("wikiPage")]
    public class XmlWikiPage
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlArray("categories")]
        [XmlArrayItem("category")]
        public string[] Categories { get; set; }

        public IEnumerable<WikiName> CategoriesNames
        {
            get { return this.Categories.Select(c => WikiName.FromString(c)); }
        }

        [XmlElement("lastModified")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string LastModifiedText
        {
            get { return this.LastModified.ToString("u"); }
            set { this.LastModified = DateTime.ParseExact(value, "u", CultureInfo.InvariantCulture); }
        }

        [XmlIgnore]
        public DateTime LastModified { get; set; }

        [XmlAttribute("version")]
        public int Version { get; set; }

        [XmlArray("inLinks")]
        [XmlArrayItem("link")]
        public string[] InLinks { get; set; }

        public IEnumerable<WikiName> InLinksNames
        {
            get { return this.InLinks.Select(l => WikiName.FromString(l)); }
        }

        [XmlArray("outLinks")]
        [XmlArrayItem("link")]
        public string[] OutLinks { get; set; }

        public IEnumerable<WikiName> OutLinksNames
        {
            get { return this.OutLinks.Select(l => WikiName.FromString(l)); }
        }

        [XmlIgnore]
        public string FileName { get; set; }

        [XmlElement("body")]
        public string Body { get; set; }

        [XmlElement("raw")]
        public string Raw { get; set; }

        [XmlElement("fullName")]
        public string FullName { get; set; }
    }
}
