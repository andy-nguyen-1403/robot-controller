using RobotController.Commands;
using RobotController.States;
using System.Collections.Generic;

namespace RobotController
{
    public class AdvancedRobot : IRobot
    {
        private readonly List<ICommand> commandHistory = new();

        public IReadOnlyList<ICommand> CommandHistory
        {
            get { return commandHistory.AsReadOnly(); }
        }

        public Map CurrentMap { get; set; }
        public Coordinate CurrentPosition { get; set; }
        public Direction Facing { get; set; }
        public IState CurrentState { get; set; }

        public AdvancedRobot(Map map)
        {
            CurrentMap = map;
            CurrentState = new IdleState(this);
        }

        public AdvancedRobot() { }

        // =========================
        // EXECUTION
        // =========================
        public void ExecuteCommand(ICommand command)
        {
            CurrentState.ExecuteCommand(command);
            commandHistory.Add(command);
        }

        // =========================
        // CLONE (CRITICAL)
        // =========================
        public static AdvancedRobot Clone(AdvancedRobot original)
        {
            var clone = new AdvancedRobot(original.CurrentMap);

            clone.Facing = original.Facing;

            if (original.CurrentPosition != null)
            {
                clone.CurrentPosition = new Coordinate(
                        original.CurrentPosition.X,
                        original.CurrentPosition.Y);
                
                clone.CurrentState = new ActiveState(clone);
            }
            
            else
            {
                clone.CurrentState = new IdleState(clone);
            }

            return clone;
        }

        // =========================
        // CAN MOVE (1 STEP)
        // =========================
        public bool CanMove
        {
            get
            {
                if (CurrentMap == null || CurrentPosition == null)
                    return false;

                return IsOnMap(NextX(1), NextY(1));
            }
        }

        // =========================
        // CAN MOVE (MULTI STEP)
        // =========================
        public bool CanMoveSteps(int steps)
        {
            if (steps < 1) steps = 1;
            if (CurrentMap == null || CurrentPosition == null)
                return false;

            return IsOnMap(NextX(steps), NextY(steps));
        }

        // =========================
        // STEP BACK
        // =========================
        public void StepBack()
        {
            if (CurrentPosition == null) return;

            int newX = CurrentPosition.X;
            int newY = CurrentPosition.Y;

            switch (Facing)
            {
                case Direction.North: newY--; break;
                case Direction.South: newY++; break;
                case Direction.East:  newX--; break;
                case Direction.West:  newX++; break;
            }

            if (IsOnMap(newX, newY))
            {
                CurrentPosition.X = newX;
                CurrentPosition.Y = newY;
            }
        }

        // =========================
        // HELPERS
        // =========================
        private bool IsOnMap(int x, int y)
        {
            if (CurrentMap == null) return false;

            return x >= 0 &&
                   y >= 0 &&
                   x < CurrentMap.Rows &&
                   y < CurrentMap.Columns;
        }

        private int NextX(int steps)
        {
            return Facing switch
            {
                Direction.East => CurrentPosition.X + steps,
                Direction.West => CurrentPosition.X - steps,
                _ => CurrentPosition.X
            };
        }

        private int NextY(int steps)
        {
            return Facing switch
            {
                Direction.North => CurrentPosition.Y + steps,
                Direction.South => CurrentPosition.Y - steps,
                _ => CurrentPosition.Y
            };
        }
    }
}