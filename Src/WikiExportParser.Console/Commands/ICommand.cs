namespace WikiExportParser.Commands
{
    /// <summary>
    /// Describe a command, which can operate on a <see cref="Wiki"/> to populate one or more <see cref="Pathfinder.DataSet.DataSet"/>.
    /// </summary>
    public interface ICommand
    {
        ILog Log { get; set; }

        Wiki.WikiExport Wiki { get; set; }

        string Help { get; }

        string Alias { get; }

        void Execute(DataSetCollection dataSets);
    }
}
