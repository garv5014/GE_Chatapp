using System;
using System.Collections.Generic;

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

  public virtual DbSet<PictureLookup> PictureLookups { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Message>(entity =>
    {
      entity.HasKey(e => e.Id).HasName("message_pkey");

      entity.ToTable("message");

      entity.Property(e => e.Id).HasColumnName("id");
      entity.Property(e => e.Clientid).HasColumnName("clientid");
      entity.Property(e => e.CreatedAt)
              .HasDefaultValueSql("now()")
              .HasColumnType("timestamp without time zone")
              .HasColumnName("created_at");
      entity.Property(e => e.EventCount).HasColumnName("event_count");
      entity.Property(e => e.MessageText).HasColumnName("message_text");
      entity.Property(e => e.Username).HasColumnName("username");
      entity.Property(e => e.VectorDict)
              .HasColumnType("jsonb")
              .HasColumnName("vector_dict");
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

    modelBuilder.Entity<PictureLookup>(entity =>
    {
      entity.HasKey(e => e.Id).HasName("picture_lookup_pkey");

      entity.ToTable("picture_lookup");

      entity.Property(e => e.Id).HasColumnName("id");
      entity.Property(e => e.MachineName).HasColumnName("machine_name");
      entity.Property(e => e.PictureId).HasColumnName("picture_id");

      entity.HasOne(d => d.Picture).WithMany(p => p.PictureLookups)
              .HasForeignKey(d => d.PictureId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("picture_lookup_picture_id_fkey");
    });

    OnModelCreatingPartial(modelBuilder);
  }

  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}