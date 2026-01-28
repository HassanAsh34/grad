using grad.DTO;

namespace grad.Interfaces
{
	public interface ITeacherServices
	{
		public Task<ResultDTO> ViewLessons(Guid sid, CancellationToken cancellation = default);
		public Task<ResultDTO> AddLesson(LessonDTO lesson, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ShowStudents(Guid Sid, CancellationToken cancellation = default);
	}
}
