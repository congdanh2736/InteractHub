using InteractHub.Api.Data;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Repositories
{
    public class PostReportRepository : IPostReportRepository
    {
        private readonly ApplicationDbContext _context;

        public PostReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PostReport?> GetPostReportAsync(int postId, string reporterId)
        {
            return await _context.PostReports.FirstOrDefaultAsync(p => p.PostId == postId && p.ReporterId == reporterId);
        }

        public async Task<PostReport?> GetPostReportByIdAsync(int reportId)
        {
            return await _context.PostReports.FirstOrDefaultAsync(r => r.Id == reportId);
        }

        public async Task<IEnumerable<object>> GetAllReportsAsync()
        {
            return await _context.PostReports
                .AsNoTracking()
                .Include(r => r.Post)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.PostId,
                    PostContent = r.Post!.Content,
                    r.ReporterId,
                    ReporterDisplayName = r.Reporter!.DisplayName,
                    r.Reason,
                    r.Status,
                    r.CreatedAt
                })
                .ToListAsync();

            //return reports.Cast<PostReport>();
        }

        public async Task<PostReport> AddAsync(PostReport r)
        {
            await _context.PostReports.AddAsync(r);
            await _context.SaveChangesAsync();
            return r;
        }
        
        public async Task<PostReport> UpdateAsync(PostReport r)
        {
            _context.PostReports.Update(r);
            await _context.SaveChangesAsync();
            return r;
        }
    }
}
