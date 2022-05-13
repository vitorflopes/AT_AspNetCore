using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Data
{
    public class ApplicationDbContext : IdentityDbContext<DetalheUsuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Amizade> Amizade { get; set; }

        public DbSet<Post> Post { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Amizade>().ToTable("Amizade");
            modelBuilder.Entity<DetalheUsuario>().ToTable("Usuario");

            modelBuilder.Entity<Amizade>()
                .HasKey(a => new { a.UsuarioIdA, a.UsuarioIdB });

            modelBuilder.Entity<Amizade>()
                .HasOne(a => a.UsuarioA)
                .WithMany(u => u.AmizadesSolicitadas)
                .HasForeignKey(a => a.UsuarioIdA)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Amizade>()
                .HasOne(a => a.UsuarioB)
                .WithMany(u => u.AmizadesRecebidas)
                .HasForeignKey(a => a.UsuarioIdB)
                .OnDelete(DeleteBehavior.Restrict);
        }

        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetalheUsuario>().HasKey(x => x.Id) .
            HasOptional(e => e.Manager).
            WithMany().
            HasForeignKey(m => m.ManagerID);
        }*/
    }
    
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile
                (@Directory.GetCurrentDirectory() + "/../AT_WebApi/appsettings.json").Build();
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DatabaseConnection");
            builder.UseSqlServer(connectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
