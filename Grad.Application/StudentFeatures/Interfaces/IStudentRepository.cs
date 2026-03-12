using Grad.Application.Common.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.StudentFeatures.Interfaces
{
	public interface IStudentRepository : IRepository
	{
		Task<List<Enrollement>> GetEnrollementsAsync(Guid Sid, CancellationToken CT = default);
		Task<bool> IsEnrolled(Guid Sid, Guid StdId, CancellationToken CT = default);
		Task<int> EnrollSubject(Enrollement enrollement, CancellationToken CT = default);
		Task<DisablityType> GetDisablityTypeAsync(Guid Sid, CancellationToken CT = default);
	}
}
