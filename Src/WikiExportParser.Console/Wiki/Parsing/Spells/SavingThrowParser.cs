// -----------------------------------------------------------------------
// <copyright file="SavingThrowParser.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using PathfinderDb.Schema;

namespace WikiExportParser.Wiki.Parsing.Spells
{
    internal static class SavingThrowParser
    {
        private const string Pattern = @"'''Jet de sauvegarde'''(?<Value>[^;]+);";

        private const string EffectPattern = @"^(?<Type>volonté|réflexes|vigueur|oui|non)(,|\s*pour)?(?<Value>[^;]+)$";

        private const string BonusPattern = @"^(?<Effect>[^;]*?)(\s*\((?<Bonus>[^\);]+)\))*$";

        public static void ParseSavingThrow(string html, Spell spell, ILog log)
        {
            if (spell.Name == "Transmutation de potion en poison")
            {
                // Cas particulier : ce sort décrit plusieurs sorts
                return;
            }

            var match = Regex.Match(html, Pattern, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

            if (!match.Success)
            {
                return;
                throw new ParseException("Jet de sauvegarde introuvable");
            }

            var value = MarkupUtil.RemoveMarkup(match.Groups["Value"].Value.Trim());
            var compareValue = value.ToLowerInvariant();

            var savingThrow = new SpellSavingThrow();

            var copyValue = false;

            if (compareValue.IndexOf(" ou ", StringComparison.Ordinal) != -1 ||
                compareValue.IndexOf(" et ", StringComparison.Ordinal) != -1 ||
                compareValue.IndexOf(" puis ", StringComparison.Ordinal) != -1)
            {
                copyValue = true;
                savingThrow.Target = SpellSavingThrowTarget.Special;
                savingThrow.Effect = SpellSavingThrowEffect.Special;
            }
            else if ((match = Regex.Match(compareValue, EffectPattern, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)).Success)
            {
                switch (match.Groups["Type"].Value)
                {
                    case "volonté":
                        savingThrow.Target = SpellSavingThrowTarget.Will;
                        break;

                    case "réflexes":
                        savingThrow.Target = SpellSavingThrowTarget.Reflex;
                        break;

                    case "vigueur":
                        savingThrow.Target = SpellSavingThrowTarget.Fortitude;
                        break;

                    case "oui":
                        savingThrow.Target = SpellSavingThrowTarget.Special;
                        copyValue = true;
                        break;

                    case "non":
                        savingThrow.Target = SpellSavingThrowTarget.None;
                        copyValue = true;
                        break;
                }

                var effect = match.Groups["Value"].Value.Trim();

                if (effect.EndsWith(", voir texte"))
                {
                    copyValue = true;
                    effect = effect.Substring(0, effect.Length - ", voir texte".Length);
                }
                else if (effect.EndsWith(", voir description"))
                {
                    copyValue = true;
                    effect = effect.Substring(0, effect.Length - ", voir description".Length);
                }
                else if (effect.EndsWith(", voir plus bas"))
                {
                    copyValue = true;
                    effect = effect.Substring(0, effect.Length - ", voir plus bas".Length);
                }

                match = Regex.Match(effect, BonusPattern, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

                if (match.Success)
                {
                    effect = match.Groups["Effect"].Value.Trim();

                    foreach (Capture bonusCapture in match.Groups["Parenthesis"].Captures)
                    {
                        var bonus = bonusCapture.Value.Trim().ToLowerInvariant();

                        switch (bonus)
                        {
                            case "inoffensif":
                                savingThrow.Harmless = true;
                                break;

                            case "objet":
                                savingThrow.Objects = true;
                                break;

                            case "inoffensif, objet":
                            case "objet, inoffensif":
                                savingThrow.Harmless = true;
                                savingThrow.Objects = true;
                                break;

                            case "spécial, voir texte":
                            case "voir texte":
                            case "voir plus bas":
                            case "voir description":
                            case "en cas d’interaction":
                            case "en cas d'interaction":
                            case "spécial, voir plus bas":
                                copyValue = true;
                                break;

                            case "":
                                break;

                            default:
                                throw new ParseException(string.Format("Détail du jet de sauvegarde non reconnu : \"{0}\" dans le texte \"{1}\"", bonus, value));
                        }
                    }
                }

                switch (effect)
                {
                    case "annuler":
                    case "annule":
                        savingThrow.Effect = SpellSavingThrowEffect.Negates;
                        break;

                    case "1/2 dégâts":
                    case "réduit de moitié":
                    case "réduire de moitié":
                        savingThrow.Effect = SpellSavingThrowEffect.Half;
                        break;

                    case "partiel":
                    case "partielle":
                        savingThrow.Effect = SpellSavingThrowEffect.Partial;
                        break;

                    case "dévoile":
                    case "dévoiler":
                    case "dévoiler l'illusion":
                        savingThrow.Effect = SpellSavingThrowEffect.Disbelief;
                        break;

                    case "":
                        break;

                    default:
                        if (copyValue)
                        {
                            // On a déjà détecté un sort spécial, on arrête là
                        }
                        else
                        {
                            throw new ParseException(string.Format("Impossible de décoder l'effet du jet de sauvegarde \"{0}\"", effect));
                        }
                        break;
                }

                if (copyValue)
                {
                    savingThrow.SpecificValue = value;
                }
            }
            else if (compareValue.EqualsOrdinal("aucun") || compareValue.EqualsOrdinal("non"))
            {
            }
            else if (compareValue.StartsWithOrdinal("aucun"))
            {
                savingThrow.SpecificValue = value;
            }
            else if (compareValue.EqualsOrdinal("voir description") || compareValue.EqualsOrdinal("voir texte"))
            {
                savingThrow.Effect = SpellSavingThrowEffect.Special;
                savingThrow.SpecificValue = value;
            }
            else if (compareValue.EqualsOrdinal("spécial, voir plus bas"))
            {
                savingThrow.Effect = SpellSavingThrowEffect.Special;
                savingThrow.SpecificValue = value;
            }
            else if (compareValue.EqualsOrdinal("spécial, voir texte"))
            {
                savingThrow.Effect = SpellSavingThrowEffect.Special;
                savingThrow.SpecificValue = value;
            }
            else
            {
                throw new ParseException(string.Format("Impossible de décoder le type de jet de sauvegarde dans \"{0}\"", value));
            }

            spell.SavingThrow = savingThrow;
        }
    }
}