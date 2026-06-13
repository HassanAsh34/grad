using Grad.Application.Common.DTOs;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Domain.Model;


namespace Grad.Application.LessonFeatures.Interfaces
{
	public interface ILessonServices
	{
		//public Task<ResultDTO> AddLesson(VideoDTO lessonDTO, CancellationToken cancellationToken = default)


		public Task<ResultDTO> AddLesson(AddLessonDTO lessonDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> UploadVideo(VideoDTO videoDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> DeleteVideo(VideoDTO videoDTO, CancellationToken cancellationToken = default);

		public Task<List<LessonContentDTO>> ViewLessons(Guid sid,CancellationToken cancellation = default);

		public Task<ResultDTO> viewLesson(LessonContentDTO lessonContentDTO,bool completed = false,CancellationToken cancellationToken = default);

		public Task<ResultDTO> DeleteLesson(DeleteLessonDTO deleteLesson, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> viewLesson(VideoDTO lessonDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> editLesson(EditLessonDTO lessonDTO, CancellationToken cancellationToken = default);
		//public Task<ResultDTO> DeleteLesson(Guid sid, Guid lid, CancellationToken cancellationToken);
	}
}
