using System;
using RobotController;
using RobotController.Commands;
using Xunit;

namespace RobotTests
{
    public class CommandTests
    {
        private Robot CreateRobot(
            int x = 1,
            int y = 1,
            Direction direction = Direction.North)
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(x, y);
            robot.Facing = direction;

            return robot;
        }

        // =========================
        // MOVE COMMAND
        // =========================

        [Fact]
        public void MoveCommand_ShouldMoveNorth()
        {
            var robot = CreateRobot(1, 1, Direction.North);
            var command = new MoveCommand();

            var result = command.Execute(robot);

            Assert.True(result);
            Assert.True(command.Executed);
            Assert.True(command.Success);
            Assert.Equal(1, robot.CurrentPosition.X);
            Assert.Equal(2, robot.CurrentPosition.Y);
            Assert.Equal("MOVE", command.Name);
            Assert.Contains("Moves robot", command.Description);
            Assert.Equal("MOVE", command.ToString());
        }

        [Fact]
        public void MoveCommand_ShouldMoveEast()
        {
            var robot = CreateRobot(1, 1, Direction.East);
            var command = new MoveCommand();

            Assert.True(command.Execute(robot));

            Assert.Equal(2, robot.CurrentPosition.X);
            Assert.Equal(1, robot.CurrentPosition.Y);
        }

        [Fact]
        public void MoveCommand_ShouldMoveSouth()
        {
            var robot = CreateRobot(1, 1, Direction.South);
            var command = new MoveCommand();

            Assert.True(command.Execute(robot));

            Assert.Equal(1, robot.CurrentPosition.X);
            Assert.Equal(0, robot.CurrentPosition.Y);
        }

        [Fact]
        public void MoveCommand_ShouldMoveWest()
        {
            var robot = CreateRobot(1, 1, Direction.West);
            var command = new MoveCommand();

            Assert.True(command.Execute(robot));

            Assert.Equal(0, robot.CurrentPosition.X);
            Assert.Equal(1, robot.CurrentPosition.Y);
        }

        [Fact]
        public void MoveCommand_ShouldFailWhenRobotCannotMove()
        {
            var robot = CreateRobot(0, 4, Direction.North);
            var command = new MoveCommand();

            var result = command.Execute(robot);

            Assert.False(result);
            Assert.True(command.Executed);
            Assert.False(command.Success);
            Assert.Equal(0, robot.CurrentPosition.X);
            Assert.Equal(4, robot.CurrentPosition.Y);
        }

        // =========================
        // LEFT COMMAND
        // =========================

        [Theory]
        [InlineData(Direction.North, Direction.West)]
        [InlineData(Direction.West, Direction.South)]
        [InlineData(Direction.South, Direction.East)]
        [InlineData(Direction.East, Direction.North)]
        public void LeftCommand_ShouldRotateCorrectly(
            Direction initial,
            Direction expected)
        {
            var robot = CreateRobot(1, 1, initial);
            var command = new LeftCommand();

            var result = command.Execute(robot);

            Assert.True(result);
            Assert.True(command.Executed);
            Assert.True(command.Success);
            Assert.Equal(expected, robot.Facing);
            Assert.Equal("LEFT", command.Name);
            Assert.Equal("LEFT", command.ToString());
        }

        // =========================
        // RIGHT COMMAND
        // =========================

        [Theory]
        [InlineData(Direction.North, Direction.East)]
        [InlineData(Direction.East, Direction.South)]
        [InlineData(Direction.South, Direction.West)]
        [InlineData(Direction.West, Direction.North)]
        public void RightCommand_ShouldRotateCorrectly(
            Direction initial,
            Direction expected)
        {
            var robot = CreateRobot(1, 1, initial);
            var command = new RightCommand();

            var result = command.Execute(robot);

            Assert.True(result);
            Assert.True(command.Executed);
            Assert.True(command.Success);
            Assert.Equal(expected, robot.Facing);
            Assert.Equal("RIGHT", command.Name);
            Assert.Equal("RIGHT", command.ToString());
        }

        // =========================
        // PLACE COMMAND
        // =========================

        [Fact]
        public void PlaceCommand_ShouldPlaceRobotOnMap()
        {
            var robot = CreateRobot(0, 0, Direction.North);
            var command = new PlaceCommand(
                3,
                4,
                Direction.West);

            var result = command.Execute(robot);

            Assert.True(result);
            Assert.True(command.Executed);
            Assert.True(command.Success);

            Assert.Equal(3, robot.CurrentPosition.X);
            Assert.Equal(4, robot.CurrentPosition.Y);
            Assert.Equal(Direction.West, robot.Facing);

            Assert.Equal(3, command.X);
            Assert.Equal(4, command.Y);
            Assert.Equal(Direction.West, command.Direction);
            Assert.Equal("PLACE", command.Name);
        }

        [Fact]
        public void PlaceCommand_ShouldFailOutsideMap()
        {
            var robot = CreateRobot();
            var command = new PlaceCommand(
                10,
                10,
                Direction.North);

            var result = command.Execute(robot);

            Assert.False(result);
            Assert.True(command.Executed);
            Assert.False(command.Success);
        }

        [Fact]
        public void PlaceCommand_ShouldFailWhenMapIsNull()
        {
            var robot = new Robot(null);
            var command = new PlaceCommand(
                1,
                1,
                Direction.North);

            var result = command.Execute(robot);

            Assert.False(result);
            Assert.True(command.Executed);
            Assert.False(command.Success);
        }

        [Fact]
        public void PlaceCommand_ToString_ShouldReturnNameWithoutInput()
        {
            var command = new PlaceCommand(
                1,
                2,
                Direction.East);

            Assert.Equal("PLACE", command.ToString());
        }

        [Fact]
        public void PlaceCommand_ToString_ShouldReturnOriginalInput()
        {
            var command = new PlaceCommand(
                1,
                2,
                Direction.East,
                "PLACE 1,2,EAST");

            Assert.Equal("PLACE 1,2,EAST", command.ToString());
        }

        // =========================
        // JUMP FORWARD
        // =========================

        [Theory]
        [InlineData(Direction.North, 2, 1, 3)]
        [InlineData(Direction.East, 2, 3, 1)]
        [InlineData(Direction.South, 1, 1, 0)]
        [InlineData(Direction.West, 1, 0, 1)]
        public void JumpForwardCommand_ShouldMoveCorrectly(
            Direction direction,
            int steps,
            int expectedX,
            int expectedY)
        {
            var robot = CreateRobot(1, 1, direction);
            var command = new JumpForwardCommand(steps);

            var result = command.Execute(robot);

            Assert.True(result);
            Assert.True(command.Executed);
            Assert.True(command.Success);
            Assert.Equal(expectedX, robot.CurrentPosition.X);
            Assert.Equal(expectedY, robot.CurrentPosition.Y);
            Assert.Equal(steps, command.Steps);
            Assert.Equal("JUMP_FORWARD", command.Name);
        }

        [Fact]
        public void JumpForwardCommand_ShouldDefaultStepsToOne()
        {
            var robot = CreateRobot(1, 1, Direction.North);
            var command = new JumpForwardCommand(0);

            Assert.Equal(1, command.Steps);

            Assert.True(command.Execute(robot));

            Assert.Equal(2, robot.CurrentPosition.Y);
        }

        [Fact]
        public void JumpForwardCommand_ShouldFailWhenTooFar()
        {
            var robot = CreateRobot(1, 3, Direction.North);
            var command = new JumpForwardCommand(5);

            var result = command.Execute(robot);

            Assert.False(result);
            Assert.True(command.Executed);
            Assert.False(command.Success);

            Assert.Equal(1, robot.CurrentPosition.X);
            Assert.Equal(3, robot.CurrentPosition.Y);
        }

        // =========================
        // STEP BACK COMMAND
        // =========================

        [Fact]
        public void StepBackCommand_ShouldMoveRobotBackward()
        {
            var robot = CreateRobot(2, 2, Direction.North);
            var command = new StepBackCommand();

            var result = command.Execute(robot);

            Assert.True(result);
            Assert.True(command.Executed);
            Assert.True(command.Success);

            Assert.Equal(2, robot.CurrentPosition.X);
            Assert.Equal(1, robot.CurrentPosition.Y);
            Assert.Equal("STEP_BACK", command.Name);
        }

        [Fact]
        public void StepBackCommand_ShouldFailForNonRobotImplementation()
        {
            var advancedRobot = new AdvancedRobot(new Map(5, 5));
            advancedRobot.CurrentPosition = new Coordinate(2, 2);
            advancedRobot.Facing = Direction.North;

            var command = new StepBackCommand();

            var result = command.Execute(advancedRobot);

            Assert.False(result);
            Assert.True(command.Executed);
            Assert.False(command.Success);
        }

        // =========================
        // REPORT COMMAND
        // =========================

        [Fact]
        public void ReportCommand_ShouldExecuteSuccessfully()
        {
            var robot = CreateRobot(2, 3, Direction.North);
            var command = new ReportCommand();

            var originalOut = Console.Out;

            try
            {
                using var writer = new System.IO.StringWriter();
                Console.SetOut(writer);

                var result = command.Execute(robot);

                Assert.True(result);
                Assert.True(command.Executed);
                Assert.True(command.Success);

                Assert.Contains("2,3,NORTH", writer.ToString());
                Assert.Equal("REPORT", command.Name);
                Assert.Equal("REPORT", command.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        // =========================
        // UNKNOWN COMMAND
        // =========================

        [Fact]
        public void UnknownCommand_ShouldAlwaysFail()
        {
            var robot = CreateRobot();
            var command = new UnknownCommand("HELLO");

            var result = command.Execute(robot);

            Assert.False(result);
            Assert.True(command.Executed);
            Assert.False(command.Success);

            Assert.Equal("HELLO", command.Input);
            Assert.Equal("UNKNOWN", command.Name);
            Assert.Equal("Represents unrecognised input.", command.Description);
            Assert.Equal("HELLO", command.ToString());
        }
    }
}
