using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Kimi.Commands.Role;

[Group("role", "Role commands")]
public class RoleModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<RoleModule> _logger;

    public RoleModule(ILogger<RoleModule> logger)
    {
        _logger = logger;
    }

    [ComponentInteraction("selection-button:*")]
    public async Task HandleRoleButton(ulong roleId)
    {
        await DeferAsync(true);
        try
        {
            var top = Context.Guild.Roles.First(x => x.Id is 1343780800310542439);
            var bottom = Context.Guild.Roles.First(x => x.Id is 1343521480868368469);

            var roles = Context.Guild.Roles
                .Where(x => x.Id != roleId
                            && x.Position < (top?.Position ?? Context.Guild.Roles.Max(m => m.Position))
                            && x.Position > (bottom?.Position ?? 0))
                .OrderByDescending(x => x.Position).Select(x => x.Id).ToList();

            if (Context.User is not SocketGuildUser guildUser)
                throw new Exception("Invalid user");

            // await guildUser.RemoveRolesAsync(roles,
            //     new RequestOptions { AuditLogReason = "Member selected a new color" });
            await guildUser.ModifyAsync(x => x.RoleIds = guildUser.Roles.Select(r => r.Id)
                    .Except(roles)
                    .Concat([roleId])
                    .ToList(),
                new RequestOptions { AuditLogReason = "Member selected a new color" });
        }
        catch (Exception ex)
        {
            await FollowupAsync($"??? Aconteceu algo de errado, chame {
                (await Context.Client.Rest.GetCurrentBotInfoAsync()).Owner.Username
            }", ephemeral: true);
            _logger.LogError(ex, "Something went wrong handling the role buttons.");
        }

        // if (roleId is not 0)
        //     await guildUser.AddRoleAsync(roleId, new RequestOptions { AuditLogReason = "Member's new color" });

        //await FollowupAsync("Nova cor selecionada!", ephemeral: true);
    }
}