using Grad.Application.Common.DTOs;
using Grad.Application.ParentFeatures.DTOs;

namespace Grad.Application.ParentFeatures.Interfaces
{
	public interface IParentServices
	{
		public Task<ResultDTO> registerStudent(RegisterStudentDTO student, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> activateAccount(LoginDTO login,Guid parentId, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ShowChildren(Guid pid, CancellationToken cancellationToken);

		public Task<ResultDTO> viewProfile(Guid pid, Guid Sid, CancellationToken cancellationToken);

		public Task<ResultDTO> DeleteStudent(ProfileDTO profile, Guid pid, CancellationToken cancellationToken);
	}
}
