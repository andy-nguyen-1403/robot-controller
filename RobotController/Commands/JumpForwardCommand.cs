using RobotController.Commands;

namespace RobotController.Commands
{
    internal class JumpForwardCommand : ICommand
    {
        private readonly int steps;

        public JumpForwardCommand(int steps)
        {
            this.steps = steps < 1 ? 1 : steps;
        }

        public int Steps => steps;
        
        public string Name => "JUMP_FORWARD";
        public string Description => $"Jumps forward {steps} cells if possible.";

        public bool Executed { get; set; }
        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;

            if (!robot.CanMoveSteps(steps))
                return Success = false;

            switch (robot.Facing)
            {
                case Direction.North:
                    robot.CurrentPosition.Y += steps;
                    break;

                case Direction.East:
                    robot.CurrentPosition.X += steps;
                    break;

                case Direction.South:
                    robot.CurrentPosition.Y -= steps;
                    break;

                case Direction.West:
                    robot.CurrentPosition.X -= steps;
                    break;
            }


            return Success = true;
        }
    }
}