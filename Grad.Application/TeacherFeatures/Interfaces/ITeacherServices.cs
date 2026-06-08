using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.QAFeature.DTO;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Application.TeacherFeatures.DTOs;
using Grad.Domain.Enums;


namespace Grad.Application.TeacherFeatures.Interfaces
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

		public Task<ResultDTO> EditLesson(EditLessonDTO lesson, CancellationToken cancellationToken = default);

		public Task<ResultDTO> DeleteLesson(DeleteLessonDTO lessonDTO, CancellationToken cancellationToken = default);

		//public Task<ResultDTO> addWords(TeacherSubjectDTO teacherSubject, AddVocabDTO vocabDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> CreateExercise(CreateLevelDTO createExerciseDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> addWords(AddVocabDTO vocabDTO, CancellationToken cancellationToken = default);

		public Task<ResultDTO> GetQuizes(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken = default);

		public Task<ResultDTO> ViewLevel(LevelDTO level, Guid Tid, CancellationToken CT = default );

		public Task<ResultDTO> EditLevel(EditLevelDTO editLevel, CancellationToken CT = default);

		public Task<ResultDTO> ListPerquisites(TeacherSubjectDTO teacherSubject, PerquisiteType type, CancellationToken cancellationToken = default);

		public Task<ResultDTO> DeleteLevel(LevelDTO level, Guid Tid, CancellationToken CT = default);

		public Task<ResultDTO> DeleteVideo(VideoDTO videoDTO, Guid Tid, CancellationToken cancellationToken = default);

		public Task<ResultDTO> listInqueries(Guid TId, Guid Sid, CancellationToken CT = default);
		public Task<ResultDTO> viewInquery(Guid Id, Guid TId, CancellationToken cancellationToken = default);
		public Task<ResultDTO> createInquery(InqueryDTO inquery, Guid TId, CancellationToken cancellationToken = default);

		public Task<ResultDTO> uploadCV(CVDTO cv, CancellationToken cancellationToken = default);



		//public Task<ResultDTO> viewCV(CVDTO cv, CancellationToken cancellationToken = default);
	}
}
