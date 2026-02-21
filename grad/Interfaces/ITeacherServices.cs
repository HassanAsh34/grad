using grad.DTO;

namespace grad.Interfaces
{
	public interface ITeacherServices
	{
		//public Task<ResultDTO> ViewLessons(Guid sid, CancellationToken cancellation = default);
		public Task<ResultDTO> AddLesson(AddLessonDTO lesson, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewSubjects(Guid teacherId, CancellationToken cancellationToken = default);
		//public Task<ResultDTO> ShowStudents(Guid Sid, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> ViewSubjects(Guid teacherId, cancellationTokenToken cancellationTokenToken= default);
		public Task<ResultDTO> ShowStudents(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewLessons(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken = default);
		public Task<ResultDTO> ViewSubject(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewLesson(LessonContentDTO lessonContent, CancellationToken cancellationToken= default);

		public Task<ResultDTO> UploadVideo(VideoDTO video, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewStudent(ProfileDTO profile, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> EditLesson(EditLessonDTO lesson, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> DeleteLesson(Guid sid, Guid lid, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> addWords(TeacherSubjectDTO teacherSubject, AddVocabDTO vocabDTO, CancellationToken cancellationToken = default);


		//public Task<ResultDTO> addWords(AddVocabDTO vocabDTO, CancellationToken cancellationToken = default);
	}
}
