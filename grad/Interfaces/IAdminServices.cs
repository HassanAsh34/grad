using grad.DTO;

namespace grad.Interfaces
{
	public interface IAdminServices
	{
		public Task<ResultDTO> BlockUser(ProfileDTO? profileDTO, CancellationToken cancellationToken);
		public Task<ResultDTO> EndSession(ProfileDTO? profileDTO, CancellationToken cancellationToken);
		public Task<ResultDTO> GetAllUsers(CancellationToken cancellationToken);

		public Task<ResultDTO> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken);

		public Task<ResultDTO> ActivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken);

		public Task<ResultDTO> DeactivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken);

		



	}
}
