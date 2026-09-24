using RobotController.Commands;

namespace RobotController.Commands
{
    internal class UnknownCommand : ICommand
    {
        public UnknownCommand(string input)
        {
            Input = input;
        }
        public string Input { get; }
        public string Name => "UNKNOWN";
        public string Description => "Represents unrecognised input.";
        public bool Executed { get; set; }
        public bool Success { get; set; }
        public bool Execute(IRobot robot)
        {
            Executed = true;
            return Success = false;
        }
        public override string ToString()
        {
            return Input;
        }
    }
}