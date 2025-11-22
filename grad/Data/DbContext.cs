using Microsoft.EntityFrameworkCore;
using grad.Model;
namespace grad.Data
{
	public class Db_Context : DbContext
	{
		public Db_Context(DbContextOptions<Db_Context> options) : base(options)
		{
			
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Admin> Admins { get; set; }
		public DbSet<Student> Students { get; set; }
		public DbSet<Parent> Parents { get; set; }

		public DbSet<Teacher> Teachers { get; set; }

		public DbSet<Subject> subjects { get; set; }

		public DbSet<RefreshToken> RefreshTokens { get; set; }

		public DbSet<EnrolledStudent> EnrolledSubjects { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>().ToTable("users");

			modelBuilder.Entity<Admin>().ToTable("Admins");

			modelBuilder.Entity<Parent>().ToTable("Parents");

			modelBuilder.Entity<Teacher>().ToTable("Teachers");
			
			//modelBuilder.Entity<Subject>().HasAlternateKey(s => s.Name);

			modelBuilder.Entity<Subject>().HasMany(s => s.Teachers).WithOne(t => t.Subject).HasForeignKey(t => t.SubjectFK).OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<EnrolledStudent>().HasAlternateKey(es => new { es.STUFK, es.SUBFK });

			modelBuilder.Entity<EnrolledStudent>().HasOne(S=>S.subject).WithMany(s => s.Students).HasForeignKey(S => S.SUBFK).OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<EnrolledStudent>().HasOne(S => S.Student).WithMany(s => s.EnrolledSubjects).HasForeignKey(S => S.STUFK).OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Student>()
			.HasOne(s => s.parent)
			.WithMany(p => p.students)
			.HasForeignKey(s => s.PID)
			.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<RefreshToken>()
				.HasOne(t=>t.User)
				.WithOne(u=>u.RefreshToken)
				.HasForeignKey<RefreshToken>(t=>t.CreatedById)
				.OnDelete(DeleteBehavior.Cascade);



		}
	}
}
