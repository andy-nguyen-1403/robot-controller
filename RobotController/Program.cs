using RobotController.CommandProviders;
using RobotController.DTOs;
using RobotController.States;
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;


[assembly: InternalsVisibleTo("RobotTests")]
namespace RobotController
{
    class Program
    {
        static void Main(string[] args)
        {
            
            ICommandProvider cp;
            string[] providerArgs;
            if (args.Length > 0 && string.Equals(args[0], "dynamic", StringComparison.OrdinalIgnoreCase))
            {
                cp = new DynamicCommandProvider(new IMetaCommandProvider[]
                {
                    new ConsoleCommandProvider(),
                    new FileCommandProvider(),
                    new AdvancedHttpCommandProvider()
                });

                providerArgs = Array.Empty<string>();

                Console.WriteLine("Robot is not placed on the map and is waiting for commands.");
                Console.WriteLine("Please type one of the supported commands from the robot manual.");
                Console.WriteLine("Meta-commands: :console, :file <path>, :api <url>, :quit");
            }
            else
            {
                cp = new ConsoleCommandProvider();
                providerArgs = Array.Empty<string>();
            }

            var map = new Map(10, 10);
            var robot = new AdvancedRobot(map);
            robot.CurrentState = new IdleState(robot);

            foreach (var command in cp.GetCommands(providerArgs))
            {
                Console.WriteLine($"Executing: {command.Name}");

                robot.ExecuteCommand(command);
                Console.WriteLine($"Success: {command.Success}");
            }

            var log = new CommandExecutionLog
            {
                WorkflowId = 1,
                SchemaVersion = "2.0",
                Commands = robot.CommandHistory.Select(c => new ExecutedCommandRecord
                {
                    Name = c.Name,
                    Success = c.Success,
                    Executed = c.Executed,
                    TimeStamp = DateTime.UtcNow,
                    RobotId = "R1"
                }).ToList()
            };

            var client = new HttpClient();
            client.PostAsJsonAsync("http://localhost:5278/command-executions", log).Wait();
        }
    }
}
