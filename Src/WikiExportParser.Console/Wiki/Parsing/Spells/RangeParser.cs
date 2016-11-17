// -----------------------------------------------------------------------
// <copyright file="RangeParser.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PathfinderDb.Schema;

namespace WikiExportParser.Wiki.Parsing.Spells
{
    internal static class RangeParser
    {
        private static readonly HashSet<string> noRangeSpells = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private const string RangePattern = "'''Portée'''(?<Value>.+?)(?:</?br/?>)";

        static RangeParser()
        {
            foreach (var line in EmbeddedResources.LoadString("Resources.SpellNoRangeList.txt").Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            {
                noRangeSpells.Add(line);
            }
        }

        public static void ParseRange(string html, Spell spell, ILog log)
        {
            var match = Regex.Match(html, RangePattern, RegexOptions.CultureInvariant);

            if (!match.Success)
            {
                if (!noRangeSpells.Contains(spell.Id))
                {
                    throw new ParseException($"Portée introuvable pour l'id {spell.Id}");
                }
                // portée ignorée
                return;
            }

            var rangeValue = match.Groups["Value"].Value.Trim();

            switch (rangeValue.ToLowerInvariant())
            {
                case "[[personnelle]]":
                case "personnelle":
                case "[[présentation des sorts#portee|personnelle]]":
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Personal};
                    break;

                case "[[contact]]":
                case "contact":
                case "[[présentation des sorts#portee|contact]]":
                case "[[présentation des sorts#contact|contact]]":
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Touch};
                    break;

                case "[[illimitée]]":
                case "illimitée":
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Unlimited};
                    break;

                case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]])":
                case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) (5 cases + 1 case/2 [[niveau|niveaux]])": // Format sans snippet
                case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Format APG
                case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]])/(5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Format UM
                case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) / (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Format UM 2
                case "courte (7,50 m + 1,50 m/2 [niveau|niveaux]])/(5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Format UM (buggé!)
                case "courte (7,5 m + 1,5 m/2 [[niveau|niveaux]]) (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Format court (pas de décimale non significative)                    
                case "courte (7,50 m + 1,50 m/2 niveaux) (5 cases + 1 case/2 niveaux)":
                case "ccourte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Transmutation du sang en acide (BUG)
                    //case "courte (7,50 m + 1,50 m/2 ([niveau|niveaux]])": // Détection faussée
                    //case "courte (7,50 mètres + 1,50 mètre/2 [[niveau|niveaux]])": // Convocation de monstres I
                    //case "proche (7,50 m + 1,50 m/2 [[niveau|niveaux]])": // Délivrance de la paralysie
                    //case "courte (7,50 m/5 cases + 1,50 m/1 case par 2 [[niveau|niveaux]])": // Illumination
                    //case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]])/(5 cases + 1 case/2 [[niveau|niveaux]])": // Lenteur
                    //case "courte (7,50 m + 1,50 m/2 niveaux) / (5 cases + 1 case/2 [[niveau|niveaux]])": // Lien télépathique
                    //case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) / (5 cases + 1 case/2 [[niveau|niveaux]])": // Manipulation à distance
                    //case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) / (5 cases + 1 cases/2 [[niveau|niveaux]])": // Mauvais oeil
                    //case "courte (7,50 + 1,50 m/2 [[niveau|niveaux]])": // Profanation
                    //case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) (5 {s:c} + 1 ({s:c}))": // Vermine géante
                    //case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]] - 5 cases + 1 case/2 [[niveau|niveaux]])": // Zone de vérité
                    //case "courte (7,50 m + 1,50 m/2 niveaux) (5 {s:c} + 1 {s:c}/2 niveaux)": // Appel cacophonique
                    //case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]) (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux])": // Bouclier involontaire
                    //case "courte (7,5 m + 1,5 m/2 [[niveau|niveaux]]) (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Dangereux final
                    //case "courte (7,50 m + 1,50 m/2 niveaux)": // Négation de l'arôme
                    //case "courte (7,50 m + 1,50 m/2 niveaux) (5 cases + 1 case/2 niveaux)": // Vérole
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Close};
                    break;

                // Format standard
                case "moyenne (30 m + 3 m/[[niveau]])":
                case "moyenne (30 m + 3 m/[[niveau]]) (20 {s:c} + 2 {s:c}/[[niveau]])":
                case "moyenne (30 m + 3 m/[[niveau|niveaux]])/(20 {s:c} + 2 {s:c}/[[niveau|niveaux]])":
                case "moyenne (30 m + 3 m/[[niveau]]) / (20 {s:c} + 2 {s:c}/[[niveau]])":
                //case "moyenne (20 cases + 2 cases/[[niveau]]) (30 m + 3 m/[[niveau]])": // Extinction des feux
                //case "moyenne (30 m/20 cases + 3 m/2 cases par [[niveau]])": // Immobilisation de morts-vivants
                //case "moyenne (30 m + 3 m/[[niveau]]) / (20 cases + 2 cases/[[niveau]])": // Lueur d'arc-en-ciel
                //case "moyenne (30 m + 3 m/[[niveau]])/(20 cases + 2 cases/[[niveau]])": // Main spectrale
                //case "moyenne (30 m + 3 m/[[niveau]]) / (20 cases + 2 case/[[niveau]])": // Mur de feu
                //case "moyenne (30 m + 3 m/niveau) / (20 cases + 2 cases/[[niveau]])": // Mur de glace
                //case "moyenne (30 m + 3 m/niveau)": // Pétrification
                // Format APG                    
                case "intermédiaire (30 m + 3 m/[[niveau]]) (20 {s:c} + 2 {s:c}/[[niveau]])":
                    //case "intermédiaire (30 m + 3 m/niveau)": // Appel des pierres
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Medium};
                    break;

                case "longue (120 m + 12 m/[[niveau]])": // Format standard
                case "longue (120 m + 12 m/[[niveau]]) (80 {s:c} + 8 {s:c}/[[niveau]])": // Format APG
                case "longue (120 m + 12 m/[[niveau]]) / (80 {s:c} + 8 {s:c}/[[niveau]])": // Format UM
                    //case "longue (120 m/80 cases + 12 m/8 cases par [[niveau]])": // Image silencieuse
                    //case "longue (120 m + 12 m/[[niveau]])/(80 cases + 8 cases/[[niveau]])": // Localisation d'objets
                    //case "longue (120 m + 12 m/[[niveau]]) / (80 cases + 8 cases/[[niveau]])": // Lueur féerique
                    //case "longue (120 m + 12 m/niveau)": // Sphère glaciale
                    //case "longue (120 m + 12 m/niveau) (80  cases + 8 cases/niveau)": // Tsunami
                    //case "longue (120 m + 12 m/niveau) (80 cases) + 8 cases par niveau)": // Vortex
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Long};
                    break;

                case "0 m":
                case "0 m (0 {s:c})":
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "0"};
                    break;

                case "1,50 m": // Charmant cadeau
                case "1,50 m (1 {s:c})": // Manteau de rêves
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "1"};
                    break;

                case "3 m":
                //case "3 m - 2 cases": // Zone d'antimagie
                case "3 m (2 {s:c})": // Interdiction du fou
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "2"};
                    break;

                case "4,50 m":
                case "4,50 m (3 {s:c})":
                    //case "4,50 m/3 cases": // Mains brûlantes
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "3"};
                    break;

                case "6 m":
                //case "6 m (4 cases)": // Manteau du chaos
                case "6 m (4 {s:c})": // Final revigorant
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "4"};
                    break;

                case "9 m":
                case "9 m (6 {s:c})": // Cri strident
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "6"};
                    break;

                case "12 m":
                case "12 m (8 {s:c})": // Recherche de pensées
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "8"};
                    break;

                case "15 m/10 cases":
                case "15 m/10 {s:c}": // Imprécation
                case "15 m (10 {s:c})": // Bénédiction
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "10"};
                    break;

                case "18 mètres":
                case "18 m":
                case "18 m/12 cases":
                case "18 m (12 {s:c})":
                case "18 mètres (12 {s:c})": // Extase
                case "18 m/12 {s:c}": // Identification
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "12"};
                    break;

                case "30 m (20 {s:c})": // Joueur de flûte
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "20"};
                    break;

                case "36 m": // Sillage de lumière
                case "36 m (24 {s:c})": // Éclair
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "24"};
                    break;

                case "jusqu’à 3 m (2 {s:c})/[[niveau]]": // Champ de force
                    spell.Range = new SpellRange {SpecificValue = "jusqu'à 2 cases/niveau"};
                    break;

                case "12 m (8 {s:c})/[[niveau]]": // Contrôle des vents
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "8/level"};
                    break;

                case "15 m (10 {s:c})/[[niveau]]": // Entrer dans une image
                    spell.Range = new SpellRange {Unit = SpellRangeUnit.Squares, SpecificValue = "10/level"};
                    break;


                case "1,5 km/[[niveau]]": // Vent des murmures
                    spell.Range = new SpellRange {SpecificValue = "1,5 km/niveau"};
                    break;

                case "3 ou 9 m (2 ou 6 {s:c})": // Détonation discordante
                    spell.Range = new SpellRange {SpecificValue = "3 ou 9 m (2 ou 6 cases)"};
                    break;

                case "9 ou 18 m (6 ou 12 {s:c})": // Souffle de dragon
                    spell.Range = new SpellRange {SpecificValue = "9 ou 18 m (6 ou 12 cases)"};
                    break;

                case "voir description": // Coffre secret
                case "courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]]) (voir description)":
                case "[[présentation des sorts#portee|contact]] (voir description)": // Passage dans l'éther
                case "0 m (voir description)": // Symbole de mort
                case "3 m (voir description)": // Communication avec les apparitions
                case "spéciale (voir description)": // Enchevetrement flamboyant & Flammes de la vengeance
                //case "contact ; voir texte": // Lien sacré
                //case "voir texte": // Vague mondiale
                case "[[présentation des sorts#portee|contact]] et [[présentation des sorts#portee|illimitée]] (voir description)": // Vengeance fantasmagorique
                case "[[personnelle]] ou 1,50 m (1 {s:c}) (voir description)": // Voile d'énergie positive & Voile du paradis
                case "3 km": // Contrôle du climat
                case "7,5 km": // Main du berger
                case "1,5 km": // Oeil indiscret
                case "n’importe où dans la zone défendue": // Défense magique
                case "[[présentation des sorts#portee|personnelle]] ou [[présentation des sorts#portee|contact]]": // Invisibilité, Prémonition et Sphère d'invisibilité
                case "[[personnelle]] ou [[contact]]":
                case "[[personnelle]] ou courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Lévitation
                case "[[personnelle]] et [[contact]]": // Téléportation et Téléportation interplanétaire
                case "[[personnelle]] et [[présentation des sorts#portee|contact]]":
                case "[[personnelle]] ou [[présentation des sorts#portee|contact]]": // Liberté de mouvement, orientation
                case "9 ou 18 m": // Souffle de dragon
                case "[[contact]] (voir description)":
                case "[[présentation des sorts#portee|contact]] ou 1,50 m (1 {s:c}) (voir description)": // Manteau de colère
                case "[[contact]] ou 1,50 m (1 {s:c}) (voir description)": // Manteau de colère
                case "[[personnelle]] et courte (7,50 m + 1,50 m/2 [[niveau|niveaux]]) (5 {s:c} + 1 {s:c}/2 [[niveau|niveaux]])": // Lien des esprits combatifs
                case "[[émanation]] de 9 m (6 {s:c}) de rayon centrée sur le lanceur de sorts": // Note pétrifiante
                    spell.Range = new SpellRange
                    {
                        SpecificValue = rangeValue
                            .Replace("[[niveau|niveaux]]", "niveaux")
                            .Replace("[[", string.Empty)
                            .Replace("]]", string.Empty)
                            .Replace("{s:c}", "case(s)")
                    };
                    break;

                default:
                    log.Warning("{0}: Portée non reconnue \"{1}\"", spell.Name, rangeValue);
                    spell.Range = new SpellRange
                    {
                        SpecificValue = MarkupUtil.RemoveMarkup(rangeValue)
                    };
                    break;
            }

            if (spell.Range != null)
            {
                return;
            }

            const string meterRange = "^(\\d+) m$";

            match = Regex.Match(rangeValue, meterRange, RegexOptions.CultureInvariant);

            if (match.Success)
            {
            }
        }
    }
}