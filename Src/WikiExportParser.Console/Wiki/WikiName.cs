// -----------------------------------------------------------------------
// <copyright file="WikiName.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using PathfinderDb.Schema;

namespace WikiExportParser.Wiki
{
    public class WikiName : IComparable<WikiName>
    {
        public const string DefaultNamespace = "Pathfinder-RPG";

        public WikiName()
        {
        }

        public WikiName(string fullName)
        {
            string name, @namespace;
            ParseFullName(fullName, out @namespace, out name);

            Namespace = @namespace;
            Name = name;
        }

        public WikiName(string @namespace, string name)
        {
            Namespace = @namespace.ToLowerInvariant();
            Name = CleanName(name) ?? string.Empty;
        }

        public string Namespace { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Obtient un identifiant unique pour la page, se basant sur le nom de celle-ci.
        /// </summary>
        public string Id
        {
            get
            {
                //return this.Name.ToLowerInvariant().Replace(",", string.Empty).Replace(' ', '-'); 
                return Ids.Normalize(Name);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Namespace, Name);
        }

        public static string IdFromTitle(string linkTitle)
        {
            //return linkTitle.ToLowerInvariant().Replace("\'", string.Empty).Replace("’", string.Empty).Replace(",", string.Empty).Replace(' ', '-');
            return Ids.Normalize(linkTitle);
        }

        public static WikiName FromLink(string linkText)
        {
            var i = linkText.IndexOf('|');

            if (i != -1)
            {
                return FromString(linkText.Substring(0, i));
            }
            return FromString(linkText);
        }

        public static WikiName FromString(string fullName)
        {
            return new WikiName(fullName);
        }

        public static string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            return name.ToLowerInvariant();
        }

        public override int GetHashCode()
        {
            return Name.ToLowerInvariant().GetHashCode() ^ Namespace.ToLowerInvariant().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as WikiName;

            if (other == null)
                return base.Equals(obj);

            if (ReferenceEquals(this, obj))
                return true;

            return other.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && other.Namespace.Equals(Namespace, StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(WikiName other)
        {
            if (other.Equals(this))
                return 0;

            var x = Namespace.CompareTo(other.Namespace);

            if (x != 0)
                return x;

            return Name.CompareTo(other.Name);
        }

        private static void ParseFullName(string fullName, out string @namespace, out string name)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var i = fullName.IndexOf('.');

            if (i != -1)
            {
                @namespace = fullName.Substring(0, i);
                name = fullName.Substring(i + 1);
            }
            else
            {
                @namespace = null;
                name = fullName;
            }
        }
    }
}