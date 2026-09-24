using RobotController;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RobotController.Commands
{
    internal class AtomicCommand : ICommand
    {
        private readonly List<ICommand> commands;

        public AtomicCommand(List<ICommand> commands)
        {
            this.commands = commands;
        }

        public string Name => "WORKFLOW";
        public string Description => "All-or-nothing workflow execution";

        public bool Executed { get; set; }
        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;

            var real = robot as AdvancedRobot;
            if (real == null)
                return Success = false;

            // =========================
            // 1. CREATE DRY RUN ROBOT
            // =========================
            var clone = AdvancedRobot.Clone(real);

            if (clone == null)
                return Success = false;

            clone.CurrentMap = real.CurrentMap;

            // =========================
            // 2. DRY RUN (VALIDATION)
            // =========================
            foreach (var cmd in commands)
            {
                var dryCmd = CloneCommand(cmd);

                clone.ExecuteCommand(dryCmd);

                if (!dryCmd.Success)
                {
                    Console.WriteLine($"[DRY FAIL] {dryCmd.Name}");
                    return Success = false;
                }
            }

            // =========================
            // 3. REAL EXECUTION
            // =========================
            foreach (var cmd in commands)
            {
                var realCmd = CloneCommand(cmd);

                real.ExecuteCommand(realCmd);

                if (!realCmd.Success)
                {
                    Console.WriteLine($"[REAL FAIL] {realCmd.Name}");
                    return Success = false;
                }
            }

            return Success = true;
        }

        // =========================
        // COMMAND CLONER (SAFE)
        // =========================
        private ICommand CloneCommand(ICommand cmd)
        {
            return cmd switch
            {
                MoveCommand => new MoveCommand(),
                LeftCommand => new LeftCommand(),
                RightCommand => new RightCommand(),
                ReportCommand => new ReportCommand(),

                PlaceCommand p =>
                    new PlaceCommand(p.X, p.Y, p.Direction, null),

                JumpForwardCommand j =>
                    new JumpForwardCommand(j.Steps),

                StepBackCommand => new StepBackCommand(),

                _ => cmd
            };
        }
    }
}