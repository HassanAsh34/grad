using grad.DTO;

namespace grad.Interfaces
{
	public interface ISubjectServices
	{
		public Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken);

		public Task<ResultDTO> RemoveSubject(SubjectDTO subject, CancellationToken cancellationToken);

		public Task<ResultDTO> UpdateSubject(SubjectDTO subject,string SubjectName, CancellationToken cancellationToken);

		public Task<ResultDTO> ViewSubjectsAsync(CancellationToken cancellationToken);

		public Task<ResultDTO> ViewSubjectAsync(SubjectDTO subject, CancellationToken cancellationToken);

	}
}
