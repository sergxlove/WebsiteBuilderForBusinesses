using WebsiteBuilderForBusinesses.Core.Infrastructures;

namespace WebsiteBuilderForBusinesses.Core.Models
{
    public class TokensUser
    {
        public Guid Id { get; }
        public Guid UserId { get; }
        public DateTime Created { get; }
        public DateTime Ended { get; }
        public string Email { get; } = string.Empty;
        public string Role { get; } = string.Empty;

        public static ResultModel<TokensUser> Create(Guid id, Guid userId, DateTime created,
            DateTime ended, string email, string role)
        {
            if (id == Guid.Empty)
                return ResultModel<TokensUser>.Failure("id is null");
            if (userId == Guid.Empty)
                return ResultModel<TokensUser>.Failure("id user is null");
            if (email == string.Empty)
                return ResultModel<TokensUser>.Failure("email is null");
            if (role == string.Empty)
                return ResultModel<TokensUser>.Failure("role is  null");
            return ResultModel<TokensUser>.Success(new TokensUser(id, userId, created, ended, email, role));
        }

        private TokensUser(Guid id, Guid userId, DateTime created,
            DateTime ended, string email, string role)
        {
            Id = id;
            UserId = userId;
            Created = created;
            Ended = ended;
            Email = email;
            Role = role;
        }
    }
}
