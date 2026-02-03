using grad.DTO;

namespace grad.Interfaces
{
	public interface IStudentServices
	{
		public Task<ResultDTO> EnrollSubject(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken = default);
		public Task<ResultDTO> viewSubject(Guid Sid, CancellationToken cancellationToken = default);
		//public Task<ResultDTO> ViewSubjects(Guid id, CancellationToken cancellationToken);
		public Task<ResultDTO> ViewSubjects(Guid id, bool Enrolled = false, CancellationToken cancellationToken = default);

		public Task<ResultDTO> viewLessons(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken = default);

		public Task<ResultDTO> viewLesson(LessonDTO lessonDTO, CancellationToken cancellationToken = default);
	}
}
