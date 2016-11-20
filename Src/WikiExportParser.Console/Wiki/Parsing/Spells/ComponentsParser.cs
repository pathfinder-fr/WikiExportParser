// -----------------------------------------------------------------------
// <copyright file="ComponentsParser.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PathfinderDb.Schema;

namespace WikiExportParser.Wiki.Parsing.Spells
{
    internal static class ComponentsParser
    {
        private const string Pattern = @"'''Composantes''' \[\[COMPOSANTES\|(?<Value>[^\]]+)\]\](?<Comment>.+?)?(?:</?br/?>)";

        private static readonly HashSet<string> noComponentSpells = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static ComponentsParser()
        {
            foreach (var line in EmbeddedResources.LoadString("Resources.SpellNoRangeList.txt").Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            {
                noComponentSpells.Add(line);
            }
        }

        public static void ParseComponents(string html, Spell spell, ILog log)
        {
            var match = Regex.Match(html, Pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                if (!noComponentSpells.Contains(spell.Id))
                {
                    throw new ParseException($"Composantes introuvables pour l'id {spell.Id}");
                }
                // composantes ignorée
                return;
            }

            spell.Components = new SpellComponents();

            var components = match.Groups["Value"].Value.Split(',').Select(s => s.Trim().ToLowerInvariant());

            foreach (var c in components)
            {
                switch (c)
                {
                    case "m":
                        spell.Components.Kinds |= SpellComponentKinds.Material;
                        break;

                    case "v":
                        spell.Components.Kinds |= SpellComponentKinds.Verbal;
                        break;

                    case "g":
                    case "s": // Désactiver pour trouver tous les sorts de l'APG avec le bug
                        spell.Components.Kinds |= SpellComponentKinds.Somatic;
                        break;

                    case "fd":
                        spell.Components.Kinds |= SpellComponentKinds.DivineFocus;
                        break;

                    case "f":
                        spell.Components.Kinds |= SpellComponentKinds.Focus;
                        break;

                    case "m/fd":
                    case "fd/m":
                        spell.Components.Kinds |= SpellComponentKinds.MaterialOrDivineFocus;
                        break;

                    case "f/fd":
                    case "fd/f":
                        spell.Components.Kinds |= SpellComponentKinds.FocusOrDivineFocus;
                        break;

                    default:
                        throw new ParseException("Formule de composantes inconnue");
                }
            }

            var comment = match.Groups["Comment"].Value.Trim();

            if (comment.Length > 1)
            {
                if (comment[0] == '(' && comment[comment.Length - 1] == ')')
                {
                    comment = comment.Substring(1, comment.Length - 2);
                }
            }

            spell.Components.Description = MarkupUtil.RemoveMarkup(comment);
        }
    }
}