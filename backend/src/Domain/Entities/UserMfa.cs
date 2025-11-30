namespace Domain.Entities;

public class UserMfa
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = false;
    public DateTime? EnabledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
}