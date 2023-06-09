using GigaChat.Server.Authentication.Constants;

using Microsoft.AspNetCore.SignalR;

namespace GigaChat.Server.SignalR.Services;

public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.Claims.FirstOrDefault(x => x.Type.Equals(UserClaimTypes.Id, StringComparison.Ordinal))?.Value;
    }
}