using Grad.Application.Common.DTOs;
using Grad.Application.ParentFeatures.DTOs;
using Grad.Application.QAFeature.DTO;

namespace Grad.Application.ParentFeatures.Interfaces
{
	public interface IParentServices
	{
		public Task<ResultDTO> registerStudent(RegisterStudentDTO student, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> activateAccount(LoginDTO login,Guid parentId, CancellationToken cancellationToken = default = default);
		public Task<ResultDTO> ShowChildren(Guid pid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> viewProfile(Guid pid, Guid Sid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> DeleteStudent(ProfileDTO profile, Guid pid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewSubjects(Guid sid, Guid pid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewSubjectStats(Guid stdid, Guid sid, Guid pid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> listInqueries(Guid UId, CancellationToken CT = default);

		public Task<ResultDTO> viewInquery(Guid Id, CancellationToken cancellationToken = default);
		public Task<ResultDTO> createInquery(InqueryDTO inquery, CancellationToken cancellationToken = default);
	}
}
