using grad.DTO;

namespace grad.Interfaces
{
	public interface ISubjectServices
	{
		public Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken);

		public Task<ResultDTO> RemoveSubject(SubjectDTO subject, CancellationToken cancellationToken);

		public Task<ResultDTO> UpdateSubject(SubjectDTO subject, CancellationToken cancellationToken);

		public Task<ResultDTO> ViewSubjects(CancellationToken cancellationToken);

	}
}
