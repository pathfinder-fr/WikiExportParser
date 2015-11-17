namespace WikiExportParser.Wiki.Parsing.Spells
{
    using System.Text.RegularExpressions;
    using Pathfinder.DataSet;

    internal static class CastingTimeParser
    {
        private const string Pattern = "'''Temps d[’']incantation''' (?<Value>.*?)(?:</?br/?>)";

        private const string ValuePattern = "^(?<Quantity>\\d+) (\\[\\[)?(?<Unit>[^\\]]+)(\\]\\])?$";

        public static void ParseCastingTime(string html, Spell spell, ILog log)
        {
            var match = Regex.Match(html, Pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                throw new ParseException("Impossible de détecter le temps d'incantation");
            }

            var castingTime = spell.CastingTime = new SpellCastingTime();

            var value = match.Groups["Value"].Value.Trim();

            match = Regex.Match(value, ValuePattern, RegexOptions.CultureInvariant);

            if (match.Success)
            {
                castingTime.Value = int.Parse(match.Groups["Quantity"].Value);

                var unit = match.Groups["Unit"].Value.ToLowerInvariant();

                if (unit.IndexOf("voir description") != -1)
                    castingTime.Unit = TimeUnit.Special;
                else
                    castingTime.Unit = ParseTimeUnit(unit);
            }

            if (!match.Success || castingTime.Unit == TimeUnit.Special)
            {
                castingTime.Value = 0;
                castingTime.Unit = TimeUnit.Special;
                castingTime.Text = MarkupUtil.RemoveMarkup(value);
            }
        }

        private static TimeUnit ParseTimeUnit(string unit)
        {
            switch (unit)
            {
                case "action simple":
                case "action simple|actions simples":
                    return TimeUnit.SimpleAction;

                case "action immédiate":
                    return TimeUnit.ImmediateAction;

                case "action rapide":
                    return TimeUnit.SwiftAction;

                case "action complexe":
                    return TimeUnit.FullRoundAction;

                case "round":
                case "round|rounds":
                    return TimeUnit.Round;

                case "minute":
                case "minutes":
                    return TimeUnit.Minute;

                case "heure":
                case "heures":
                    return TimeUnit.Hour;

                case "jour":
                case "jours":
                    return TimeUnit.Day;

                case "minute/500 g":
                    return TimeUnit.Special;

                case "minute par page":
                    return TimeUnit.Special;

                default:
                    throw new ParseException(string.Format("Unité de temps {0} inconnue", unit));

            }
        }
    }
}