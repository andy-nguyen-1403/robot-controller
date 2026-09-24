using RobotController.Commands;

namespace RobotController.Commands
{
    internal class StepBackCommand : ICommand
    {
        public string Name => "STEP_BACK";
        public string Description => "Moves the robot one step backward.";

        public bool Executed { get; set; }
        public bool Success { get; set; }

        public bool Execute(IRobot robot)
        {
            Executed = true;

            if (robot is Robot concreteRobot)
            {
                concreteRobot.StepBack();
                return Success = true;
            }

            return Success = false;
        }
    }
}