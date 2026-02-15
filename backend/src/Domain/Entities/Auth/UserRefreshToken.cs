namespace Domain.Entities.Auth
{
    public class UserRefreshToken : Entity
    {
        public Guid UserId { get; set; }
        public Guid JwtId { get; set; }
        public Guid Token { get; set; }
        public bool Used { get; set; }
        public bool Revoked { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
