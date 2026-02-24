using Grad_Structured.Application.Common.DTOs;
using Grad_Structured.Application.lesson.DTOs;
using Grad_Structured.Application.Lesson.DTOs;
using Grad_Structured.Application.student.DTOs;
namespace Grad_Structured.Application.student.Interfaces
{
	public interface IStudentServices
	{
		public Task<ResultDTO> EnrollSubject(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken = default);
		//public Task<ResultDTO> viewSubject(Guid Sid,CancellationToken cancellationToken = default);
		public Task<ResultDTO> viewSubject(Guid Sid, Guid guid, CancellationToken cancellationToken);
		//public Task<ResultDTO> ViewSubjects(Guid id, CancellationToken cancellationToken);
		public Task<ResultDTO> ViewSubjects(Guid id, bool Enrolled = false, CancellationToken cancellationToken = default);

		public Task<ResultDTO> viewLessons(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken = default);

		public Task<ResultDTO> viewLesson(LessonContentDTO lessonDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> completeLesson(CompletelessonDTO lessonDTO, CancellationToken cancellationToken);
	}
}
