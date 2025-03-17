using System.IO.Compression;
using System.Reactive;
using System.Runtime.CompilerServices;
using Kimi.Jobs.Configuration;
using Kimi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Kimi.Jobs;

public class BackupJob : IJob
{
    public static readonly JobKey Key = new("backup");

    private readonly ILogger<BackupJob> _logger;

    private readonly JobConfiguration _configuration;
    private readonly string _databasePath;

    private readonly KimiDbContext _dbContext;

    public BackupJob(ILogger<BackupJob> logger, IOptions<JobConfiguration> options, IConfiguration configuration,
        KimiDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        _configuration = options.Value;

        _databasePath = configuration["ConnectionStrings:Database"]?.Split('=', ' ')
            .FirstOrDefault(x => x.EndsWith("Kimi.db")) ?? throw new Exception("Invalid database path.");
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.RefireCount > 5)
            return;

        try
        {
            _logger.LogInformation("Backup job started");
            _logger.LogInformation("Backing up Kimi.db to the following path: {path}", _configuration.BackupPath);

            var directory = new DirectoryInfo(_configuration.BackupPath);
            
            if (!directory.Exists)
                directory.Create();

            var file = directory.EnumerateFiles("Kimi-*_to_*.zip")
                .FirstOrDefault(x => x.CreationTime >= DateTime.Today.AddDays(-7));

            file ??= new FileInfo(
                $"{directory.FullName}/Kimi-{DateTime.Now:yyyy-MM-dd}_to_{DateTime.Now.AddDays(7):yyyy-MM-dd}.zip");

            await using var fileStream = file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);

            using var zip = new ZipArchive(fileStream, ZipArchiveMode.Update);

            var timestamp = DateTime.Now;
            var fileName = $"Kimi-{timestamp:yyyy-MM-dd}";
            var count = zip.Entries.Count(x => x.Name.Contains(fileName));

            fileName += count > 0
                ? $"_{count}.db"
                : ".db";

            var query = FormattableStringFactory.Create("VACUUM main INTO {0}",
                $"{_configuration.BackupPath}/_{fileName}");
            
            await _dbContext.Database.ExecuteSqlAsync(query);

            zip.CreateEntryFromFile($"{_configuration.BackupPath}/_{fileName}", fileName);

            directory.EnumerateFiles("_Kimi-*.db").FirstOrDefault()?.Delete();

            _logger.LogInformation("Successfully backed up Kimi.db to {file}", file.Name);
        }
        catch (Exception ex)
        {
            throw new JobExecutionException(refireImmediately: true, cause: ex);
        }
    }
}