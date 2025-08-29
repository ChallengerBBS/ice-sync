using Microsoft.EntityFrameworkCore;
using IceSync.Api.Domain.Entities;

namespace IceSync.Api.Data
{
    public interface IIceSyncDbContext
    {
        DbSet<Workflow> Workflows { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
