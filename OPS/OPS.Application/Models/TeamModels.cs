namespace OPS.Application.Models;

public class TeamListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int ActiveIncidentCount { get; set; }
    public int ManagerCount { get; set; }
    public int ResponderCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TeamDetailDto : TeamListDto
{
    public List<UserTeamDto> Members { get; set; } = new List<UserTeamDto>();
    public int ResolvedIncidentCount { get; set; }
    public int SlaBreachedIncidentCount { get; set; }
}

public class UserTeamDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}

public class CreateTeamRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateTeamRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
