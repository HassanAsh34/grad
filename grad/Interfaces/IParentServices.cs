using grad.DTO;

namespace grad.Interfaces
{
	public interface IParentServices
	{
		public Task<ResultDTO> registerStudent(RegisterStudentDTO student, CancellationToken cancellationToken);

		public Task<ResultDTO> activateAccount(LoginDTO login, CancellationToken cancellationToken);
		public Task<ResultDTO> ShowChildren(string? pid, CancellationToken cancellationToken);
	}
}
