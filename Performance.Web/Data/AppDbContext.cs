using Microsoft.EntityFrameworkCore;
using Performance.Web.Models;

namespace Performance.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Employee ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Id is provided by the application (or set by the DB as a default)
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.FirstName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.LastName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.PersonnelCode)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(e => e.PersonnelCode).IsUnique();

            // Manager1Id–Manager4Id are plain nullable Guid columns.
            // No FK constraints are defined to avoid self-referential cycle issues
            // and to simplify the schema. LINQ queries compare the Guid values directly.
        });

        // ── Evaluation ────────────────────────────────────────────────────────
        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.FeedbackText)
                  .IsRequired()
                  .HasMaxLength(4000);

            // Use Restrict on both FKs to avoid multiple cascade paths
            entity.HasOne(e => e.Evaluator)
                  .WithMany()
                  .HasForeignKey(e => e.EvaluatorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TargetEmployee)
                  .WithMany()
                  .HasForeignKey(e => e.TargetEmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
