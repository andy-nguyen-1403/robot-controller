using RobotController.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RobotController.CommandProviders
{
    internal class DynamicCommandProvider : ICommandProvider
    {
        private readonly Dictionary<string, IMetaCommandProvider> providersByMetaCommand;
        private readonly IMetaCommandProvider consoleProvider;

        private string[] currentArgs = new string[0];
        private bool shouldExit;

        public IMetaCommandProvider CurrentProvider { get; private set; }

        public DynamicCommandProvider(IReadOnlyCollection<IMetaCommandProvider> commandProviders)
        {
            if (commandProviders == null)
                throw new ArgumentNullException(nameof(commandProviders));

            if (commandProviders.Count == 0)
                throw new ArgumentException("At least one command provider must be supplied.");

            providersByMetaCommand = commandProviders.ToDictionary(
                p => p.MetaCommand,
                p => p,
                StringComparer.OrdinalIgnoreCase);

            if (!providersByMetaCommand.TryGetValue(":console", out var console))
                throw new InvalidOperationException("Console provider (:console) is required.");

            consoleProvider = console;
            CurrentProvider = consoleProvider;
        }

        public void SwitchTo(IMetaCommandProvider provider, string[] args)
        {
            CurrentProvider = provider;
            currentArgs = args ?? Array.Empty<string>();
        }

        public IEnumerable<ICommand> GetCommands(string[] args)
        {
            while (!shouldExit)
            {
                bool switchedProvider = false;

                foreach (var command in CurrentProvider.GetCommands(currentArgs))
                {
                    // HANDLE UNKNOWN → META COMMAND
                    if (command is UnknownCommand unknown &&
                        !string.IsNullOrWhiteSpace(unknown.Input))
                    {
                        var parts = unknown.Input
                            .Trim()
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        var metaCommand = parts[0];

                        // NOT a meta command
                        if (!metaCommand.StartsWith(":"))
                        {
                            Console.WriteLine("Unsupported command: " + unknown.Input);
                            continue;
                        }

                        // EXIT
                        if (metaCommand.Equals(":quit", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Exiting dynamic mode...");
                            shouldExit = true;
                            yield break;
                        }

                        // SWITCH PROVIDER
                        if (providersByMetaCommand.TryGetValue(metaCommand, out var provider))
                        {
                            SwitchTo(provider, parts.Skip(1).ToArray());

                            Console.WriteLine("Switched to provider " + metaCommand);
                            switchedProvider = true;
                            break;
                        }

                        Console.WriteLine("Unknown meta-command: " + metaCommand);
                        continue;
                    }

                    // NORMAL COMMAND → PASS TO ROBOT
                    yield return command;
                }

                if (shouldExit)
                    yield break;

                // If switched → restart loop immediately
                if (switchedProvider)
                    continue;

                // AFTER FILE/API → RETURN TO CONSOLE
                if (!ReferenceEquals(CurrentProvider, consoleProvider))
                {
                    SwitchTo(consoleProvider, Array.Empty<string>());
                    Console.WriteLine("Switched back to console.");
                }
            }
        }
    }
}