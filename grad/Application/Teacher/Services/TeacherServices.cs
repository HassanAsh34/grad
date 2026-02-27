using grad.Application.Common.DTOs;
using grad.Application.lesson.DTOs;
using grad.Application.lesson.Interfaces;
using grad.Domain.Model;
using grad.Application.teacher.DTOs;
using grad.Application.teacher.Interfaces;
using grad.Application.Common.Interfaces;
using grad.Application.subject.Interfaces;
using Microsoft.EntityFrameworkCore;
using grad.Application.Users.Interfaces;
using grad.Application.subject.DTOs;
using grad.Application.Lesson.DTOs;

namespace grad.Application.teacher.Services
{
	public class TeacherServices : ITeacherServices
	{
		private readonly ISubjectServices _subjectServices;

		private readonly ILessonServices _lessonServices;

		private readonly IRepository _repository;

		private readonly IUserServices _userServices;

		public TeacherServices(ISubjectServices subjectServices,IRepository repository,ILessonServices lessonServices,IUserServices userServices)
		{
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_lessonServices = lessonServices ?? throw new ArgumentNullException(nameof(lessonServices));
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
		}

		public async Task<ResultDTO> ShowStudents(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken)
		{
			AssignedSubject assignedSubject = await _repository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == teacherSubject.SubjectId && a.TeacherId == teacherSubject.TeacherId, cancellationToken: cancellationToken);
			if (assignedSubject == null)
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "Subject wasnt found"
				};
			else 
			{
				IEnumerable<Enrollement> students = await _repository
					.GetEntitiesAsync<Enrollement>(
						e => e.SUBFK == assignedSubject.SubjectId,
						q => q.Include(e=>e.Student).Include(e=>e.Student.parent),
						cancellationToken: cancellationToken
					);
				var result = students.Select(e => new LISTStudentDTO
				{
					Id = e.Student.Id,
					name = $"{e.Student.FName} {e.Student.LName}",
					email = e.Student.EmailorUserName,
					parentInfo = e.Student.parent != null ? new parentInfo
					{
						Email = e.Student.parent.EmailorUserName,
						phoneNumber = e.Student.parent.phoneNumber
					} : null
				}).ToList();


				//IEnumerable<LISTStudentDTO> result = students.Select(e => new LISTStudentDTO
				//{
				//	Id = e.Student.Id,
				//	name = $"{e.Student.FName} {e.Student.LName}",
				//	email = e.Student.EmailorUserName,
				//	parentInfo = e.Student.parent != null ? new parentInfo
				//	{
				//		Email = e.Student.parent.EmailorUserName,
				//		phoneNumber = e.Student.parent.phoneNumber
				//	} : null
				//	//add his progress
				//	// add only what you need
				//});
				int stdCount = result.Count;

				return new ResultDTO
				{
					Message = $"{stdCount} students were found",
					StatusCode = stdCount >0 ? StatusCodes.Status200OK : StatusCodes.Status204NoContent,
					result = result
				};
			}

		}

		public async Task<ResultDTO> ViewStudent(ProfileDTO profile,CancellationToken cancellationToken)
		{
			return await _userServices.ViewProfile(profile, cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> ViewSubjects(Guid teacherId, CancellationToken cancellationToken)
		{
			return await _subjectServices.ViewSubjectsAsync(Tid: teacherId, cancellationToken: cancellationToken);	
		} //done

		public async Task<ResultDTO> ViewSubject(TeacherSubjectDTO teacherSubject,CancellationToken cancellationToken)
		{
			AssignedSubject assignedSubject = await _repository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == teacherSubject.SubjectId && a.TeacherId == teacherSubject.TeacherId, cancellationToken: cancellationToken);
			if(assignedSubject == null)
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "Subject wasnt found"
				};
			else
				return await _subjectServices.ViewSubjectAsync(assignedSubject.SubjectId, cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> ViewLessons(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken)
		{
			AssignedSubject assignedSubject = await _repository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == teacherSubject.SubjectId && a.TeacherId == teacherSubject.TeacherId, cancellationToken: cancellationToken);
			if (assignedSubject == null)
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "Subject wasnt found"
				};
			else
				return await _lessonServices.ViewLessons(assignedSubject.SubjectId, cancellationToken);
		}

		public async Task<ResultDTO> ViewLesson(LessonContentDTO lessonContent, CancellationToken cancellationToken)
		{
			//AssignedSubject assignedSubject = await _repository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == teacherSubject.SubjectId && a.TeacherId == teacherSubject.TeacherId, cancellationToken: cancellationToken);
			//if (assignedSubject == null)
			//	return new ResultDTO
			//	{
			//		StatusCode = StatusCodes.Status404NotFound,
			//		Message = "Subject wasnt found"
			//	};
			//else
			//return await _lessonServices.viewLesson(new LessonContentDTO { Id = lessonId,SubjectId = teacherSubject.SubjectId}, cancellationToken);
			return await _lessonServices.viewLesson(lessonContent, cancellationToken);
		}
		
		public async Task<ResultDTO> UploadVideo(VideoDTO video, CancellationToken cancellationToken)
		{
			return await _lessonServices.UploadVideo(video, cancellationToken);
		}

		public async Task<ResultDTO> AddLesson(AddLessonDTO lesson, CancellationToken cancellationToken)
		{
			AssignedSubject assigned = await _repository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == lesson.SubjectId && a.TeacherId == lesson.UId,cancellationToken: cancellationToken);
			if (assigned != null)
			{
				lesson.SubjectId = assigned.SubjectId;
				return await _lessonServices.AddLesson(lesson, cancellationToken);
			}
			else
				return new ResultDTO
				{
					Message = "Subject not found",
					StatusCode = StatusCodes.Status400BadRequest
				};
		}


		public async Task<ResultDTO> EditLesson(EditLessonDTO lesson, CancellationToken cancellationToken)
		{
			AssignedSubject assigned = await _repository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == lesson.SubjectId && a.TeacherId == lesson.UId, cancellationToken: cancellationToken);
			if (assigned != null)
			{
				lesson.SubjectId = assigned.SubjectId;
				return await _lessonServices.editLesson(lesson, cancellationToken);
			}
			else
				return new ResultDTO
				{
					Message = "Subject not found",
					StatusCode = StatusCodes.Status400BadRequest
				};
		}

		public async Task<ResultDTO> DeleteLesson(DeleteLessonDTO lesson, CancellationToken cancellationToken)
		{
			AssignedSubject assigned = await _repository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == lesson.SubjectId && a.TeacherId == lesson.UId, cancellationToken: cancellationToken);
			if (assigned != null)
			{
				lesson.SubjectId = assigned.SubjectId;
				return await _lessonServices.DeleteLesson(lesson, cancellationToken);
			}
			else
				return new ResultDTO
				{
					Message = "Subject not found",
					StatusCode = StatusCodes.Status400BadRequest
				};
		}

		public async Task<ResultDTO> addWords(AddVocabDTO vocabDTO, CancellationToken cancellationToken)
		{
			AssignedSubject assignedSubject = await _repository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == vocabDTO.sid && a.TeacherId == vocabDTO.Tid, cancellationToken: cancellationToken);
			if (assignedSubject == null)
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "Subject wasnt found"
				};
			else
			{
				vocabDTO.sid = assignedSubject.SubjectId;
				return await _subjectServices.addwords(vocabDTO, cancellationToken);
			}
		}

	}
}
