using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UDL.Models;

namespace UDL.Data;

public partial class UdlDbContext : DbContext
{
    public UdlDbContext(DbContextOptions<UdlDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HistorialNivel> HistorialNivel { get; set; }

    public virtual DbSet<Jugador> Jugador { get; set; }

    public virtual DbSet<Nivel> Nivel { get; set; }

    public virtual DbSet<Record> Record { get; set; }

    public virtual DbSet<Submission> Submission { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HistorialNivel>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PK__Historia__9CC7DBB4DADFE71B");

            entity.Property(e => e.EstadoAredl).HasMaxLength(20);
            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())", "DF_HistorialNivel_Fecha");
            entity.Property(e => e.PuntosAredl).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdNivelNavigation).WithMany(p => p.HistorialNivel)
                .HasForeignKey(d => d.IdNivel)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HistorialNivel_Nivel");
        });

        modelBuilder.Entity<Jugador>(entity =>
        {
            entity.HasKey(e => e.IdJugador).HasName("PK__Jugador__99E320160B9B4028");

            entity.HasAlternateKey(e => e.NombreGd).HasName("UQ_Jugador_NombreGD");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Jugador_Activo");
            entity.Property(e => e.EsUruguayo).HasDefaultValue(true, "DF_Jugador_EsUruguayo");
            entity.Property(e => e.FechaAlta).HasDefaultValueSql("(sysdatetime())", "DF_Jugador_FechaAlta");
            entity.Property(e => e.NombreGd)
                .HasMaxLength(50)
                .HasColumnName("NombreGD");
        });

        modelBuilder.Entity<Nivel>(entity =>
        {
            entity.HasKey(e => e.IdNivel).HasName("PK__Nivel__A7F93DEC37D82E6F");

            entity.HasAlternateKey(e => e.AredlId).HasName("UQ_Nivel_AredlId");

            entity.Property(e => e.EdelEnjoyment).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EstadoAredl).HasMaxLength(20);
            entity.Property(e => e.GddlTier).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.NlwTier).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Publisher).HasMaxLength(100);
            entity.Property(e => e.PuntosAredl).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UltimaSincronizacion).HasDefaultValueSql("(sysdatetime())", "DF_Nivel_UltimaSincronizacion");
            entity.Property(e => e.Verifier).HasMaxLength(100);
        });

        modelBuilder.Entity<Record>(entity =>
        {
            entity.HasKey(e => e.IdRecord).HasName("PK__Record__356CCF9A06A9792F");

            entity.HasAlternateKey(e => new { e.IdJugador, e.IdNivel }).HasName("UQ_Record_JugadorNivel");

            entity.Property(e => e.Dispositivo).HasMaxLength(50);
            entity.Property(e => e.FechaAprobacion).HasDefaultValueSql("(sysdatetime())", "DF_Record_FechaAprobacion");
            entity.Property(e => e.Fps).HasColumnName("FPS");
            entity.Property(e => e.RawFootageUrl).HasMaxLength(500);
            entity.Property(e => e.VideoUrl).HasMaxLength(500);

            entity.HasOne(d => d.IdJugadorNavigation).WithMany(p => p.Record)
                .HasForeignKey(d => d.IdJugador)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Record_Jugador");

            entity.HasOne(d => d.IdNivelNavigation).WithMany(p => p.Record)
                .HasForeignKey(d => d.IdNivel)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Record_Nivel");

            entity.HasOne(d => d.IdSubmissionOrigenNavigation).WithMany(p => p.Record)
                .HasForeignKey(d => d.IdSubmissionOrigen)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Record_Submission");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.IdSubmission).HasName("PK__Submissi__DA0C699301490051");

            entity.Property(e => e.Dispositivo).HasMaxLength(50);
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente", "DF_Submission_Estado");
            entity.Property(e => e.FechaEnvio).HasDefaultValueSql("(sysdatetime())", "DF_Submission_FechaEnvio");
            entity.Property(e => e.Fps).HasColumnName("FPS");
            entity.Property(e => e.IdModerador).HasMaxLength(450);
            entity.Property(e => e.MotivoRechazo).HasMaxLength(500);
            entity.Property(e => e.Notas).HasMaxLength(500);
            entity.Property(e => e.RawFootageUrl).HasMaxLength(500);
            entity.Property(e => e.VideoUrl).HasMaxLength(500);

            entity.HasOne(d => d.IdJugadorNavigation).WithMany(p => p.Submission)
                .HasForeignKey(d => d.IdJugador)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Submission_Jugador");

            entity.HasOne(d => d.IdNivelNavigation).WithMany(p => p.Submission)
                .HasForeignKey(d => d.IdNivel)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Submission_Nivel");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
