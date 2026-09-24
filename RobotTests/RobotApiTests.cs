using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RobotTests;

public class RobotApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RobotApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ShouldReturnHelloRobot()
    {
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal("Hello, Robot!", content);
    }

    [Fact]
    public async Task GetRobotCommands_ShouldReturnCommands()
    {
        var response = await _client.GetAsync("/robot-commands");

        response.EnsureSuccessStatusCode();

        var commands =
            await response.Content.ReadFromJsonAsync<List<RobotCommand>>();

        Assert.NotNull(commands);
        Assert.NotEmpty(commands);
    }

    [Fact]
    public async Task GetMoveCommands_ShouldReturnOnlyMoveCommands()
    {
        var response = await _client.GetAsync("/robot-commands/move");

        response.EnsureSuccessStatusCode();

        var commands =
            await response.Content.ReadFromJsonAsync<List<RobotCommand>>();

        Assert.NotNull(commands);
        Assert.All(commands!, command =>
            Assert.True(command.IsMoveCommand));
    }

    [Fact]
    public async Task GetExistingCommand_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/robot-commands/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetNonExistingCommand_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/robot-commands/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMap_ShouldReturnDefaultFiveByFiveMap()
    {
        var response = await _client.GetAsync("/robot-map");

        response.EnsureSuccessStatusCode();

        var map =
            await response.Content.ReadFromJsonAsync<RobotMap>();

        Assert.NotNull(map);
        Assert.Equal(5, map!.Size);
    }

    [Fact]
    public async Task CheckCoordinateInsideMap_ShouldReturnTrue()
    {
        var response = await _client.GetAsync("/robot-map/2-2");

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<bool>();

        Assert.True(result);
    }

    [Fact]
    public async Task CheckInvalidCoordinate_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/robot-map/invalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CheckCoordinateOutsideMap_ShouldReturnFalse()
    {
        var response = await _client.GetAsync("/robot-map/10-10");

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<bool>();

        Assert.False(result);
    }

    [Fact]
    public async Task CreateDuplicateCommand_ShouldReturnConflict()
    {
        var command = new
        {
            id = 1,
            name = "LEFT",
            isMoveCommand = false
        };

        var response =
            await _client.PostAsJsonAsync("/robot-commands", command);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}

public class RobotCommand
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsMoveCommand { get; set; }
}

public class RobotMap
{
    public int Size { get; set; }
}
