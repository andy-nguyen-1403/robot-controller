using RobotController.Commands;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net.Security;

namespace RobotController.CommandProviders
{
    internal class HttpCommandProvider : IMetaCommandProvider
    {
        public string MetaCommand => ":http";
        private readonly HttpClient httpClient;

        public HttpCommandProvider() : this(new HttpClient()) { }

        public HttpCommandProvider(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public IEnumerable<ICommand> GetCommands(string[] args)
        {
            if (args == null || args.Length == 0)
                yield break;

            var url = args[0];

            var response = httpClient.GetStringAsync(url).Result;

            if (string.IsNullOrWhiteSpace(response))
                yield break;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var commandSet = JsonSerializer.Deserialize<CommandSetDto>(response, options);

            if (commandSet == null)
                yield break;

            foreach (var cmd in commandSet.Commands)
            {
                switch (cmd.Name?.ToUpper())
                {
                    case "MOVE":
                        var steps = cmd.NumberOfSteps.GetValueOrDefault(1);
                        if (steps < 1) steps = 1;

                        for (var i = 0; i < steps; i++)
                            yield return new MoveCommand();
                        
                        break;

                    case "LEFT":
                        yield return new LeftCommand();
                        break;

                    case "RIGHT":
                        yield return new RightCommand();
                        break;

                    case "REPORT":
                        yield return new ReportCommand();
                        break;

                    case "PLACE":
                        yield return DeserializePlace(cmd);
                        break;

                    case "JUMP_FORWARD":
                        var jump = cmd.NumberOfSteps.GetValueOrDefault(2);
                        if (jump < 1) jump = 1;
                        
                        yield return new JumpForwardCommand(jump);
                        break;
                }
            }
        }
        
        private static PlaceCommand DeserializePlace(RobotCommandDto dto)
        {
            if (dto.X == null || dto.Y == null)
                throw new ArgumentException("Invalid PLACE command");
            
            var direction = Enum.Parse<Direction>(dto.Direction, true);
            return new PlaceCommand(dto.X.Value, dto.Y.Value, direction);
        }
    }  

    public class RobotCommandDto
    {
        public string Name { get; set; }
        public bool? IsMoveCommand { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public string Direction { get; set; }
        public string Comment { get; set; }
        public int? NumberOfSteps { get; set; }
    }

    public class CommandSetDto
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public string SchemaVersion { get; set; }
        public string ExecutionMode { get; set; }
        public List<RobotCommandDto> Commands { get; set; } = new List<RobotCommandDto>();
        public CommandSetDto()
        {
            Commands = new List<RobotCommandDto>();
        }
    }
}
