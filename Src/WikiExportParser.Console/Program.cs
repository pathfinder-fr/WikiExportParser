// -----------------------------------------------------------------------
// <copyright file="Program.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WikiExportParser.Commands;
using WikiExportParser.Logging;
using WikiExportParser.Wiki;
using WikiExportParser.Writers;

namespace WikiExportParser
{
    internal class Program
    {
        private static readonly int FixedArgs = 3;

        private static List<ICommand> allCommands;

        private static void Main(string[] args)
        {
            Console.WriteLine("Pathfinder-fr Wiki Export Parser v{0}", typeof (Program).Assembly.GetName().Version);

            allCommands = new List<ICommand>();
            allCommands.AddRange(CommandLoader.LoadCommandFromAssemblyOf(typeof (ICommand)));
#if DEBUG
            allCommands.AddRange(CommandLoader.LoadCommandFromAssemblyOf(typeof (ScanConjurateurCommand)));
#endif

            if (args == null || args.Length < FixedArgs || args.Any(x => x.Equals("/help", StringComparison.OrdinalIgnoreCase)))
            {
                ShowHelp();
                return;
            }

            Console.WriteLine();

            var xmlPath = args[0];
            var xmlOut = args[1];
            var commandName = args[2];

            if (!Directory.Exists(xmlPath))
            {
                Console.WriteLine("ERREUR: Le dossier spécifié {0} n'existe pas", xmlPath);
                return;
            }

            var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var commandNames = new List<string>();

            foreach (var arg in args.Skip(FixedArgs))
            {
                if (arg[0] == '/')
                {
                    if (arg.Length > 1)
                    {
                        var i = arg.IndexOf(':');

                        if (i == -1 || i == 1 || i == arg.Length - 1)
                        {
                            options.Add(arg.Substring(1), string.Empty);
                        }
                        else
                        {
                            options.Add(arg.Substring(1, i - 1), arg.Substring(i + 1));
                        }
                    }
                }
                else
                {
                    commandNames.Add(arg);
                }
            }

            Console.WriteLine("Commandes à exécuter :");
            var commands = LoadCommands(allCommands, commandName, commandNames);
            foreach (var command in commands)
            {
                Console.WriteLine("- {0}", command.Alias);
            }

            Console.WriteLine("Chargement de l'export wiki...");
            var export = new WikiExport();
            export.Load(xmlPath);

            ILog log = new ConsoleLog();
            FileLog fileLog = null;

            try
            {
                var dataSets = new DataSetCollection();
                dataSets.Lang = "fr-FR";

                string logFileName;
                if (options.TryGetValue("log", out logFileName))
                {
                    fileLog = new FileLog(logFileName);
                    log = new CombineLog(log, fileLog);
                }

                foreach (var command in commands)
                {
                    log.Information("Exécution commande {0}", command.Alias);
                    command.Wiki = export;
                    command.Log = log;
                    command.Execute(dataSets);
                }

                // Serialisation
                var directory = Path.GetDirectoryName(xmlOut);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var writers = new IDataSetWriter[]
                {
                    new XmlDataSetWriter(),
                    new JsonDataSetWriter(),
                    new CsvDataSetWriter(),
                    new XmlSingleDataSetWriter()
                };

                foreach (var dataSet in dataSets.DataSets)
                {
                    foreach (var writer in writers.Where(w => w.Accept(dataSet.Key, dataSet.Value, options)))
                    {
                        writer.Write(dataSet.Key, dataSet.Value, xmlOut);
                    }
                }
            }
            finally
            {
                if (fileLog != null)
                {
                    fileLog.Dispose();
                }
            }

#if DEBUG
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
#endif
        }

        private static void ShowHelp()
        {
            Console.WriteLine("usage: WikiExportParser <in> <out> <command> [<command>]* <options...>");
            Console.WriteLine();
            Console.WriteLine(" in       Chemin (dossier) contenant les espaces de nom des fichiers du wiki.");
            Console.WriteLine("          Il s'agit en général d'un dossier \"Out\" extrait depuis le fichier");
            Console.WriteLine("          d'export wiki disponible à l'adresse http://db.pathfinder-fr.org/raw/WikiXml.7z");
            Console.WriteLine(" out      Dossier où les fichiers XML seront générés.");
            Console.WriteLine(" command  Commande(s) à exécuter.");
            Console.WriteLine(" options  Options supplémentaires");
            Console.WriteLine();
            Console.WriteLine("Commandes disponibles :");
            Console.WriteLine();
            var maxAlias = allCommands.Max(x => x.Alias.Length);

            var maxLineWidth = Console.WindowWidth - maxAlias - 3;

            foreach (var command in allCommands)
            {
                Console.Write(" {0}{1}", command.Alias, new string(' ', maxAlias + 1 - command.Alias.Length));
                var help = command.Help;
                var i = 0;
                if (help.Length < maxLineWidth)
                {
                    Console.WriteLine(help);
                }
                else
                {
                    Console.WriteLine(help.Substring(0, maxLineWidth));
                    do
                    {
                        i += maxLineWidth;
                        Console.Write(new string(' ', maxAlias + 2));
                        Console.WriteLine(help.SafeSubstring(i, maxLineWidth));
                    } while (help.Length - i > maxLineWidth);
                }
            }
            Console.WriteLine();
            Console.WriteLine("Options disponibles :");
            Console.WriteLine();
            Console.WriteLine(" /log:[file]  Ecrit le journal de génération des données dans le fichier indiqué");
            Console.WriteLine(" /csv         Génère les données au format CSV. Par défaut, les données ne sont générées qu'au format XML");
        }

        private static List<ICommand> LoadCommands(IList<ICommand> allCommands, string commandName, IEnumerable<string> commandNames)
        {
            var commands = new List<ICommand>();

            var command = allCommands.FirstOrDefault(c => c.Alias.Equals(commandName, StringComparison.OrdinalIgnoreCase));
            if (command != null)
            {
                commands.Add(command);
            }

            foreach (var additionnalCommandName in commandNames)
            {
                var additionnalCommand = allCommands.FirstOrDefault(c => c.Alias.Equals(additionnalCommandName, StringComparison.OrdinalIgnoreCase));
                if (additionnalCommand != null)
                {
                    commands.Add(additionnalCommand);
                }
            }

            return commands;
        }
    }
}