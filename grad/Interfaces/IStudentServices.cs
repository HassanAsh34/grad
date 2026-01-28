using grad.DTO;

namespace grad.Interfaces
{
	public interface IStudentServices
	{
		public Task<ResultDTO> EnrollSubject(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken);
		public Task<ResultDTO> viewSubject(Guid Sid, CancellationToken cancellationToken);
		public Task<ResultDTO> ViewSubjects(Guid id, CancellationToken cancellationToken);
	}
}
