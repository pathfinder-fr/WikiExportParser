// -----------------------------------------------------------------------
// <copyright file="MarkupUtil.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using PathfinderDb.Schema;

namespace WikiExportParser.Wiki
{
    public static class MarkupUtil
    {
        public const string LinkPattern = @"\[\[(?<Name>[^|#\]]+)(?<Anchor>#[^|\]]+)?(\|(?<Title>[^\]]+))?\]\]";

        public const string LinkOnlyPattern = "^" + LinkPattern + "$";

        public static string DetectSourceSnippet(string markup)
        {
            if (markup.IndexOf("{s:UC}", StringComparison.OrdinalIgnoreCase) != -1)
                return Source.Ids.UltimateCombat;
            if (markup.IndexOf("{s:UM}", StringComparison.OrdinalIgnoreCase) != -1)
                return Source.Ids.UltimateMagic;
            if (markup.IndexOf("{s:APG}", StringComparison.OrdinalIgnoreCase) != -1)
                return Source.Ids.AdvancedPlayerGuide;

            return null;
        }

        public static string RemoveHtmlMarkup(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            html = Regex.Replace(html, @"href=""(?<Page>[^""#]+).html(#(?<Anchor>[^""]+))?""", HtmlHrefMatchEvaluator, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            return html;
        }

        public static string RemoveMarkup(string markup)
        {
            // Remplace les liens
            markup = Regex.Replace(markup, LinkPattern, ExtractLinkTitle, RegexOptions.CultureInvariant);

            // Supprime les ''', ''
            markup = Regex.Replace(markup, "'{2,3}", string.Empty);

            // Supprime certains snippets
            markup = Regex.Replace(markup, @"\{s:c\}", "c", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            return markup;
        }

        public static string ExtractLinkTitle(Match match)
        {
            return match.Groups["Title"].Value.EmptyAsNull() ?? match.Groups["Name"].Value;
        }

        public static string ExtractLinkTargetName(Match match)
        {
            return match.Groups["Name"].Value.EmptyAsNull() ?? match.Groups["Title"].Value;
        }

        private static string HtmlHrefMatchEvaluator(Match match)
        {
            var page = match.Groups["Page"].Value;
            var anchor = match.Groups["Anchor"].Value;

            page = page.ToLowerInvariant();
            anchor = anchor.ToLowerInvariant();

            if (!string.IsNullOrEmpty(anchor))
            {
                return string.Format("href=\"pf://{0}#{1}\"", page, anchor);
            }
            return string.Format("href=\"pf://{0}\"", page);
        }
    }
}