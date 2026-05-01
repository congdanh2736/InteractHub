using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Reports;
using InteractHub.Api.Entities;
using InteractHub.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Services
{
    public class PostReportService : IPostReportService
    {
        private readonly IPostReportRepository _postReportRepository;
        private readonly IPostRepository _postRepository;

        public PostReportService(IPostReportRepository postReportRepository, IPostRepository postRepository)
        {
            _postReportRepository = postReportRepository;
            _postRepository = postRepository;
        }

        public async Task<object?> CreateReportAsync(CreateReportRequest request, string reporterId)
        {
            var postExists = await _postRepository.GetByIdAsync(request.PostId);
            if (postExists is null) return null;

            var existingReport = await _postReportRepository.GetPostReportAsync(request.PostId, reporterId);

            if (existingReport != null)
            {
                return new { message = "Bạn đã báo cáo bài viết này rồi. Quản trị viên đang xem xét!" };
            }

            var report = new PostReport
            {
                PostId = request.PostId,
                ReporterId = reporterId,
                Reason = request.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _postReportRepository.AddAsync(report);

            return new { 
                message = "Đã gửi báo cáo thành công. Cảm ơn bạn!" 
            };
        }

        public async Task<IEnumerable<object>> GetAllReportsAsync()
        {
            return await _postReportRepository.GetAllReportsAsync();
        }

        public async Task<object?> UpdateReportStatusAsync(int reportId, UpdateReportStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
                return null;

            var report = await _postReportRepository.GetPostReportByIdAsync(reportId);
            if (report == null) return null;

            report.Status = request.Status.Trim();

            await _postReportRepository.UpdateAsync(report);

            return new
            {
                message = "Cập nhật trạng thái báo cáo thành công.",
                reportId = report.Id,
                status = report.Status
            };
        }
    }
}