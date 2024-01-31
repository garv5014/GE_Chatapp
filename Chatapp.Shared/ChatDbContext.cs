using Chatapp.Shared.Entities;

using Microsoft.EntityFrameworkCore;

namespace Chatapp.Shared;

public partial class ChatDbContext : DbContext
{
  public ChatDbContext(DbContextOptions<ChatDbContext> options)
      : base(options)
  {
  }

  public virtual DbSet<Message> Messages { get; set; }

  public virtual DbSet<Picture> Pictures { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Message>(entity =>
    {
      entity.HasKey(e => e.Id).HasName("message_pkey");

      entity.ToTable("message");

      entity.Property(e => e.Id).HasColumnName("id");
      entity.Property(e => e.CreatedAt)
              .HasDefaultValueSql("now()")
              .HasColumnType("timestamp without time zone")
              .HasColumnName("created_at");
      entity.Property(e => e.MessageText).HasColumnName("message_text");
      entity.Property(e => e.Username).HasColumnName("username");
    });

    modelBuilder.Entity<Picture>(entity =>
    {
      entity.HasKey(e => e.Id).HasName("picture_pkey");

      entity.ToTable("picture");

      entity.Property(e => e.Id).HasColumnName("id");
      entity.Property(e => e.BelongsTo).HasColumnName("belongs_to");
      entity.Property(e => e.NameOfFile).HasColumnName("name_of_file");

      entity.HasOne(d => d.BelongsToNavigation).WithMany(p => p.Pictures)
              .HasForeignKey(d => d.BelongsTo)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("picture_belongs_to_fkey");
    });

    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}