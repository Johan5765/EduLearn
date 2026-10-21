using System;
using System.Collections.Generic;
using Aula.Models;
using Microsoft.EntityFrameworkCore;

namespace Aula.Data;

public partial class EduLearnDbContext : DbContext
{
    public EduLearnDbContext()
    {
    }

    public EduLearnDbContext(DbContextOptions<EduLearnDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Curso> Cursos { get; set; }

    public virtual DbSet<Evaluacione> Evaluaciones { get; set; }

    public virtual DbSet<Inscripcione> Inscripciones { get; set; }

    public virtual DbSet<Leccione> Lecciones { get; set; }

    public virtual DbSet<Pregunta> Preguntas { get; set; }

    public virtual DbSet<Recurso> Recursos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => e.CursoId).HasName("PK__Cursos__7E0235D74D6023D8");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Titulo).HasMaxLength(150);

            entity.HasOne(d => d.Profesor).WithMany(p => p.Cursos)
                .HasForeignKey(d => d.ProfesorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cursos__Profesor__3E52440B");
        });

        modelBuilder.Entity<Evaluacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Evaluaci__3214EC073676F0F5");

            entity.Property(e => e.Titulo).HasMaxLength(200);

            entity.HasOne(d => d.Leccion).WithMany(p => p.Evaluaciones)
                .HasForeignKey(d => d.LeccionId)
                .HasConstraintName("FK__Evaluacio__Lecci__48CFD27E");
        });

        modelBuilder.Entity<Inscripcione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inscripc__3214EC07E4026B50");

            entity.HasIndex(e => new { e.CursoId, e.EstudianteId }, "UQ_Inscripcion").IsUnique();

            entity.Property(e => e.FechaInscripcion).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Curso).WithMany(p => p.Inscripciones)
                .HasForeignKey(d => d.CursoId)
                .HasConstraintName("FK__Inscripci__Curso__4F7CD00D");
        });

        modelBuilder.Entity<Leccione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Leccione__3214EC07140FF5E3");

            entity.Property(e => e.Titulo).HasMaxLength(200);

            entity.HasOne(d => d.Curso).WithMany(p => p.Lecciones)
                .HasForeignKey(d => d.CursoId)
                .HasConstraintName("FK__Lecciones__Curso__4316F928");
        });

        modelBuilder.Entity<Pregunta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pregunta__3214EC07996C4EDB");

            entity.Property(e => e.Enunciado).HasMaxLength(500);

            entity.HasOne(d => d.Evaluacion).WithMany(p => p.Pregunta)
                .HasForeignKey(d => d.EvaluacionId)
                .HasConstraintName("FK__Preguntas__Evalu__4BAC3F29");
        });

        modelBuilder.Entity<Recurso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Recursos__3214EC07AF3FC57B");

            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Url).HasMaxLength(500);

            entity.HasOne(d => d.Leccion).WithMany(p => p.Recursos)
                .HasForeignKey(d => d.LeccionId)
                .HasConstraintName("FK__Recursos__Leccio__45F365D3");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Roles__F92302F17FC1E8D5");

            entity.Property(e => e.Nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuarios__2B3DE7B89472B5E7");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A190723052E").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__RolId__3A81B327");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
