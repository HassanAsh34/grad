using grad.DTO;

namespace grad.Interfaces
{
	public interface IAdminServices
	{
		//public Task<ResultDTO> BlockUser(Guid uid, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ToggleBan(Guid UID, CancellationToken cancellationToken);
		public Task<ResultDTO> EndSession(Guid uid, CancellationToken cancellationToken = default);
		public Task<ResultDTO> GetAllUsers(string scheme, string host,CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> ActivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> DeactivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewSubjects(CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewSubject(Guid sid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> RemoveSubject(Guid sid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> UpdateSubject(Guid sid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> AssignTeacherToSubject(TeacherSubjectDTO assignTeacherDTO, CancellationToken cancellationToken = default);

	}
}
