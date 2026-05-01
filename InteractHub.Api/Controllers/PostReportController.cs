using InteractHub.Api.Dtos.Reports;
using InteractHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InteractHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Mặc định: phải đăng nhập
    public class PostReportsController : ControllerBase
    {
        private readonly IPostReportService _reportService;

        public PostReportsController(IPostReportService reportService)
        {
            _reportService = reportService;
        }

        // User đã đăng nhập có thể tạo report
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _reportService.CreateReportAsync(request, userId);
            if (result == null) return NotFound("Không tìm thấy bài viết này.");

            return Ok(result);
        }

        // Chỉ Admin xem được tất cả report
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _reportService.GetAllReportsAsync();
            return Ok(reports);
        }

        // Chỉ Admin cập nhật trạng thái report (nếu service của bạn đã có hàm này)
        [HttpPut("{reportId:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateReportStatus([FromRoute] int reportId, [FromBody] UpdateReportStatusRequest request)
        {
            var result = await _reportService.UpdateReportStatusAsync(reportId, request);
            if (result == null) return NotFound("Không tìm thấy báo cáo.");

            return Ok(result);
        }
    }
}