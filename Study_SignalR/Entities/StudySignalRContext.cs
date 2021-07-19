using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study_SignalR.Entities
{
	public class StudySignalRContext:DbContext
	{
		public DbSet<AppUser> AppUsers { get; set; }
		public DbSet<AppMessage> AppMessages{ get; set; }

		public StudySignalRContext(DbContextOptions options):base(options)
		{}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<AppMessage>()
				.Property(m => m.Message)
				.HasMaxLength(2048);
			modelBuilder.Entity<AppMessage>()
				.HasOne(m => m.Sender)
				.WithMany(u => u.SendMesg)
				.HasForeignKey(m => m.SenderId)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<AppMessage>()
				.HasOne(m => m.Receiver)
				.WithMany(u => u.ReceivedMesg)
				.HasForeignKey(m => m.ReceiverId)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<AppUser>()
				.Property(m => m.Username)
				.HasMaxLength(100);
			modelBuilder.Entity<AppUser>()
				.HasIndex(m => m.Username)
				.IsUnique();
			modelBuilder.Entity<AppUser>()
				.Property(m => m.Password)
				.HasMaxLength(200);
			modelBuilder.Entity<AppUser>()
				.Property(m => m.FullName)
				.HasMaxLength(100);
			modelBuilder.Entity<AppUser>()
				.Property(m => m.MessageKey)
				.HasMaxLength(100);
		}
	}
}
