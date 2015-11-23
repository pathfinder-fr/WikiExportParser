// -----------------------------------------------------------------------
// <copyright file="DescriptorParser.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using System.Text.RegularExpressions;
using PathfinderDb.Schema;

namespace WikiExportParser.Wiki.Parsing.Spells
{
    internal static class DescriptorParser
    {
        private const string DescriptorPattern = @"\(\[\[registre\|(?<Title>[^\]]+)\]\]\)";

        private const string DescriptorPattern2 = @"<nowiki>\[</nowiki>\[\[registre\|(?<Title>[^\]]+)\]\]<nowiki>\]</nowiki>";

        public static void ParseDescriptor(string html, Spell spell, ILog log)
        {
            var match = Regex.Match(html, DescriptorPattern, RegexOptions.CultureInvariant);

            if (!match.Success)
            {
                match = Regex.Match(html, DescriptorPattern2, RegexOptions.CultureInvariant);
            }

            if (match.Success)
            {
                foreach (var descriptor in match.Groups["Title"].Value.Split(',').Select(d => d.Trim().ToLowerInvariant()))
                {
                    spell.Descriptor |= ParseDescriptorValue(descriptor);
                }
            }
        }

        private static SpellDescriptors ParseDescriptorValue(string registry)
        {
            switch (registry.ToLowerInvariant())
            {
                case "acide":
                    return SpellDescriptors.Acid;

                case "air":
                    return SpellDescriptors.Air;

                case "chaos":
                    return SpellDescriptors.Chaotic;

                case "froid":
                    return SpellDescriptors.Cold;

                case "obscurité":
                    return SpellDescriptors.Darkness;

                case "mort":
                    return SpellDescriptors.Death;

                case "terre":
                    return SpellDescriptors.Earth;

                case "électricité":
                    return SpellDescriptors.Electricity;

                case "mal":
                    return SpellDescriptors.Evil;

                case "peur":
                    return SpellDescriptors.Fear;

                case "feu":
                    return SpellDescriptors.Fire;

                case "force":
                    return SpellDescriptors.Force;

                case "bien":
                    return SpellDescriptors.Good;

                case "guérison":
                    return SpellDescriptors.Healing;

                case "langage":
                    return SpellDescriptors.LanguageDependent;

                case "loi":
                    return SpellDescriptors.Lawful;

                case "lumière":
                    return SpellDescriptors.Light;

                case "mental":
                    return SpellDescriptors.MindAffecting;

                case "ombre":
                    return SpellDescriptors.Shadow;

                case "son":
                case "sonore":
                    return SpellDescriptors.Sonic;

                case "téléportation":
                    return SpellDescriptors.Teleportation;

                case "eau":
                    return SpellDescriptors.Water;

                case "malédiction":
                    return SpellDescriptors.Curse;

                case "maladie":
                    return SpellDescriptors.Disease;

                case "émotion":
                case "émotions":
                    return SpellDescriptors.Emotion;

                case "douleur":
                    return SpellDescriptors.Pain;

                case "poison":
                    return SpellDescriptors.Poison;

                case "voir texte": // cas du sort "Réprobation"
                case "variable": // case des sorts "Tir dénergie à larme de siège supérieur" et "Tir d'énergie à l'arme de siège"
                    return SpellDescriptors.None;

                default:
                    throw new ParseException(string.Format("Registre inconnu : {0}", registry));
            }
        }
    }
}