using System;
using System.Collections.Generic;

#nullable enable
namespace RobotController.DTOs
{
    public record ExecutedCommandRecord
    {
        public string? Name { get; set; }
        public bool Executed { get; set; }
        public bool Success { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public string? Direction { get; set; }
        public string? Comment { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? RobotId { get; set; }
    }

    public record CommandExecutionLog
    {
        public int WorkflowId { get; set; }
        public string? SchemaVersion { get; set; }
        public List<ExecutedCommandRecord> Commands { get; set; } = new();
    }
}