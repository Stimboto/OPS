namespace OPS.Application.DTOs.Comments;

public class CommentDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCommentDto
{
    public string CommentText { get; set; } = string.Empty;
}

public class UpdateCommentDto
{
    public string CommentText { get; set; } = string.Empty;
}
