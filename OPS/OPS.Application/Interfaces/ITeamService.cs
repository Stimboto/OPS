using OPS.Application.Models;

namespace OPS.Application.Interfaces;

public interface ITeamService
{
    Task<IEnumerable<TeamListDto>> GetTeamsAsync();
    Task<TeamDetailDto> GetTeamAsync(int id);
    Task<TeamDetailDto> CreateTeamAsync(CreateTeamRequest request, int currentUserId);
    Task<TeamDetailDto> UpdateTeamAsync(int id, UpdateTeamRequest request, int currentUserId);
    Task DeleteTeamAsync(int id, int currentUserId);

    Task<IEnumerable<UserTeamDto>> GetTeamMembersAsync(int teamId);
    Task AddMemberToTeamAsync(int teamId, int userId, int currentUserId);
    Task RemoveMemberFromTeamAsync(int teamId, int userId, int currentUserId);
}
