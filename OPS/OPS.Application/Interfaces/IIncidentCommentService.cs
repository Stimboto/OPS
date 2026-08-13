using OPS.Application.DTOs.Comments;

namespace OPS.Application.Interfaces;

public interface IIncidentCommentService
{
    Task<IEnumerable<CommentDto>> GetCommentsAsync(int incidentId, int userId);
    Task<CommentDto> CreateCommentAsync(int incidentId, int userId, CreateCommentDto dto);
    Task<CommentDto> UpdateCommentAsync(int incidentId, int commentId, int userId, UpdateCommentDto dto);
    Task DeleteCommentAsync(int incidentId, int commentId, int userId);
}
