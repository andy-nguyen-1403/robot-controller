using System.Globalization;
//API storage
var executionLogs = new List<CommandExecutionLog>();
var rollbackSets = new List<CommandSet>();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();


// Seed data
var commandSets = new List<CommandSet>
{
    new CommandSet(
        "Rollback test",
        "2.0",
        "AllOrNothing",
        new List<RobotCommandRecord>
        {
            new ("PLACE", 1, 1, "NORTH"),
            new ("MOVE"),
            new ("MOVE"),
            new ("RIGHT"), 
            new ("REPORT")
        }
    )
    {
        Id = 1
    }
};

List<RobotCommand> commands = new List<RobotCommand>()
{
    new RobotCommand(1, "LEFT", false),
    new RobotCommand(2, "RIGHT", false),
    new RobotCommand(3, "MOVE", true),
    new RobotCommand(4, "PLACE", false)
};

RobotMap map = new RobotMap(5);

// GET /
app.MapGet("/", () => "Hello, Robot!");

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "robot-api"
}));

// GET all commands
app.MapGet("/robot-commands", () => Results.Ok(commands));

// GET move commands
app.MapGet("/robot-commands/move", () =>
{
    var result = commands.Where(c => c.IsMoveCommand).ToList();
    return Results.Ok(result);
});

// GET command by id
app.MapGet("/robot-commands/{id:int}", (int id) =>
{
    var command = commands.FirstOrDefault(c => c.Id == id);
    return command != null ? Results.Ok(command) : Results.NotFound();
});

// POST new command
app.MapPost("/robot-commands", (RobotCommand cmd) =>
{
    if (commands.Any(c => c.Id == cmd.Id))
        return Results.Conflict($"Command with Id {cmd.Id} already exists.");

    commands.Add(cmd);
    return Results.Created($"/robot-commands/{cmd.Id}", cmd);
});

// PUT update command
app.MapPut("/robot-commands/{id:int}", (int id, RobotCommand updated) =>
{
    var command = commands.FirstOrDefault(c => c.Id == id);
    if (command == null) return Results.NotFound();

    command.Name = updated.Name;
    command.IsMoveCommand = updated.IsMoveCommand;
    return Results.NoContent();
});

// GET map
app.MapGet("/robot-map", () => Results.Ok(map));

// CHECK coordinate
app.MapGet("/robot-map/{coordinate}", (string coordinate) =>
{
    var parts = coordinate.Split('-');
    if (parts.Length != 2) return Results.BadRequest("Coordinate must be x-y.");

    if (!int.TryParse(parts[0], out int x) || !int.TryParse(parts[1], out int y))
        return Results.BadRequest("Coordinate must be numeric.");

    bool inside = map.IsInside(x, y);
    return Results.Ok(inside);
});

// UPDATE map
app.MapPut("/robot-map", (RobotMap newMap) =>
{
    if (newMap.Size < 2 || newMap.Size > 100)
        return Results.BadRequest("Map size must be between 2 and 100.");

    map = newMap;
    return Results.NoContent();
});

// GET command-sets
app.MapGet("/command-sets/{id}", (int id) =>
{
    return commandSets.FirstOrDefault(x => x.Id == id);
});

// ===== EXECUTION LOGGING =====
// POST execution log
app.MapPost("/command-executions", (CommandExecutionLog log) =>
{
    executionLogs.Add(log);
    return Results.Created($"/command-executions/{log.WorkflowId}", log);
});

// GET execution log
app.MapGet("/command-executions/{workflowId:int}", (int workflowId) =>
{
    var log = executionLogs.FirstOrDefault(x => x.WorkflowId == workflowId);
    return log != null ? Results.Ok(log) : Results.NotFound();
});

// ===== ROLLBACK GENERATION =====
app.MapPost("/command-sets/{id:int}/rollback", (int id) =>
{
    var log = executionLogs.FirstOrDefault(x => x.WorkflowId == id);

    if (log == null)
        return Results.NotFound("Execution log not found");

    // 1. ONLY SUCCESSFUL COMMANDS
    var success = log.Commands
        .Where(c => c.Success)
        .ToList();

    // 2. REVERSE ORDER
    success.Reverse();

    var rollbackCommands = new List<RobotCommandRecord>();

    foreach (var cmd in success)
    {
        // 3. STOP AT PLACE
        if (cmd.Name == "PLACE")
            break;

        // 4. ANTIPODE MAPPING
        switch (cmd.Name)
        {
            case "LEFT":
                rollbackCommands.Add(new RobotCommandRecord("RIGHT"));
                break;

            case "RIGHT":
                rollbackCommands.Add(new RobotCommandRecord("LEFT"));
                break;

            case "MOVE":
                rollbackCommands.Add(new RobotCommandRecord("STEP_BACK"));
                break;
        }
    }

    var rollbackSet = new CommandSet(
        "Generated rollback",
        "2.0",
        "AllOrNothing",
        rollbackCommands
    );

    rollbackSets.Add(rollbackSet);

    return Results.Ok(rollbackSet);
});


// GET rollback
app.MapGet("/command-sets/{id:int}/rollback", (int id) =>
{
    var rollback = rollbackSets.FirstOrDefault();
    return rollback != null ? Results.Ok(rollback) : Results.NotFound();
});



// RUN APP
app.Run();

public partial class Program { }

// ===== CLASSES =====

public class RobotCommand
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsMoveCommand { get; set; }

    // Parameterless constructor needed for JSON deserialization
    public RobotCommand() { }

    public RobotCommand(int id, string name, bool isMove)
    {
        Id = id;
        Name = name;
        IsMoveCommand = isMove;
    }
}



public class RobotMap
{
    public int Size { get; set; }

    public RobotMap(int size)
    {
        Size = size;
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Size && y < Size;
    }
}

public record CommandSet
{
    public int Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? SchemaVersion { get; set; }
    public string? ExecutionMode { get; set; }
    public List<RobotCommandRecord> Commands { get; set; } = new();
    
    public CommandSet(string comment, string? schemaVersion, string? executionMode, List<RobotCommandRecord> commands)
    {
        Id = 0;
        Comment = comment;
        SchemaVersion = schemaVersion;
        ExecutionMode = executionMode;
        Commands = commands ?? new List<RobotCommandRecord>();
    }
}

public class RobotCommandRecord
{
    public string Name { get; set; } = string.Empty;
    public int? X { get; set; }
    public int? Y { get; set; }
    public string? Direction { get; set; }
    public int? NumberOfSteps { get; set; }

    public RobotCommandRecord(string name, int? x = null, int? y = null, string? direction = null, int? numberOfSteps = null)
    {
        Name = name;
        X = x;
        Y = y;
        Direction = direction;
        NumberOfSteps = numberOfSteps;
    }
}

// Executed Command Record
public record ExecutedCommandRecord
{
    public string Name { get; set; } = string.Empty;
    public bool Executed { get; set; }
    public bool Success { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public string? Direction { get; set; }
    public string? Comment { get; set; }
    public DateTime TimeStamp { get; set; }
    public string? RobotId { get; set; }

}

// Command Execution Log
public record CommandExecutionLog
{
    public int WorkflowId { get; set; }
    public string? SchemaVersion { get; set; }
    public List<ExecutedCommandRecord> Commands { get; set; } = new();

}
