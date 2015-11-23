// -----------------------------------------------------------------------
// <copyright file="ScanConjurateurCommand.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using WikiExportParser.Wiki;

namespace WikiExportParser.Commands
{
    /// <summary>
    /// Commande temporaire utilisée pour vérifier toutes les pages qui pointent vers la page du conjurateur.
    /// </summary>
    internal class ScanConjurateurCommand : ICommand
    {
        public WikiExport Wiki { get; set; }

        public ILog Log { get; set; }

        public string Help
        {
            get { return "(debug) Analyse les pages utilisant le nom conjurateur et invocateur."; }
        }

        public string Alias
        {
            get { return "scanconjurateur"; }
        }

        public void Execute(DataSetCollection dataSets)
        {
            Console.WriteLine("Liste des pages pointant vers la page de l'invocateur :");
            foreach (var page in Wiki.Pages.Where(p => p.OutLinks.Any(l => l.FullName == "Pathfinder-RPG.Conjurateur")))
            {
                Console.WriteLine("{0}", page.Title);
            }

            Console.WriteLine();
            Console.WriteLine("Liste des pages contenant le mot conjurateur :");
            foreach (var page in Wiki.Pages.Where(p => p.Body.IndexOf("conjurateur", StringComparison.OrdinalIgnoreCase) != -1))
            {
                Console.WriteLine("{0}", page.Title);
            }

            Console.ReadKey();
        }
    }
}