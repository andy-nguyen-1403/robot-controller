using RobotController;
using RobotController.Commands;
using Xunit;

namespace RobotTests
{
    public class RobotTests
    {
        // ==========================================
        // BASIC ROBOT TESTS
        // ==========================================

        [Fact]
        public void Robot_WithoutMap_ShouldNotBeAbleToMove()
        {
            var robot = new Robot();

            robot.CurrentPosition = new Coordinate(1, 1);
            robot.Facing = Direction.North;

            Assert.False(robot.CanMove);
        }

        [Fact]
        public void Robot_CanMoveNorthInsideMap()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(1, 1);
            robot.Facing = Direction.North;

            Assert.True(robot.CanMove);
        }

        [Fact]
        public void Robot_CannotMoveOutsideMap()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(1, 4);
            robot.Facing = Direction.North;

            Assert.False(robot.CanMove);
        }

        [Fact]
        public void Robot_CanMoveMultipleStepsInsideMap()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(1, 1);
            robot.Facing = Direction.North;

            Assert.True(robot.CanMoveSteps(2));
        }

        [Fact]
        public void Robot_CannotMoveMultipleStepsOutsideMap()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(1, 3);
            robot.Facing = Direction.North;

            Assert.False(robot.CanMoveSteps(5));
        }

        // ==========================================
        // ROBOT CAN MOVE - ALL DIRECTIONS
        // ==========================================

        [Theory]
        [InlineData(Direction.North, 1, 1, true)]
        [InlineData(Direction.East, 1, 1, true)]
        [InlineData(Direction.South, 1, 1, true)]
        [InlineData(Direction.West, 1, 1, true)]
        public void Robot_CanMove_ShouldCheckAllDirections(
            Direction direction,
            int x,
            int y,
            bool expected)
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(x, y);
            robot.Facing = direction;

            Assert.Equal(expected, robot.CanMove);
        }

        [Fact]
        public void Robot_CanMove_ShouldReturnFalseAtNorthBoundary()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(1, 4);
            robot.Facing = Direction.North;

            Assert.False(robot.CanMove);
        }

        [Fact]
        public void Robot_CanMove_ShouldReturnFalseAtEastBoundary()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(4, 1);
            robot.Facing = Direction.East;

            Assert.False(robot.CanMove);
        }

        [Fact]
        public void Robot_CanMove_ShouldReturnFalseAtSouthBoundary()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(1, 0);
            robot.Facing = Direction.South;

            Assert.False(robot.CanMove);
        }

        [Fact]
        public void Robot_CanMove_ShouldReturnFalseAtWestBoundary()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(0, 1);
            robot.Facing = Direction.West;

            Assert.False(robot.CanMove);
        }

        [Fact]
        public void Robot_CanMove_ShouldReturnFalseWhenMapIsNull()
        {
            var robot = new Robot(null);

            robot.CurrentPosition = new Coordinate(1, 1);
            robot.Facing = Direction.North;

            Assert.False(robot.CanMove);
        }

        // ==========================================
        // ROBOT EXECUTE COMMAND
        // ==========================================

        [Fact]
        public void Robot_ExecuteCommand_ShouldStoreCommand()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(1, 1);
            robot.Facing = Direction.North;

            var command = new MoveCommand();

            robot.ExecuteCommand(command);

            Assert.NotNull(robot.CurrentState);
        }

        // ==========================================
        // ROBOT STEP BACK
        // ==========================================

        [Theory]
        [InlineData(Direction.North, 2, 2, 2, 1)]
        [InlineData(Direction.South, 2, 2, 2, 3)]
        [InlineData(Direction.East, 2, 2, 1, 2)]
        [InlineData(Direction.West, 2, 2, 3, 2)]
        public void Robot_StepBack_ShouldMoveBackward(
            Direction direction,
            int startX,
            int startY,
            int expectedX,
            int expectedY)
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(startX, startY);
            robot.Facing = direction;

            robot.StepBack();

            Assert.Equal(expectedX, robot.CurrentPosition.X);
            Assert.Equal(expectedY, robot.CurrentPosition.Y);
        }

        // ==========================================
        // ROBOT CAN MOVE STEPS
        // ==========================================

        [Theory]
        [InlineData(Direction.North, 1, 1, 2, true)]
        [InlineData(Direction.East, 1, 1, 2, true)]
        [InlineData(Direction.South, 1, 3, 2, true)]
        [InlineData(Direction.West, 3, 1, 2, true)]
        public void Robot_CanMoveSteps_ShouldCheckMultipleSteps(
            Direction direction,
            int x,
            int y,
            int steps,
            bool expected)
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(x, y);
            robot.Facing = direction;

            Assert.Equal(expected, robot.CanMoveSteps(steps));
        }

        [Theory]
        [InlineData(Direction.North)]
        [InlineData(Direction.East)]
        [InlineData(Direction.South)]
        [InlineData(Direction.West)]
        public void Robot_CanMoveSteps_ShouldReturnFalseWhenOutsideMap(
            Direction direction)
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(0, 0);
            robot.Facing = direction;

            Assert.False(robot.CanMoveSteps(10));
        }

        [Fact]
        public void Robot_CanMoveSteps_ShouldTreatZeroAsOneStep()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = new Coordinate(1, 1);
            robot.Facing = Direction.North;

            Assert.True(robot.CanMoveSteps(0));
        }

        [Fact]
        public void Robot_CanMoveSteps_ShouldReturnFalseWhenMapIsNull()
        {
            var robot = new Robot(null);

            robot.CurrentPosition = new Coordinate(1, 1);
            robot.Facing = Direction.North;

            Assert.False(robot.CanMoveSteps(1));
        }

        [Fact]
        public void Robot_CanMoveSteps_ShouldReturnFalseWhenPositionIsNull()
        {
            var robot = new Robot(new Map(5, 5));

            robot.CurrentPosition = null;
            robot.Facing = Direction.North;

            Assert.False(robot.CanMoveSteps(1));
        }

        // ==========================================
        // ROBOT CONSTRUCTORS
        // ==========================================

        [Fact]
        public void Robot_DefaultConstructor_ShouldCreateRobot()
        {
            var robot = new Robot();

            Assert.Null(robot.CurrentMap);
        }

        [Fact]
        public void Robot_MapConstructor_ShouldSetMap()
        {
            var map = new Map(5, 5);

            var robot = new Robot(map);

            Assert.Same(map, robot.CurrentMap);
            Assert.NotNull(robot.CurrentState);
        }
    }
}