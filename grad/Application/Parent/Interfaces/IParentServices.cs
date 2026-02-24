using Grad_Structured.Application.Common.DTOs;
using Grad_Structured.Application.parent.DTOs;

namespace Grad_Structured.Application.parent.Interfaces
{
	public interface IParentServices
	{
		public Task<ResultDTO> registerStudent(RegisterStudentDTO student, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> activateAccount(LoginDTO login,Guid parentId, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ShowChildren(Guid pid, CancellationToken cancellationToken);

		public Task<ResultDTO> viewProfile(Guid pid, Guid Sid, CancellationToken cancellationToken);
	}
}
