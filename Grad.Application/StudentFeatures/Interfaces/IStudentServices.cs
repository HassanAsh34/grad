using Grad.Application.Common.DTOs;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.StudentFeatures.DTOs;
using Grad.Application.SubmissionFeatures.DTOs;
namespace Grad.Application.StudentFeatures.Interfaces
{
	public interface IStudentServices
	{
		public Task<ResultDTO> EnrollSubject(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken = default);
		//public Task<ResultDTO> viewSubject(Guid Sid,CancellationToken cancellationToken = default);
		//public Task<ResultDTO> viewSubject(Guid Sid, Guid guid, CancellationToken cancellationToken);
		//public Task<ResultDTO> ViewSubjects(Guid id, CancellationToken cancellationToken);
		public Task<ResultDTO> ViewSubjects(Guid id, bool Enrolled = false, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewExerciseQuize(LevelDTO levelDTO, Guid stdID, CancellationToken cancellationToken = default);

		public Task<ResultDTO> viewLessons(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken = default);

		public Task<ResultDTO> viewLesson(LessonContentDTO lessonDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> createSubmission(CreateSubmissionDTO createSubmission, CancellationToken cancellationToken = default);

		public Task<ResultDTO> completeLesson(CompletelessonDTO lessonDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> viewSubmissions(Guid stdID, CancellationToken cancellationToken = default);
	}
}
