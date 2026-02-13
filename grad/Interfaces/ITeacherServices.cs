using grad.DTO;

namespace grad.Interfaces
{
	public interface ITeacherServices
	{
		public Task<ResultDTO> ViewLessons(Guid sid, CancellationToken cancellation = default);
		public Task<ResultDTO> AddLesson(LessonDTO lesson, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ShowStudents(Guid Sid, CancellationToken cancellation = default);

		public Task<ResultDTO> ViewSubject(Guid sid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> EditLesson(EditLessonDTO lesson, CancellationToken cancellationToken = default);

		public Task<ResultDTO> DeleteLesson(Guid sid, Guid lid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> addWords(AddVocabDTO vocabDTO, CancellationToken cancellationToken = default);
	}
}
