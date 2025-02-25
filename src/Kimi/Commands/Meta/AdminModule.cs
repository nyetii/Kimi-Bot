using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Kimi.Configuration;
using Kimi.Repository.Models;
using Kimi.Repository.Repositories;
using Microsoft.Extensions.Options;

namespace Kimi.Commands.Meta;

[RequireOwner]
[Group("admin", "Administration commands")]
public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
    [Group("profile", "Commands about Kimi's profile")]
    public class ProfileModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ProfileRepository _profileRepository;

        public ProfileModule(IServiceProvider provider)
        {
            var scope = provider.CreateScope();
            _profileRepository = scope.ServiceProvider.GetRequiredService<ProfileRepository>();
        }

        [SlashCommand("get", "Gets Kimi status")]
        public async Task GetStatus(int? id)
        {
            await DeferAsync();
            var profile = id is null
                ? await _profileRepository.GetAsync()
                : await _profileRepository.GetAsync(id.Value);

            if (profile is null)
            {
                await FollowupAsync("Could not find status.");
                return;
            }

            await Context.Client.SetStatusAsync(profile.StatusType);
            await Context.Client.SetGameAsync(profile.StatusMessage, profile.StatusUrl, profile.StatusActivityType);

            await FollowupAsync("Kimi's status has been reset.");
        }

        [SlashCommand("set", "Updates Kimi status")]
        public async Task UpdateStatus(string? message, string? url, ActivityType? activity, UserStatus? userStatus,
            bool @default = false, bool persist = true, bool ephemeral = true)
        {
            await DeferAsync(ephemeral);

            var profile = new Profile
            {
                Default = @default,
                StatusMessage = message ?? string.Empty,
                StatusUrl = url ?? string.Empty,
                StatusActivityType = activity ?? ActivityType.CustomStatus,
                StatusType = userStatus ?? UserStatus.Online
            };

            if (persist)
                await _profileRepository.InsertAsync(profile);

            await Context.Client.SetStatusAsync(profile.StatusType);
            await Context.Client.SetGameAsync(profile.StatusMessage, profile.StatusUrl, profile.StatusActivityType);

            await FollowupAsync("Kimi's status has been updated.", ephemeral: ephemeral);
        }

        [SlashCommand("reset", "Resets Kimi status")]
        public async Task ResetStatus(bool ephemeral = true)
        {
            await DeferAsync(ephemeral);
            var profile = await _profileRepository.GetAsync();

            if (profile is null)
            {
                await FollowupAsync("Could not find status.");
                return;
            }

            await Context.Client.SetStatusAsync(profile.StatusType);
            await Context.Client.SetGameAsync(profile.StatusMessage, profile.StatusUrl, profile.StatusActivityType);

            await FollowupAsync("Kimi's status has been reset.", ephemeral: ephemeral);
        }

        [SlashCommand("set-default", "Sets default status")]
        public async Task SetDefaultStatus(int id, bool ephemeral = true)
        {
            await DeferAsync(ephemeral);
            var profile = await _profileRepository.SetDefaultAsync(id);

            await Context.Client.SetStatusAsync(profile.StatusType);
            await Context.Client.SetGameAsync(profile.StatusMessage, profile.StatusUrl, profile.StatusActivityType);

            await FollowupAsync("Kimi's default status has been updated.", ephemeral: ephemeral);
        }

        [SlashCommand("list", "Lists all Kimi status")]
        public async Task ListStatus(bool ephemeral = true)
        {
            await DeferAsync(ephemeral);
            var profiles = await _profileRepository.GetAllAsync();

            await using var sw = new StreamWriter(new MemoryStream());
            await sw.WriteLineAsync("".PadRight(17)
                                    + "Status Type".PadRight(15)
                                    + "Activity Type".PadRight(16)
                                    + "URL".PadRight(profiles.Max(x => (x.StatusUrl ?? "no URL").Length) + 3)
                                    + "Message");

            foreach (var profile in profiles)
                await sw.WriteLineAsync($"ID: {profile.Id}{(profile.Default ? " [default]" : "")}".PadRight(16)
                                        + $" ({profile.StatusType})".PadRight(16)
                                        + $"({profile.StatusActivityType})".PadRight(16)
                                        + $"({profile.StatusUrl ?? "no URL"})"
                                            .PadRight(profiles.Max(x => (x.StatusUrl ?? "no URL").Length) + 3)
                                        + $"{profile.StatusMessage}".TrimEnd());

            await sw.FlushAsync();
            await FollowupWithFileAsync(sw.BaseStream, "status.txt", ephemeral: ephemeral);
        }
    }

    [Group("roles", "Commands about roles")]
    public class RoleModule : InteractionModuleBase<SocketInteractionContext>
    {
        // TODO: Rewrite this entire class and put it on a database
        private readonly KimiConfiguration _configuration;

        public RoleModule(IOptions<KimiConfiguration> configuration)
        {
            _configuration = configuration.Value;
        }

        [SlashCommand("set", "Sets a message of role buttons")]
        public async Task RoleButtons(IAttachment attachment, IChannel channel, string? messageId = null)
        {
            try
            {
                await DeferAsync(true);
                using var httpClient = new HttpClient();

                // var byteCode = await httpClient.GetByteArrayAsync(attachment.Url);
                // var encoded = Encoding.UTF8.GetString(byteCode);
                //
                // var json = JsonSerializer.Deserialize<dynamic>(encoded);

                var json = await httpClient.GetFromJsonAsync<Json>(attachment.Url);

                if (json is null)
                {
                    await FollowupAsync("Could not retrieve json.");
                    return;
                }

                if (channel is not SocketTextChannel socketChannel)
                {
                    await FollowupAsync("Could not find channel.");
                    return;
                }

                var componentBuilder = new ComponentBuilder();

                foreach (var row in json.Components)
                {
                    var rowBuilder = new ActionRowBuilder();
                    foreach (var button in row)
                    {
                        var roleExists = Context.Guild.Roles.Any(x => x.Id == button.RoleId || button.RoleId is 0);

                        var buttonBuilder = new ButtonBuilder();
                        buttonBuilder.WithLabel(" ");
                        buttonBuilder.WithStyle(ButtonStyle.Secondary);
                        buttonBuilder.WithCustomId($"admin.roles.button:{button.RoleId}");
                        buttonBuilder.WithEmote(Emote.Parse(button.Emote));
                        buttonBuilder.WithDisabled(!roleExists);
                        rowBuilder.WithButton(buttonBuilder);
                    }

                    componentBuilder.AddRow(rowBuilder);
                }

                var embedBuilder = new EmbedBuilder();
                embedBuilder.WithColor(243, 197, 199);
                embedBuilder.WithAuthor(json.Embed.AuthorName, json.Embed.AuthorIconUrl);
                embedBuilder.WithDescription(json.Embed.Description);
                embedBuilder.WithFooter(Context.Guild.Name, Context.Guild.IconUrl);
                embedBuilder.WithTimestamp(new DateTimeOffset(2017, 09, 05, 21, 34, 0, TimeSpan.FromHours(-3)));

                var message = await socketChannel.GetMessageAsync(ulong.Parse(messageId ?? "1"));

                if (message is SocketUserMessage socketMessage)
                {
                    await socketMessage.ModifyAsync(x =>
                    {
                        x.Embed = embedBuilder.Build();
                        x.Components = componentBuilder.Build();
                    });
                }
                else if (message is not null)
                {
                    await FollowupAsync("Could not retrieve message.", ephemeral: true);
                    return;
                }
                else
                {
                    message = await socketChannel.SendMessageAsync(embed: embedBuilder.Build(),
                        components: componentBuilder.Build());
                }

                await FollowupAsync($"See the message: {message.GetJumpUrl()}", ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Could not retrieve message. {ex}", ephemeral: true);
                throw;
            }
        }

        [ComponentInteraction("button:*")]
        public async Task HandleRoleButton(ulong roleId)
        {
            await DeferAsync(true);
            var top = Context.Guild.Roles.First(x => x.Id is 1343780800310542439);
            var bottom = Context.Guild.Roles.First(x => x.Id is 1343521480868368469);

            var roles = Context.Guild.Roles
                .Where(x => x.Id != roleId
                            && x.Position < (top?.Position ?? Context.Guild.Roles.Max(m => m.Position))
                            && x.Position > (bottom?.Position ?? 0))
                .OrderByDescending(x => x.Position).Select(x => x.Id).ToList();

            if (Context.User is not SocketGuildUser guildUser)
            {
                await FollowupAsync($"??? Aconteceu algo de errado, chame {
                    (await Context.Client.Rest.GetCurrentBotInfoAsync()).Owner.Username
                }");
                return;
            }

            // await guildUser.RemoveRolesAsync(roles,
            //     new RequestOptions { AuditLogReason = "Member selected a new color" });
            await guildUser.ModifyAsync(x => x.RoleIds = guildUser.Roles.Select(r => r.Id)
                    .Except(roles)
                    .Concat([roleId])
                    .ToList(),
                new RequestOptions { AuditLogReason = "Member selected a new color" });

            // if (roleId is not 0)
            //     await guildUser.AddRoleAsync(roleId, new RequestOptions { AuditLogReason = "Member's new color" });

            //await FollowupAsync("Nova cor selecionada!", ephemeral: true);
        }

        private class Json
        {
            [JsonPropertyName("message")] public required string Message { get; set; }
            [JsonPropertyName("embed")] public required JsonEmbed Embed { get; set; }
            [JsonPropertyName("components")] public required List<List<JsonComponent>> Components { get; set; }
        }

        private class JsonEmbed
        {
            [JsonPropertyName("authorName")] public required string AuthorName { get; set; }
            [JsonPropertyName("authorIconUrl")] public required string AuthorIconUrl { get; set; }
            [JsonPropertyName("description")] public required string Description { get; set; }
        }

        private class JsonComponent
        {
            [JsonPropertyName("roleId")] public required ulong RoleId { get; set; }
            [JsonPropertyName("emote")] public required string Emote { get; set; }
        }
    }

    [SlashCommand("talk", "Send a message as Kimi.")]
    public async Task Talk(string message, IChannel? channel = null, string reply = "0", bool ephemeral = true)
    {
        await DeferAsync(true);

        channel ??= Context.Channel;

        if (channel is not SocketTextChannel socketChannel)
        {
            await FollowupAsync($"Could not find channel. {Emote.Parse("<:SCred:1343474991185793035>")}");
            return;
        }

        var sent = await socketChannel.SendMessageAsync(message,
            messageReference: new MessageReference(ulong.Parse(reply), failIfNotExists: false));
        if (sent is null)
        {
            await FollowupAsync($"Could not send message. {Emote.Parse("<:SCred:1343474991185793035>")}");
            return;
        }

        await FollowupAsync(
            $"See the sent message here: {sent.GetJumpUrl()} {Emote.Parse("<:SCorange:1343474882729611265>")}",
            ephemeral: ephemeral);
    }
}