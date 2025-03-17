using Kimi.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kimi.Repository.Repositories;

public class ProfileRepository
{
    private readonly ILogger<ProfileRepository> _logger;
    private readonly KimiDbContext _dbContext;

    public ProfileRepository(ILogger<ProfileRepository> logger, KimiDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Profile?> GetAsync()
    {
        return await _dbContext.Profiles.FirstOrDefaultAsync(x => x.Default);
    }
    
    public async Task<Profile?> GetAsync(int id)
    {
        return await _dbContext.Profiles.FindAsync(id);
    }

    public async Task<List<Profile>> GetAllAsync()
    {
        return await _dbContext.Profiles.ToListAsync();
    }

    public async Task InsertAsync(Profile profile)
    {
        _logger.LogInformation("Added a new profile: [{status}] ({activity}) {message} ({url})", profile.StatusType,
            profile.StatusActivityType, profile.StatusMessage, profile.StatusUrl ?? "no URL");
        
        if (profile.Default)
        {
            var oldDefaults = _dbContext.Profiles.Where(x => x.Default);
            
            foreach(var oldDefault in oldDefaults)
                oldDefault.Default = false;
            
            _dbContext.Profiles.UpdateRange(oldDefaults);
        }
        
        await _dbContext.Profiles.AddAsync(profile);
            
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Profile> SetDefaultAsync(int id)
    {
        var newDefault = await _dbContext.Profiles.FindAsync(id);

        if (newDefault is null)
            throw new Exception("Profile Status does not exist.");
        
        newDefault.Default = true;
        
        var oldDefaults = _dbContext.Profiles.Where(x => x.Default);

        foreach(var oldDefault in oldDefaults)
            oldDefault.Default = false;
            
        _dbContext.Profiles.UpdateRange(oldDefaults);
        _dbContext.Profiles.Update(newDefault);
        await _dbContext.SaveChangesAsync();
        
        return newDefault;
    }
}