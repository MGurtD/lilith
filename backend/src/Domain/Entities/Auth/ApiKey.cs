namespace Domain.Entities.Auth
{
    public class ApiKey : Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string KeyPrefix { get; set; } = string.Empty;
        public string KeyHash { get; set; } = string.Empty;
        public string Scopes { get; set; } = string.Empty;
        public DateTime? ExpiresOn { get; set; }
        public DateTime? LastUsedOn { get; set; }
    }
}
