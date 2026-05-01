using InteractHub.Api.Data;
using InteractHub.Api.Dtos.Users;
using InteractHub.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InteractHub.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> FindUserAsync(string id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<UserDtos>> SearchUsersAsync(string searchTerm)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.DisplayName.ToLower().Contains(searchTerm))
                .Take(10)
                .Select(u => new UserDtos
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    Email = u.Email!
                })
                .ToListAsync();
        }
    }
}
