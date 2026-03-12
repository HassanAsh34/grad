using Grad.Application.TeacherFeatures.Interfaces;
using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Grad.Infrastructure.Repository
{
	public class TeacherRepository : Repository , ITeacherRepository
	{
		private readonly Db_Context _context;
		
		public TeacherRepository(Db_Context context) : base (context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<bool> CanAccess(Guid ?Tid, Guid ?Sid, CancellationToken CT)
		{	
			var res = Tid != null ? Sid != null ? await  _context.AssignedSubjects.Where(a => a.SubjectId == Sid && a.TeacherId == Tid).FirstOrDefaultAsync(CT) : null :null;
			return res != null ? true : false;
		}

		public async Task<List<Student>> showStudents(Guid Sid,CancellationToken CT)
		{
			return await _context.Enrollents.Where(e => e.SUBFK == Sid).Select(s=>s.Student!).ToListAsync();
		}
	}
}
