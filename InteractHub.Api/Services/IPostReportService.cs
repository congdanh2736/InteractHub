using InteractHub.Api.Dtos.Reports;

namespace InteractHub.Api.Services
{
    public interface IPostReportService
    {
        Task<object?> CreateReportAsync(CreateReportRequest request, string reporterId);

        // Admin xem toàn bộ report
        Task<IEnumerable<object>> GetAllReportsAsync();

        // Admin cập nhật trạng thái report
        Task<object?> UpdateReportStatusAsync(int reportId, UpdateReportStatusRequest request);
    }
}
