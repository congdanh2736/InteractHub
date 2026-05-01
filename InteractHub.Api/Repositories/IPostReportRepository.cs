using InteractHub.Api.Entities;

namespace InteractHub.Api.Repositories
{
    public interface IPostReportRepository
    {
        Task<PostReport?> GetPostReportAsync(int postId, string reporterId);
        Task<PostReport?> GetPostReportByIdAsync(int reportId);
        Task<IEnumerable<object>> GetAllReportsAsync();
        Task<PostReport> AddAsync(PostReport r);
        Task<PostReport> UpdateAsync(PostReport r);
    }
}
