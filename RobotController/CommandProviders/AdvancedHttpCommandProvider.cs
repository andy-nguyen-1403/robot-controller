using RobotController.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace RobotController.CommandProviders
{
    internal class AdvancedHttpCommandProvider : IMetaCommandProvider
    {
        public string MetaCommand => ":api";
        private readonly HttpClient httpClient;

        public AdvancedHttpCommandProvider() : this(new HttpClient()) { }

        public AdvancedHttpCommandProvider(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public IEnumerable<ICommand> GetCommands(string[] args)
        {
            if (args == null || args.Length == 0)
                yield break;

            var url = args[0];
            var response = httpClient.GetStringAsync(url).Result;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var commandSet = JsonSerializer.Deserialize<CommandSetDto>(response, options);

            if (commandSet == null)
                yield break;

            var mappedCommands = new List<ICommand>();

            foreach (var dto in commandSet.Commands)
            {
                switch (dto.Name?.ToUpper())
                {
                    case "MOVE":
                        var steps = dto.NumberOfSteps.GetValueOrDefault(1);
                        if (steps < 1) steps = 1;

                        for (int i = 0; i < steps; i++)
                            mappedCommands.Add(new MoveCommand());
                        break;

                    case "LEFT":
                        mappedCommands.Add(new LeftCommand());
                        break;

                    case "RIGHT":
                        mappedCommands.Add(new RightCommand());
                        break;

                    case "REPORT":
                        mappedCommands.Add(new ReportCommand());
                        break;

                    case "PLACE":
                        mappedCommands.Add(DeserializePlace(dto));
                        break;

                    case "JUMP_FORWARD":
                        var jump = dto.NumberOfSteps.GetValueOrDefault(2);
                        if (jump < 1) jump = 1;
                        mappedCommands.Add(new JumpForwardCommand(jump));
                        break;
                }
            }

            // KEY PART (Distinction logic)
            var mode = string.IsNullOrWhiteSpace(commandSet.ExecutionMode)
                ? "BestEffort"
                : commandSet.ExecutionMode;

            if (mode.Equals("AllOrNothing", StringComparison.OrdinalIgnoreCase))
            {
                yield return new AtomicCommand(mappedCommands);
                yield break;
            }

            foreach (var cmd in mappedCommands)
                yield return cmd;
        }

        private static PlaceCommand DeserializePlace(RobotCommandDto dto)
        {
            if (dto.X == null || dto.Y == null || string.IsNullOrWhiteSpace(dto.Direction))
                throw new ArgumentException("Invalid PLACE command from API");

            if (!Enum.TryParse<Direction>(dto.Direction, true, out var direction))
                throw new ArgumentException($"Invalid direction: {dto.Direction}");

            return new PlaceCommand(dto.X.Value, dto.Y.Value, direction);
        }
    }
}