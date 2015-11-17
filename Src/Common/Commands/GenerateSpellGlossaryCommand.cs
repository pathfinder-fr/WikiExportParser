namespace WikiExportParser.Commands
{
    using System.IO;
    using System;
    using System.Linq;
    using Pathfinder.DataSet;
    using System.Text.RegularExpressions;
    using WikiExportParser.Wiki;

    /// <summary>
    /// Génère le contenu de la page du wiki du glossaire des sorts français anglais en se basant sur les informations de la base de données des sorts.
    /// </summary>
    /// <remarks>
    /// Adresse de la page du wiki : <a href="http://www.pathfinder-fr.org/Wiki/Pathfinder-RPG.Glossaire%20des%20sorts.ashx">Glossaire des sorts</a>.
    /// </remarks>
    public class GenerateSpellGlossaryCommand : SpellCommandBase, ICommand
    {
        private const string WikiUrlPrefix = "http://www.pathfinder-fr.org/Wiki/Pathfinder-RPG.";

        private const string WikiUrlSuffix = ".ashx";

        public string Help
        {
            get { return "Génère le contenu de la page du wiki du glossaire des sorts français/anglais dans un fichier SpellGlossary.txt"; }
        }

        public string Alias
        {
            get { return "spellglossary"; }
        }

        public void Execute(DataSetCollection dataSets)
        {
            var spells = this.ReadSpells();

            this.AddSpellLists(spells);

            this.GenerateIds(spells);

            // Génération page format wiki
            using (var writer = new StreamWriter("SpellGlossary.txt"))
            {
                writer.WriteLine("{s:SortTable}{s:MenuGlossaires}");
                writer.WriteLine();
                writer.WriteLine();
                writer.WriteLine("''Cliquez sur un titre de colonnes pour trier le tableau. Pour faire un tri par catégorie puis, à l'intérieur de chaque catégorie, selon un autre critère, triez d'abord selon cet autre critère (par ordre alphabétique par exemple) puis cliquez sur le titre de la colonne des catégories.");
                writer.WriteLine();
                writer.WriteLine("En cliquant sur un des noms anglais, vous serez redirigés vers le PRD officiel en anglais, sur le site de Paizo.''");
                writer.WriteLine();
                writer.WriteLine("{| CLASS=\"tablo sortable\" ID=\"tabsort\"");
                writer.WriteLine("|+ Glossaire des sorts");
                writer.WriteLine("! Anglais !! Français !! École !! Mag !! Prê !! Dru !! Rôd !! Bar !! Pal !! Alc !! Con !! Sor !! Inq !! Ora !! Apal !! Source");

                foreach (var spell in spells.OrderBy(s => AsSourceOrderKey(s.Source.Id)).ThenBy(s => s.Name))
                {
                    // Nom anglais
                    var englishName = GetLocalizedEntry(spell, "en-US", "name", "?");
                    writer.WriteLine("|-");
                    writer.Write("| class=\"gauche\" | ");
                    writer.Write(englishName);
                    var prdSource = spell.Source.References.FirstOrDefault(s => s.Name == References.PaizoPrd);
                    if (prdSource != null)
                    {
                        // Lien présent
                        writer.Write(' ');
                        writer.Write("[[{0}|PRD]]", prdSource.Href.ToString().Replace("ultimageMagic", "ultimateMagic"));
                    }
                    else if (englishName != "?")
                    {
                        // Génération nom
                        writer.Write(' ');
                        writer.Write("[[{0}|PRD]]", PrdUtils.GenerateSpellPrdUrl(englishName, spell.Source.Id));
                    }
                    writer.WriteLine();

                    // Nom français
                    var frenchName = spell.Name;
                    writer.Write("| class=\"gauche\" | ");
                    writer.Write(frenchName);
                    var wikiUrl = spell.Source.References.First(r => r.Name == References.PathfinderFrWiki).HrefString;
                    writer.Write(" [[{0}|(lien)]]", wikiUrl.Substring(WikiUrlPrefix.Length, wikiUrl.Length - WikiUrlPrefix.Length - WikiUrlSuffix.Length));
                    writer.WriteLine();
                    writer.WriteLine("| {0}", this.AsSchoolLabel(spell.School));

                    // Niveaux
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.SorcererWizard));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Cleric));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Druid));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Ranger));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Bard));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Paladin));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Alchemist));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Summoner));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Witch));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Inquisitor));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.Oracle));
                    writer.Write("| {0} |", GetSpellListLevel(spell, SpellList.Ids.AntiPaladin));

                    // Source
                    writer.WriteLine("| {0}", AsSourceLabel(spell));
                }

                writer.WriteLine("|}");
            }
        }

        private static int AsSourceOrderKey(string sourceId)
        {
            if (sourceId == Source.Ids.UltimateCombat)
            {
                return 4;
            }
            else if (sourceId == Source.Ids.UltimateMagic)
            {
                return 3;
            }
            else if (sourceId == Source.Ids.AdvancedPlayerGuide)
            {
                return 2;
            }
            else if (sourceId == Source.Ids.PaizoBlog)
            {
                return 99;
            }
            else
            {
                return 1;
            }
        }

        private static string GetLocalizedEntry(Spell spell, string language, string href, string defaultValue = null)
        {
            if (spell.Localization == null)
                return defaultValue;

            return spell.Localization.GetLocalizedEntry(language, href, defaultValue);
        }

        private string AsSourceLabel(Spell spell)
        {
            switch (spell.Source.Id)
            {
                case Source.Ids.PathfinderRpg: return "PHB";
                case Source.Ids.AdvancedPlayerGuide: return "APG";
                case Source.Ids.UltimateMagic: return "UM";
                case Source.Ids.UltimateCombat: return "UC";
                default: return string.Empty;
            }
        }

        private string AsSchoolLabel(SpellSchool school)
        {
            switch (school)
            {
                case SpellSchool.Abjuration: return "Abj";
                case SpellSchool.Conjuration: return "Inv";
                case SpellSchool.Divination: return "Div";
                case SpellSchool.Enchantment: return "Enc";
                case SpellSchool.Evocation: return "Évo";
                case SpellSchool.Illusion: return "Ill";
                case SpellSchool.Necromancy: return "Nec";
                case SpellSchool.Transmutation: return "Tra";
                case SpellSchool.Universal: return "Uni";
                default: throw new NotSupportedException(school.ToString());
            }
        }

        private static string GetSpellListLevel(Spell spell, string spellListId)
        {
            var level = spell.Levels.FirstOrDefault(l => l.List == spellListId);

            return level != null ? level.Level.ToString() : string.Empty;
        }
    }
}
