namespace Kimi.Jobs.Configuration;

public class JobConfiguration
{
    public required string BackupPath { get; set; } = $"{AppContext.BaseDirectory}/backup";
}