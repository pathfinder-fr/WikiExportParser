namespace WikiExportParser.Wiki.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Pathfinder.DataSet;

    partial class FeatParser
    {
        private class PrerequisiteParser
        {
            private const RegexOptions DefaultOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;

            private const string AttributePattern = @"^\[\[((?:force\|(?<Attr>for))|(?:dextérité\|(?<Attr>dex))|(?:constitution\|(?<Attr>con))|(?:intelligence\|(?<Attr>int))|(?:sagesse\|(?<Attr>sag))|(?:charisme\|(?<Attr>cha)))\]\] (?<Value>\d+)$";

            private const string AttributeShortPattern = @"^(?:\[\[)?(?:(?<Attr>for)|(?<Attr>dex)|(?<Attr>con)|(?<Attr>int)|(?<Attr>sag)|(?<Attr>cha))(?:\]\])?\s(?<Value>\d+)$";

            private const string BbaPattern = @"^(?:\[\[)?BBA(?:\|bonus de base à l[’']attaque)?(?:\]\])?\s(?:de\s)?\+(?<Value>\d+)$";

            private static readonly string[] ClassLevelPatterns = new string[] {
                @"^\[\[(?<Class>[^\]]+)\]\]\s(de\s)?(\[\[)?niveau(\]\])?\s(?<Level>\d+)$",
                @"^\[\[(?<Class>[^\]]+)\]\] (?<Level>\d+)$",
            };

            private static readonly string[] SpellCasterLevelPatterns = new string[] {
                @"^lanceur de sorts (de )?niveau (?<Level>\d+)$",
                @"^\[\[Niveau\]\] (?<Level>\d+) de \[\[NLS\|lanceur de sorts\]\]$",
                @"^\[\[Niveau\]\] (?<Level>\d+) de \[\[Lancer des sorts#NLS\|lanceur de sorts\]\]$",
                @"^\[\[NLS\]\] (?<Level>\d+)$",
                @"^capacité de lancer des sorts de (?<Level>\d+)<sup>(ème|er)</sup> niveau$"
            };

            private static readonly string[] SkillRankLevelPatterns = new string[] {
                @"^(?<Ranks>\d+) (?:\[\[)?rangs?(?:\]\])? en \[\[(?<Skill>[^\]]+)\]\]",
                @"^\[\[(?<Skill>[^\]]+)\]\]\s(?<Ranks>\d+)\srangs?$",
                @"^(?<Ranks>\d+) (?:\[\[)?rang\|rangs?(?:\]\])? en \[\[(?<Skill>[^\]]+)\]\]",
                @"^\[\[(?<Skill>[^\]]+)\]\] \((?<Speciality>[^)]+)\) (?<Ranks>\d+) rangs$",
            };

            private static readonly string[] ClassPowerPatterns = new[] {
                @"^(pouvoir|capacité|aptitude) de classe (de\s)?\[\[([^|]+\|)?(?<Name>[^]]+)\]\]$"
            };

            private static readonly string[] SpellCastPatterns = new[] {
                @"^''" + MarkupUtil.LinkPattern + "''$",
            };

            private static HashSet<string> otherTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            private readonly ILog log;

            private readonly WikiPage page;

            public PrerequisiteParser(WikiPage page, ILog log)
            {
                this.page = page;
                this.log = log ?? Logging.NullLog.Instance;
            }

            static PrerequisiteParser()
            {
                foreach (var line in EmbeddedResources.LoadString("Resources.FeatPrerequisiteOtherTypes.txt").Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    otherTypes.Add(line);
                }
            }

            public IFeatPrerequisiteItem Parse(string markup, WikiExport export, bool hasSemiColons)
            {
                if (markup.EndsWith("."))
                {
                    // Si le prérequis se termine par un point on le supprime
                    markup = markup.Substring(0, markup.Length - 1);
                }

                if (markup.IndexOf(" ou ") == -1)
                {
                    // Condition unique
                    var prerequisite = CreateInstance(markup, export);
                    prerequisite.Description = MarkupUtil.RemoveMarkup(markup);
                    return prerequisite;
                }
                else
                {
                    // Condition composée (ou)
                    var choiceContainer = new FeatPrerequisiteChoice();

                    // Si les conditions étaient séparées par un ";", on découpe la chaîne selon "," et "ou"
                    // Si les conditions n'étaient pas séparées par un ";", on découpé la chaîne selon "ou" uniquement
                    var separators = hasSemiColons ? new[] { ",", " ou " } : new[] { " ou " };

                    var choiceMarkup = markup.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).ToArray();

                    var items = new List<FeatPrerequisite>(choiceMarkup.Length);
                    foreach (var itemMarkup in choiceMarkup)
                    {
                        var item = CreateInstance(itemMarkup, export);
                        item.Description = MarkupUtil.RemoveMarkup(itemMarkup);
                        items.Add(item);
                    }

                    choiceContainer.Items = items.ToArray();

                    return choiceContainer;
                }
            }

            private FeatPrerequisite CreateInstance(string markup, WikiExport export)
            {
                var lower = markup.ToLowerInvariant();

                Match match;

                if (IsMatch(lower, AttributePattern, out match))
                {
                    return AsAttribute(match);
                }

                if (IsMatch(lower, AttributeShortPattern, out match))
                {
                    return AsAttribute(match);
                }

                if (IsMatch(lower, MarkupUtil.LinkOnlyPattern, out match))
                {
                    var target = MarkupUtil.ExtractLinkTargetName(match);

                    var targetName = new WikiName(WikiName.DefaultNamespace, target);

                    var targetPage = export.FindPage(targetName);
                    if (targetPage == null)
                    {
                        this.log.Warning("{0}: Impossible de trouver la page nommée '{1}' désignée par le lien '{2}'", this.page.Title, target, markup);
                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Other };
                    }

                    string subValue;
                    int number;
                    FeatPrerequisiteType targetType = ResolveTypeFromPageCategories(targetPage, out subValue, out number);

                    if (targetType == FeatPrerequisiteType.Other)
                    {
                        this.log.Warning(
                            "{3}: Impossible de déterminer le type de lien pour la page '{0}' désignée par le lien '{1}' avec les catégories '{2}'",
                            targetPage.Title,
                            markup,
                            string.Join(", ", targetPage.Categories.Select(c => c.Name)),
                            this.page.Title
                            );

                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Other };
                    }

                    return new FeatPrerequisite
                    {
                        Type = targetType,
                        Value = WikiName.IdFromTitle(target),
                        SubValue = subValue,
                        Number = number
                    };
                }

                if (IsMatch(markup, BbaPattern, out match))
                {
                    return new FeatPrerequisite
                    {
                        Type = FeatPrerequisiteType.BBA,
                        Number = int.Parse(match.Groups["Value"].Value)
                    };
                }

                if (IsMatch(markup, SpellCasterLevelPatterns, out match))
                {
                    return new FeatPrerequisite
                    {
                        Type = FeatPrerequisiteType.SpellcasterLevel,
                        Number = int.Parse(match.Groups["Level"].Value)
                    };
                }

                if (IsMatch(markup, SkillRankLevelPatterns, out match))
                {
                    return new FeatPrerequisite
                    {
                        Type = FeatPrerequisiteType.SkillRank,
                        Value = WikiName.IdFromTitle(match.Groups["Skill"].Value),
                        SubValue = WikiName.IdFromTitle(match.Groups["Speciality"].Value).EmptyAsNull(),
                        Number = int.Parse(match.Groups["Ranks"].Value)
                    };
                }

                if (IsMatch(markup, ClassLevelPatterns, out match))
                {
                    return new FeatPrerequisite
                    {
                        Type = FeatPrerequisiteType.ClassLevel,
                        Value = WikiName.IdFromTitle(match.Groups["Class"].Value),
                        Number = int.Parse(match.Groups["Level"].Value)
                    };
                }

                if (IsMatch(markup, ClassPowerPatterns, out match))
                {
                    return new FeatPrerequisite
                    {
                        Type = FeatPrerequisiteType.Other,
                        OtherType = FeatPrerequisiteOtherTypes.ClassPower,
                        Value = WikiName.IdFromTitle(match.Groups["Name"].Value)
                    };
                }

                if (IsMatch(markup, SpellCastPatterns, out match))
                {
                    return new FeatPrerequisite
                    {
                        Type = FeatPrerequisiteType.SpellCast,
                        Value = WikiName.IdFromTitle(MarkupUtil.ExtractLinkTargetName(match)),
                    };
                }

                if (otherTypes.Contains(markup))
                {
                    return new FeatPrerequisite { Type = FeatPrerequisiteType.Other };
                }

                switch (lower)
                {
                    case "pouvoir de classe ancrage":
                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Other, OtherType = FeatPrerequisiteOtherTypes.ClassPower, Value = WikiName.IdFromTitle("Ancrage") };

                    case "pouvoir de classe [[oracle#mystere|mystère]]":
                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Other, OtherType = FeatPrerequisiteOtherTypes.ClassPower, Value = WikiName.IdFromTitle("Mystère") };

                    case "[[maniement dune arme exotique|maniement d'une arme exotique]] (filet)":
                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Other, OtherType = FeatPrerequisiteOtherTypes.ExoticWeaponProficiency, Value = WikiName.IdFromTitle("filet") };

                    case "[[arme de prédilection]] pour l’arme choisie":
                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Feat, Value = WikiName.IdFromTitle("arme de prédilection") };

                    case "[[arme de prédilection]] (bâton)":
                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Feat, Value = WikiName.IdFromTitle("arme de prédilection"), SubValue = WikiName.IdFromTitle("bâton") };

                    case "aptitude de classe de [[représentation bardique]]":
                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Other, OtherType = FeatPrerequisiteOtherTypes.ClassPower, Value = WikiName.IdFromTitle("représentation bardique") };

                    case "capacité de classe de [[canalisation|canalisation d’énergie]]":
                    case "capacité de classe permettant de [[canalisation|canaliser de l’énergie négative]]":
                    case "capacité de [[classe]] à [[canalisation|canaliser de l’énergie]]":
                    case "capacité de [[classe]] permettant de [[canalisation|canaliser de l’énergie]]":
                    case "Capacité de classe permettant de canaliser de l’énergie":
                        return new FeatPrerequisite { Type = FeatPrerequisiteType.Other, OtherType = FeatPrerequisiteOtherTypes.ClassPower, Value = WikiName.IdFromTitle("canalisation") };
                }

                this.log.Warning(string.Format("{1}: Condition '{0}' non reconnue", markup, this.page.Title));
                return new FeatPrerequisite { Type = FeatPrerequisiteType.Other };
            }

            private FeatPrerequisiteType ResolveTypeFromPageCategories(WikiPage page, out string subValue, out int number)
            {
                subValue = null;
                number = 0;
                foreach (var category in page.Categories)
                {
                    switch (category.Name.ToLowerInvariant())
                    {
                        case "don":
                            return FeatPrerequisiteType.Feat;

                        case "race":
                            return FeatPrerequisiteType.Race;

                        case "classe":
                            number = 1;
                            return FeatPrerequisiteType.ClassLevel;

                        case "sort":
                            return FeatPrerequisiteType.SpellCast;

                        case "monstre":
                            return FeatPrerequisiteType.MonsterRace;
                    }
                }

                return FeatPrerequisiteType.Other;
            }

            private bool IsMatch(string markup, string[] patterns, out Match match)
            {
                match = null;
                foreach (var pattern in patterns)
                {
                    if (IsMatch(markup, pattern, out match))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool IsMatch(string markup, string pattern, out Match match)
            {
                match = Regex.Match(markup, pattern, DefaultOptions);
                return match.Success;
            }

            private FeatPrerequisite AsBba(Match match)
            {
                return new FeatPrerequisite
                {
                    Type = FeatPrerequisiteType.BBA,
                    Number = int.Parse(match.Groups["Value"].Value)
                };
            }

            private FeatPrerequisite AsFeat(Match match)
            {
                return new FeatPrerequisite
                {
                    Type = FeatPrerequisiteType.Feat,
                    Value = match.Groups["Name"].Value
                };
            }

            private FeatPrerequisite AsSpellCasterLevel(Match match)
            {
                return new FeatPrerequisite
                {
                    Type = FeatPrerequisiteType.SpellcasterLevel,
                    Number = int.Parse(match.Groups["Level"].Value)
                };
            }

            private FeatPrerequisite AsAttribute(Match match)
            {
                var attribute = CharacterAttributeUtil.FromText(match.Groups["Attr"].Value);
                var value = match.Groups["Value"].Value;

                return new FeatPrerequisite
                {
                    Type = FeatPrerequisiteType.Attribute,
                    Value = CharacterAttributeUtil.AsText(attribute),
                    Number = int.Parse(value)
                };
            }
        }
    }
}