namespace Domain.Entities;

public class ExternalLogin
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public virtual User User { get; set; } = null!;
}