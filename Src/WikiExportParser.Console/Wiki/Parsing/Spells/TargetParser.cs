// -----------------------------------------------------------------------
// <copyright file="TargetParser.cs" organization="Pathfinder-Fr">
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
    internal static class TargetParser
    {
        private const string Pattern = @"'''(?<Type>(Cibles?|Effet|Zone d'effet|Zone|Cible ou zone d’effet|Cible et zone d’effet|Cible ou effet|Zone d'effet ou cible|Cible, effet ou zone d’effet))'''(?<Value>.+?)(?:</?br/?>)";

        private static readonly IDictionary<string, int> typeCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public static void ParseTarget(string html, Spell spell, ILog log)
        {
            var match = Regex.Match(html, Pattern, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

            if (!match.Success)
            {
                //log.Information("{0}: Pas de cible", spell.Name);
                return;
            }
            var type = match.Groups["Type"].Value;
            int count;
            typeCount.TryGetValue(type, out count);
            count++;
            typeCount[type] = count;

            var value = MarkupUtil.RemoveMarkup(match.Groups["Value"].Value.Trim());

            spell.Target = new SpellTarget
            {
                Value = value
            };
        }

        public static void Flush(ILog log)
        {
            log.Information("[Spells.TargetParser] Statistiques sur les cibles :");

            foreach (var typePair in typeCount.OrderByDescending(tc => tc.Value))
            {
                log.Information(" - {0} {1}", typePair.Value, typePair.Key);
            }
        }
    }
}