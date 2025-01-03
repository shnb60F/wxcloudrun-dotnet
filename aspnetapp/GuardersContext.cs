using System;
using System.Collections.Generic;
using aspnetapp.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace aspnetapp
{
    public partial class GuardersContext : DbContext
    {
        public GuardersContext()
        {
        }
        public DbSet<GuarderDB> Guarders { get; set; }
        public DbSet<QuestionDB> Questions { get; set; }
        public DbSet<TreasureBoxDB> TreasureBoxs { get; set; }
        public DbSet<UserDB> Users { get; set; }
        public GuardersContext(DbContextOptions<GuardersContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(
                    "server=localhost;port=3306;user=root;password=123456;database=guarders;",
                    ServerVersion.AutoDetect("server=localhost;port=3306;user=root;password=123456;database=guarders;")
                );
            }
#else
            if (!optionsBuilder.IsConfigured)
            {
                var username = Environment.GetEnvironmentVariable("MYSQL_USERNAME");
                var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
                var addressParts = Environment.GetEnvironmentVariable("MYSQL_ADDRESS")?.Split(':');
                var host = addressParts?[0];
                var port = addressParts?[1];
                var connstr = $"server={host};port={port};user={username};password={password};database=guarders";
                optionsBuilder.UseMySql(connstr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.18-mysql"));
            }
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Guarder复合主键
            modelBuilder.Entity<GuarderDB>()
                .HasKey(g => new { g.UserId, g.GuarderType });
            //Question复合主键
            modelBuilder.Entity<QuestionDB>()
                .HasKey(q => new { q.UserId, q.QuestionID });
        }
    }
}
