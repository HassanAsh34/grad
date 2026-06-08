using Grad.Application.Common.Interfaces;
using Grad.Domain.Model;

namespace Grad.Application.TeacherFeatures.Interfaces
{
	public interface ITeacherRepository : IRepository
	{
		public Task<List<Student>> showStudents(Guid Sid, CancellationToken CT = default);
		public Task<bool> CanAccess(Guid ?Tid, Guid ?Sid, CancellationToken CT = default);

		public Task<List<Guid>> GetAssignedSubjectIDs(Guid teacherId, CancellationToken cancellationToken = default);
	}
}
