using TaskLists.Application.Models;
using TaskLists.Domain.Entities;

namespace TaskLists.Application.Extensions;

public static class UserExtensions
{
    public static UserInfo ToUserInfo(this User user)
    {
        return new UserInfo(
            user.Id,
            user.DisplayName,
            user.CreatedAt);
    }
}
