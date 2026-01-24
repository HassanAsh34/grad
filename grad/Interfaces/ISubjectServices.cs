using grad.DTO;

namespace grad.Interfaces
{
	public interface ISubjectServices
	{
		public Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken);

		public Task<ResultDTO> RemoveSubject(SubjectDTO subject, CancellationToken cancellationToken);

		public Task<ResultDTO> UpdateSubject(SubjectDTO subject,string SubjectName, CancellationToken cancellationToken);

		public Task<ResultDTO> ViewSubjectsAsync(CancellationToken cancellationToken);

		public Task<ResultDTO> AddLesson(LessonDTO lessonDTO, CancellationToken cancellationToken);

		public Task<ResultDTO> ViewLessons(LessonDTO lesson, CancellationToken cancellation);
		public Task<ResultDTO> ViewSubjectAsync(string sid, CancellationToken cancellationToken);

		public Task<bool> IsSubjectExist(string? subjectName = "", bool? deaf_mute = false, string? subjectId = "", CancellationToken cancellationToken = default);
	}
}
