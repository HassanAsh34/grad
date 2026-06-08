using Grad.Application.Common.Interfaces;
using Grad.Application.StudentFeatures.DTOs;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.StudentFeatures.Interfaces
{
	public interface IStudentRepository : IRepository
	{
		//Task<List<Enrollement>> GetEnrollementsAsync(Guid Sid, CancellationToken CT = default);

		//Task<Dictionary<Guid, Enrollement>> GetEnrollementsAsync(Guid Sid, CancellationToken CT = default);

		//Task<Dictionary<Guid, EnrollmentDTO>> GetEnrollementsAsync(Guid Sid, CancellationToken ct);

		Task<List<Enrollment>> GetEnrollementsAsync(Guid Sid, CancellationToken ct = default);

		Task<Enrollment> GetEnrollementAsync(Guid SID,Guid STDID,CancellationToken CT = default);

		//Task<bool> IsEnrolled(Guid Sid, Guid StdId, CancellationToken CT = default);
		Task<int> EnrollSubject(Enrollment enrollement, CancellationToken CT = default);
		Task<DisablityType> GetDisablityTypeAsync(Guid Sid, CancellationToken CT = default);
	}
}
