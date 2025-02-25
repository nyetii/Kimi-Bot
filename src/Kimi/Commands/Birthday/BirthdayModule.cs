using Discord.Interactions;
using Kimi.Configuration;
using Kimi.Extensions;
using Kimi.Repository.Repositories;
using Microsoft.Extensions.Options;

namespace Kimi.Commands.Birthday;

[Group("birthday", "Birthday!!!")]
public class BirthdayModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly KimiConfiguration _configuration;
    
    private readonly UserRepository _userRepository;

    public BirthdayModule(IOptions<KimiConfiguration> options, IServiceProvider provider)
    {
        _configuration = options.Value;
        
        var scope = provider.CreateScope();
        _userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
    }

    [SlashCommand("set", "Set your birthdate so you can earn a role")]
    public async Task SetBirthday([MinValue(1), MaxValue(31)]int day, Months month)
    {
        await DeferAsync(true);
        try
        {
            var date = new DateTime(DateTime.UtcNow.Year, (int)month, day);

            await _userRepository.UpdateBirthdateAsync(Context.User.Id, date);

            await FollowupAsync($"All set! You're going to get a special birthday role on {date:M}", ephemeral: true);
        }
        catch (ArgumentOutOfRangeException)
        {
            await FollowupAsync($"Not a valid date ({month} {day})");
        }
        catch (Exception ex) when (ex.Message is "Birthdate is already set.")
        {
            await FollowupAsync(ex.Message);
        }
        catch (Exception ex)
        {
            await Context.Client.SendToLogChannelAsync(_configuration.LogChannel, ex);
            throw;
        }
    }
    
    [SlashCommand("remove", "Remove your birthdate from the database")]
    public async Task RemoveBirthday()
    {
        await DeferAsync(true);
        
        await _userRepository.UpdateBirthdateAsync(Context.User.Id);

        await FollowupAsync($"Your birth date has been removed.", ephemeral: true);
    }

    public enum Months
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }
}