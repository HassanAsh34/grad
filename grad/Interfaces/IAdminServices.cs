using grad.DTO;

namespace grad.Interfaces
{
	public interface IAdminServices
	{
		public Task<ResultDTO> BlockUser(string uid, CancellationToken cancellationToken);
		public Task<ResultDTO> EndSession(string uid, CancellationToken cancellationToken);
		public Task<ResultDTO> GetAllUsers(CancellationToken cancellationToken);

		public Task<ResultDTO> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken);

		//public Task<ResultDTO> ActivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken);

		//public Task<ResultDTO> DeactivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken);

		public Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken);

		public Task<ResultDTO> ViewSubjects(CancellationToken cancellationToken);

		public Task<ResultDTO> ViewSubject(string sid, CancellationToken cancellation);

		public Task<ResultDTO> RemoveSubject(string sid, CancellationToken cancellationToken);

		public Task<ResultDTO> UpdateSubject(string sid, CancellationToken cancellationToken);

		public Task<ResultDTO> AssignTeacherToSubject(AssignTeacherDTO assignTeacherDTO, CancellationToken cancellationToken);

	}
}
