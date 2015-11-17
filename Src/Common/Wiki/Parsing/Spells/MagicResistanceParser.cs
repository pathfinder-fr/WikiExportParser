namespace WikiExportParser.Wiki.Parsing.Spells
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Pathfinder.DataSet;
    internal static class MagicResistanceParser
    {
        private const string RemoveMarkupPattern = @"\[\[présentation des sorts#(jetsdesauvegarde|rm)\|(inoffensif)\]\]";

        private const string RMPattern = @"^(\[\[RM\|)?(?<val>oui|non)(\]\])?( \((?<word>[^,\)]+)(, (?<word>[^,\)]+))*\))?$";

        private const string Yes = "oui";

        private const string No = "non";

        public static void ParseMagicResistance(string html, Spell spell, ILog log)
        {
            const string pattern = "'''Résistance à la magie''' ";
            var i = html.IndexOf(pattern);

            var spellResist = spell.SpellResistance = new SpellResistance();

            if (i == -1)
            {
                //throw new ParseException("Résistance à la magie introuvable");
                spellResist.Resist = SpecialBoolean.No;
                return;
            }

            var eol = html.IndexOf('\n', i);

            if (eol == -1)
            {
                throw new ParseException("Résistance à la magie mal formée");
            }

            i += pattern.Length;

            // La valeur de résistance à la magie est la fin de la phrase
            var value = html.Substring(i, eol - i);

            // On conserve la valeur originale (si la détection échoue, c'est elle que l'on indiquera)
            var originalValue = value;

            // On supprime les éventuels liens indiqués en fin de RM
            value = Regex.Replace(value.ToLowerInvariant(), RemoveMarkupPattern, "$2").Trim();

            switch (value)
            {
                case No:
                    // Il ne reste plus que le texte "non" comme valeur, on le renvoie
                    spellResist.Resist = SpecialBoolean.No;
                    return;

                case Yes:
                    // Il ne reste plus que le texte "oui" comme valeur, on le renvoie
                    spellResist.Resist = SpecialBoolean.Yes;
                    return;
            }

            // On va tenter de déchiffrer la valeur spécifique de RM indiquée
            var match = Regex.Match(value, RMPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (!match.Success)
            {
                // On a pas pu détecter le type de RM, on renvoie spécial
                spellResist.Resist = SpecialBoolean.Special;
                spellResist.Text = MarkupUtil.RemoveMarkup(originalValue).Trim();
                return;
            }

            switch (match.Groups["val"].Value.ToLowerInvariant())
            {
                case Yes:
                    spellResist.Resist = SpecialBoolean.Yes;
                    break;

                case No:
                    spellResist.Resist = SpecialBoolean.No;
                    break;
            }

            foreach (var capture in match.Groups["word"].Captures.Cast<Capture>())
            {
                switch (capture.Value.ToLowerInvariant())
                {
                    case "voir description":
                    case "voir texte":
                    case "spécial":
                        spellResist.Text = value;
                        break;

                    case "inoffensif":
                        spellResist.Harmless = true;
                        break;

                    case "objet":
                    case "[[présentation des sorts#jetsdesauvegarde|objet]]":
                        spellResist.Objects = true;
                        break;

                    default:
                        // ??
                        throw new ParseException(string.Format("Détail de RM non reconnu : {0}", capture.Value));
                }
            }
        }
    }
}