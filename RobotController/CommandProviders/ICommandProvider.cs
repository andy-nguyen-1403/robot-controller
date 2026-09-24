using RobotController.Commands;
using System.Collections.Generic;

namespace RobotController.CommandProviders
{
    /// <summary>
    /// Common interface for all command providers (Console, File, HTTP).
    /// </summary>
    public interface ICommandProvider
    {
        IEnumerable<ICommand> GetCommands(string[] args);
    }

    public interface IMetaCommandProvider : ICommandProvider
    {
    string MetaCommand { get; }
    }
}