using Microsoft.EntityFrameworkCore;
using UDL.Models;

namespace UDL.Data;

public partial class UdlDbContext
{
    // CHECK existentes en SQL Server que el scaffolding no genera.
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Jugador>().ToTable("Jugador", "dbo");
        modelBuilder.Entity<Nivel>().Property(e => e.RequiresRawFootage)
            .HasDefaultValue(false, "DF_Nivel_RequiresRawFootage");
        modelBuilder.Entity<Nivel>().Property(e => e.TwoPlayer)
            .HasDefaultValue(false, "DF_Nivel_TwoPlayer");
        modelBuilder.Entity<Nivel>().ToTable("Nivel", "dbo", table =>
        {
            table.HasCheckConstraint("CK_Nivel_PosicionAredl", "([PosicionAredl] IS NULL OR [PosicionAredl]>(0))");
            table.HasCheckConstraint("CK_Nivel_PuntosAredl", "([PuntosAredl]>=(0))");
        });
        modelBuilder.Entity<HistorialNivel>().ToTable("HistorialNivel", "dbo", table =>
        {
            table.HasCheckConstraint("CK_HistorialNivel_Posicion", "([PosicionAredl] IS NULL OR [PosicionAredl]>(0))");
            table.HasCheckConstraint("CK_HistorialNivel_Puntos", "([PuntosAredl]>=(0))");
        });
        modelBuilder.Entity<Record>().ToTable("Record", "dbo", table =>
        {
            table.HasCheckConstraint("CK_Record_Porcentaje", "([Porcentaje]>=(1) AND [Porcentaje]<=(100))");
            table.HasCheckConstraint("CK_Record_FPS", "([FPS] IS NULL OR [FPS]>(0))");
        });
        modelBuilder.Entity<Submission>().ToTable("Submission", "dbo", table =>
        {
            table.HasCheckConstraint("CK_Submission_Porcentaje", "([Porcentaje]>=(1) AND [Porcentaje]<=(100))");
            table.HasCheckConstraint("CK_Submission_Estado", "([Estado]='Rechazado' OR [Estado]='Aprobado' OR [Estado]='Pendiente')");
            table.HasCheckConstraint("CK_Submission_FPS", "([FPS] IS NULL OR [FPS]>(0))");
            table.HasCheckConstraint("CK_Submission_MotivoRechazo", "([Estado]<>'Rechazado' OR [MotivoRechazo] IS NOT NULL)");
            table.HasCheckConstraint("CK_Submission_Revision", "([Estado]='Pendiente' AND [FechaRevision] IS NULL AND [IdModerador] IS NULL OR ([Estado]='Rechazado' OR [Estado]='Aprobado') AND [FechaRevision] IS NOT NULL AND [IdModerador] IS NOT NULL)");
        });
    }
}
