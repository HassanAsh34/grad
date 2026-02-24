using Grad_Structured.Application.Common.DTOs;
using Grad_Structured.Application.lesson.DTOs;
using Grad_Structured.Application.Lesson.DTOs;

namespace Grad_Structured.Application.lesson.Interfaces
{
	public interface ILessonServices
	{
		//public Task<ResultDTO> AddLesson(VideoDTO lessonDTO, CancellationToken cancellationToken = default)


		public Task<ResultDTO> AddLesson(AddLessonDTO lessonDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> UploadVideo(VideoDTO videoDTO, CancellationToken cancellationToken);

		public Task<ResultDTO> DeleteVideo(VideoDTO videoDTO, CancellationToken cancellationToken);

		public Task<ResultDTO> DeleteLesson(LessonContentDTO lessonContent, CancellationToken cancellationToken);


		public Task<ResultDTO> ViewLessons(Guid sid, CancellationToken cancellation = default);

		public Task<ResultDTO> viewLesson(LessonContentDTO lessonContentDTO, CancellationToken cancellationToken);

		public Task<ResultDTO> completeLesson(CompletelessonDTO Completelesson, CancellationToken cancellationToken);

		//public Task<ResultDTO> viewLesson(VideoDTO lessonDTO, CancellationToken cancellationToken);

		//public Task<ResultDTO> editLesson(EditLessonDTO lessonDTO, CancellationToken cancellationToken);
		//public Task<ResultDTO> DeleteLesson(Guid sid, Guid lid, CancellationToken cancellationToken);
	}
}
