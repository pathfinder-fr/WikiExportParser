// -----------------------------------------------------------------------
// <copyright file="CommandLoader.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace WikiExportParser.Commands
{
    /// <summary>
    /// Expose des méthodes permettant de charger des commandes.
    /// </summary>
    public static class CommandLoader
    {
        public static IEnumerable<ICommand> LoadCommandFromAssemblyOf(Type type)
        {
            var commandType = typeof (ICommand);

            return type.Assembly.GetExportedTypes()
                .Where(t => t.Namespace == commandType.Namespace && commandType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => (ICommand) Activator.CreateInstance(t));
        }
    }
}