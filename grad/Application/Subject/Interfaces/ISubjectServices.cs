using Grad_Structured.Application.Common.DTOs;
using Grad_Structured.Application.subject.DTOs;

namespace Grad_Structured.Application.subject.Interfaces
{
	public interface ISubjectServices
	{
		public Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken = default);

		public Task<ResultDTO> RemoveSubject(Guid subjectid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> UpdateSubject(Guid subjectid,string SubjectName, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewSubjectsAsync(Guid ?Tid = null,List<Guid> guids = null, int disability = -1, bool enrolled = false, CancellationToken cancellationToken = default);
		//public Task<ResultDTO> ViewSubjectsAsync(int disability = -1, CancellationToken cancellationToken = default);

		public Task<bool> IncreaseLessonCount(Guid subjectId,CancellationToken cancellationToken);
		//public Task<ResultDTO> AddLesson(AddLessonDTO lessonDTO, CancellationToken cancellationToken = default);
		//public Task<ResultDTO> AddLesson(VideoDTO lessonDTO, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> ViewLessons(Guid lessonid, CancellationToken cancellation = default);
		public Task<ResultDTO> ViewSubjectAsync(Guid sid,bool all = true, CancellationToken cancellationToken = default);

		public Task<ResultDTO> addwords(AddVocabDTO vocabDTO, CancellationToken cancellationToken = default);

		public Task<bool> IsSubjectExist(string? subjectName = "", bool? deaf_mute = false, Guid? subjectId = null, CancellationToken cancellationToken = default);
	}
}
