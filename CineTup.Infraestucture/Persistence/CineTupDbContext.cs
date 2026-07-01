using CineTup.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CineTup.Infrastructure.Persistance
{
    public class CineTupDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<ShowTime> ShowTimes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<SysAdmin> SysAdmins { get; set; }
        public DbSet<User> Users { get; set; }

        public CineTupDbContext(DbContextOptions<CineTupDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.UseTpcMappingStrategy();
            });

            modelBuilder.Entity<ShowTime>()
                .Property(st => st.TicketPrice)
                .HasPrecision(18, 2);
        }
    }
}