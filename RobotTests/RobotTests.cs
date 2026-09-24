using RobotController;
using Xunit;

namespace RobotTests;

public class RobotTests
{
    [Fact]
    public void Robot_ShouldNotMove_WhenThereIsNoMap()
    {
        var robot = new Robot();

        Assert.False(robot.CanMove);
    }

    [Fact]
    public void Robot_ShouldMoveNorth_WhenPositionIsInsideMap()
    {
        var map = new Map(5, 5);
        var robot = new Robot(map)
        {
            CurrentPosition = new Coordinate(2, 2),
            Facing = Direction.North
        };

        Assert.True(robot.CanMove);
    }

    [Fact]
    public void Robot_ShouldNotMove_WhenFacingMapBoundary()
    {
        var map = new Map(5, 5);
        var robot = new Robot(map)
        {
            CurrentPosition = new Coordinate(2, 4),
            Facing = Direction.North
        };

        Assert.False(robot.CanMove);
    }

    [Fact]
    public void Robot_CanMoveMultipleSteps_WhenInsideMap()
    {
        var map = new Map(5, 5);
        var robot = new Robot(map)
        {
            CurrentPosition = new Coordinate(1, 1),
            Facing = Direction.North
        };

        Assert.True(robot.CanMoveSteps(2));
    }

    [Fact]
    public void Robot_CannotMoveMultipleSteps_OutsideMap()
    {
        var map = new Map(5, 5);
        var robot = new Robot(map)
        {
            CurrentPosition = new Coordinate(1, 1),
            Facing = Direction.North
        };

        Assert.False(robot.CanMoveSteps(5));
    }

    [Fact]
    public void AdvancedRobot_ShouldStartWithEmptyCommandHistory()
    {
        var map = new Map(5, 5);
        var robot = new AdvancedRobot(map);

        Assert.Empty(robot.CommandHistory);
    }

    [Fact]
    public void AdvancedRobot_Clone_ShouldCopyPositionAndDirection()
    {
        var map = new Map(5, 5);

        var robot = new AdvancedRobot(map)
        {
            CurrentPosition = new Coordinate(2, 3),
            Facing = Direction.East
        };

        var clone = AdvancedRobot.Clone(robot);

        Assert.NotNull(clone.CurrentPosition);
        Assert.Equal(2, clone.CurrentPosition.X);
        Assert.Equal(3, clone.CurrentPosition.Y);
        Assert.Equal(Direction.East, clone.Facing);
    }
}
