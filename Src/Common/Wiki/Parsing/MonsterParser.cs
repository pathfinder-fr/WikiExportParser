namespace WikiExportParser.Wiki.Parsing
{
    using Pathfinder.DataSet;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class MonsterParser
    {
        private static readonly Regex bdStart = new Regex(@"^<div class=""BD"">\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex bdEnd = new Regex(@"^</div>", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex pucemSnippet = new Regex(@"{s:pucem\|(?<type>[^|}]*)\|(?<environment>[^|}]*)\|(?<climate>[^|}]*)}", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private static readonly Regex bdTitreSnippet = new Regex(@"{s:BDTitre\|(?<Name>[^|}]+)\|?(?<FP>[^|}]*)}", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private static readonly Regex bdTextMagicalItemSnippet = new Regex(@"({s:BDTexte\||\* )'''NLS''' \d+ ; '''Prix''' [\d ]+ p[poeac]}?", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private static readonly Regex bdTextSource = new Regex(@"^({s:BDTexte\||\* )''Source(?<Plural>s)? : (?<Value>[^}\n]+)}?$", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        private readonly Dictionary<string, int> typeDebug = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private readonly ILog log;

        private WikiPage page;

        private ElementSource[] sources;

        private int i;

        public MonsterParser(ILog log)
        {
            this.log = log;
        }

        public List<Monster> ParseAll(WikiPage page, string raw)
        {
            this.sources = null;
            this.page = page;
            this.i = 0;

            var result = new List<Monster>();

            Match nextBdStart;
            while ((nextBdStart = bdStart.Match(raw, i)).Success)
            {
                Match end = bdEnd.Match(raw, nextBdStart.Index);

                if (!end.Success)
                {
                    // Impossible de détecter la balise de fin du bloc
                    throw new InvalidOperationException("Impossible de détecter le bloc de fin du bloc BD");
                }

                var rawBloc = raw.Substring(nextBdStart.Index, end.Index - nextBdStart.Index);

                try
                {
                    var monster = this.Parse(page, rawBloc);
                    if (monster != null)
                    {
                        result.Add(monster);
                    }

                }
                catch (Exception ex)
                {
                    throw new ParseException(string.Format("Impossible de décoder le bloc {0}", result.Count + 1), ex);
                }

                this.i = end.Index;
            }

            return result;
        }

        private Monster Parse(WikiPage page, string rawBloc)
        {
            this.sources = null;
            this.page = page;

            var monster = new Monster();

            var blocSources = ParseSources(rawBloc);
            if (blocSources != null && this.sources == null)
            {
                this.sources = blocSources;
            }

            if (!ParseNameAndCR(rawBloc, monster))
            {
                return null;
            }

            if (!ParsePuce(rawBloc, monster))
                return null;

            if (blocSources != null)
            {
                monster.Sources = blocSources;
            }

            monster.Id = Ids.Normalize(page.WikiName.Name);

            return monster;
        }

        internal void Flush()
        {
        }

        private bool ParseNameAndCR(string rawBloc, Monster monster)
        {
            var bdTitre = bdTitreSnippet.Match(rawBloc);

            if (!bdTitre.Success)
            {
                // On va vérifier si ça n'est pas un objet magique
                if (bdTextMagicalItemSnippet.IsMatch(rawBloc))
                {
                    return false;
                }

                throw new InvalidOperationException("Impossible de détecter la balise titre");
            }

            monster.Name = bdTitre.Groups["Name"].Value;
            if (monster.Name == string.Empty)
            {
                // Impossible de lire le nom
                return false;
            }

            var i = monster.Name.IndexOf('(');
            if (i != -1)
            {
                monster.Name = monster.Name.Substring(0, 2).TrimEnd();
            }

            var fpText = bdTitre.Groups["FP"].Value;
            if (fpText == string.Empty || !fpText.StartsWith("fp", StringComparison.OrdinalIgnoreCase))
            {
                // Impossible de lire le FP
                return false;
            }

            fpText = fpText.Substring(2).TrimStart();

            if (fpText == "1/2")
            {
                fpText = "0.5";
            }
            else if (fpText == "1/4")
            {
                fpText = "0.25";
            }
            else if (fpText == "1/3")
            {
                fpText = "0.33";
            }
            else if (fpText == "1/8")
            {
                fpText = "0.125";
            }
            else if (fpText == "1/6")
            {
                fpText = "0.16";
            }

            try
            {
                monster.CR = decimal.Parse(fpText, NumberStyles.None | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Impossible de lire le FP {0}", fpText), ex);
            }

            return true;
        }

        private bool ParsePuce(string rawBloc, Monster monster)
        {
            var match = pucemSnippet.Match(rawBloc);

            if (!match.Success)
            {
                this.Warning("Impossible de détecter la balise pucem");
                return true;
            }

            monster.Type = ParseType(match.Groups["type"].Value);
            monster.Environment = ParseEnvironment(match.Groups["environment"].Value);
            monster.Climate = ParseClimate(match.Groups["climate"].Value);

            return true;
        }

        private CreatureType ParseType(string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "aberration": return CreatureType.Aberration;
                case "animal": return CreatureType.Animal;
                case "créature artificielle": return CreatureType.Construct;
                case "créature magique": return CreatureType.MagicalBeast;
                case "dragon": return CreatureType.Dragon;
                case "extérieur": return CreatureType.Outsider;
                case "fée": return CreatureType.Fey;
                case "humanoïde": return CreatureType.Humanoid;
                case "humanoïde monstrueux": return CreatureType.MonstrousHumanoid;
                case "mort-vivant": return CreatureType.Undead;
                case "plante": return CreatureType.Plant;
                case "vase": return CreatureType.Ooze;
                case "vermine": return CreatureType.Vermin;
                case "": return CreatureType.Other;
                default:
                    this.Warning("Valeur puce type inconnue : '{0}'", type);
                    return CreatureType.Other;
            }
        }

        private CreatureEnvironment ParseEnvironment(string environment)
        {
            switch (environment.ToLowerInvariant())
            {
                case "aquatique": return CreatureEnvironment.Aquatic;
                case "ciel": return CreatureEnvironment.Sky;
                case "collines": return CreatureEnvironment.Hills;
                case "désert": return CreatureEnvironment.Desert;
                case "forêt-jungle": return CreatureEnvironment.ForestJungle;
                case "marais": return CreatureEnvironment.Swamp;
                case "montagnes": return CreatureEnvironment.Mountains;
                case "plaines": return CreatureEnvironment.Plains;
                case "ruines-donjons": return CreatureEnvironment.RuinsDungeons;
                case "souterrain": return CreatureEnvironment.Underground;
                case "ville": return CreatureEnvironment.Urban;
                case "": return CreatureEnvironment.Unknown;
                default:
                    this.Warning("Valeur puce environnement inconnue : '{0}'", environment);
                    return CreatureEnvironment.Unknown;
            }
        }

        private CreatureClimate ParseClimate(string climate)
        {
            switch (climate.ToLowerInvariant())
            {
                case "extraplanaire": return CreatureClimate.Planar;
                case "froid": return CreatureClimate.Cold;
                case "tempéré": return CreatureClimate.Temperate;
                case "tropical": return CreatureClimate.Warm;
                case "": return CreatureClimate.Other;
                default:
                    this.Warning("Valeur puce climat inconnue : '{0}'", climate);
                    return CreatureClimate.Other;
            }
        }

        private ElementSource[] ParseSources(string rawText)
        {
            var match = bdTextSource.Match(rawText);
            if (match.Success)
            {
                var value = match.Groups["Value"].Value;
                var isPlural = match.Groups["Plural"].Success;

                if (isPlural)
                {
                    return value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(ParseSource).Where(s => s != null).ToArray();
                }

                var source = ParseSource(value);

                if (source != null)
                {
                    return new[] { source };
                }

                return null;

            }
            else if (this.sources != null)
            {
                return this.sources;
            }

            return null;
        }

        private ElementSource ParseSource(string value)
        {
            value = value.Trim().Replace("''", string.Empty);

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var match = Regex.Match(value, @"^(?<Name>[^,]+)[,\s]+(p[\. ]{0,2}|pages? )(?<Page>\d+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var source = ParseSourceId(match.Groups["Name"].Value);
                return source;
            }
            else
            {
                var rawSrc = this.ParseSourceId(value, false);
                if (rawSrc != null)
                {
                    return rawSrc;
                }

                this.Warning("Impossible de lire la source \"{0}\"", value);
                return null;
            }
        }

        private ElementSource ParseSourceId(string name, bool logWarning = true)
        {
            switch (name.ToLowerInvariant())
            {
                case "bestiaire": return new ElementSource { Id = Source.Ids.Bestiary };
                case "bestiaire 2": return new ElementSource { Id = Source.Ids.Bestiary2 };
                case "bestiaire 3": return new ElementSource { Id = Source.Ids.Bestiary3 };
                case "art de la magie": return new ElementSource { Id = Source.Ids.UltimateMagic };
                default:
                    if (logWarning)
                    {
                        this.Warning("Source inconnue : {0}", name);
                    }
                    return null;
            }
        }

        private void Warning(string format, params object[] args)
        {
            this.log.Warning("Page \"{0}\" (id {1}): {2}", this.page.Title, this.page.Id, string.Format(format, args));
        }
    }
}
