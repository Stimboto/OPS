namespace OPS.Domain.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    public ICollection<UserTeam> UserTeams { get; set; } = new List<UserTeam>();
}
