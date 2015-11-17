namespace WikiExportParser.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Expose des méthodes permettant de charger des commandes.
    /// </summary>
    public static class CommandLoader
    {
        public static IEnumerable<ICommand> LoadCommandFromAssemblyOf(Type type)
        {
            var commandType = typeof(Commands.ICommand);

            return type.Assembly.GetExportedTypes()
                .Where(t => t.Namespace == commandType.Namespace && commandType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => (ICommand)Activator.CreateInstance(t));
        }
    }
}
