using OPS.Application.DTOs.Attachments;

namespace OPS.Application.Interfaces;

public interface IAttachmentService
{
    Task<IEnumerable<AttachmentDto>> GetAttachmentsAsync(int incidentId, int userId);
    Task<AttachmentDto> UploadAttachmentAsync(int incidentId, int userId, Stream fileStream, string fileName, string contentType, long fileLength);
    Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadAttachmentAsync(int attachmentId, int userId);
    Task DeleteAttachmentAsync(int attachmentId, int userId);
}
