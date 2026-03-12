using Grad.Application.Users.Interfaces;
using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Grad.Infrastructure.Repository
{
	public class UserRepository : Repository, IUserRepository
	{
		private readonly Db_Context _context;

		public UserRepository(Db_Context context) : base(context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<Parent?> GetParentWithStudentsAsync(Guid id, CancellationToken ct = default)
		{
			return await _context.Parents
				.Include(p => p.students)
				.Include(p => p.RefreshToken)
				.FirstOrDefaultAsync(p => p.Id == id, ct);
		}

		public async Task<Student?> GetStudentWithParentAndSubjectsAsync(Guid id, Guid? pid = null, CancellationToken ct = default)
		{
			var query = _context.Students
				.Include(s => s.parent)
				.Include(s => s.EnrolledSubjects)
				.Include(s => s.RefreshToken)
				.AsQueryable();

			if (pid.HasValue)
			{
				query = query.Where(s => s.Id == id && s.PID == pid.Value);
			}
			else
			{
				query = query.Where(s => s.Id == id);
			}

			return await query.FirstOrDefaultAsync(ct);
		}

		public async Task<Teacher?> GetTeacherWithAssignedSubjectsAsync(Guid id, CancellationToken ct = default)
		{
			return await _context.Teachers
				.Include(t => t.AssignedSubjects)
				.Include(t => t.RefreshToken)
				.FirstOrDefaultAsync(t => t.Id == id, ct);
		}

		public async Task<Admin?> GetAdminByIdAsync(Guid id, CancellationToken ct = default)
		{
			return await _context.Admins
				.Include(a => a.RefreshToken)
				.FirstOrDefaultAsync(a => a.Id == id, ct);
		}
	}
}
