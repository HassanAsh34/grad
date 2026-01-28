using grad.DTO;

namespace grad.Interfaces
{
	public interface IParentServices
	{
		public Task<ResultDTO> registerStudent(RegisterStudentDTO student, CancellationToken cancellationToken = default);

		public Task<ResultDTO> activateAccount(LoginDTO login,Guid parentId, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ShowChildren(Guid pid, CancellationToken cancellationToken);
	}
}
