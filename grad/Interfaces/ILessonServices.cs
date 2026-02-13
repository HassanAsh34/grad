using grad.DTO;

namespace grad.Interfaces
{
	public interface ILessonServices
	{
		public Task<ResultDTO> AddLesson(LessonDTO lessonDTO, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ViewLessons(Guid sid, CancellationToken cancellation = default);

		public Task<ResultDTO> viewLesson(LessonDTO lessonDTO, CancellationToken cancellationToken);

		public Task<ResultDTO> editLesson(EditLessonDTO lessonDTO, CancellationToken cancellationToken);
		public Task<ResultDTO> DeleteLesson(Guid sid, Guid lid, CancellationToken cancellationToken);
	}
}
