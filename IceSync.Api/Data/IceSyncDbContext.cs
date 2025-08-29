using Microsoft.EntityFrameworkCore;
using IceSync.Api.Domain.Entities;

namespace IceSync.Api.Data
{
    public class IceSyncDbContext : DbContext, IIceSyncDbContext
    {
        public IceSyncDbContext(DbContextOptions<IceSyncDbContext> options) : base(options)
        {
        }

        public DbSet<Workflow> Workflows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.HasKey(e => e.WorkflowId);
                entity.Property(e => e.WorkflowId).ValueGeneratedNever(); // Allow explicit values
                entity.Property(e => e.WorkflowName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.MultiExecBehavior).HasMaxLength(100).IsRequired(false);
            });
        }
    }
}
