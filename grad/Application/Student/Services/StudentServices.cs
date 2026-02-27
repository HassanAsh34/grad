using grad.Application.Common.Interfaces;
using grad.Application.lesson.DTOs;
using grad.Application.lesson.Interfaces;
using grad.Application.subject.Interfaces;
using grad.Application.student.DTOs;
using grad.Application.student.Interfaces;
using grad.Domain.Enums;
using grad.Application.Common.DTOs;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using grad.Domain.Model;
using grad.Infrastructure.Persistence;
using grad.Application.subject.DTOs;
using grad.Application.Lesson.DTOs;

namespace grad.Application.student.Services
{
	public class StudentServices : IStudentServices
	{
		private readonly ISubjectServices _subjectServices;
		private readonly IRepository _Repository;
		private readonly IMongoCollection<StudentProgress> _studentProgress;
		private readonly IUowServices _uowServices;
		private readonly ILessonServices _lessonServices;

		public StudentServices(ISubjectServices subjectServices,IRepository repository,IUowServices uow,ILessonServices lessonServices,MongoDBContext context)
		{
			_subjectServices = subjectServices ??  throw new ArgumentNullException(nameof(subjectServices));
			_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_uowServices = uow ?? throw new ArgumentNullException(nameof(uow));
			_studentProgress = context.StudentProgress ?? throw new ArgumentNullException(nameof(context));
			_lessonServices = lessonServices ?? throw new ArgumentException(nameof(lessonServices));
		}

		public async Task<ResultDTO> ViewSubjects(Guid id,bool Enrolled,CancellationToken cancellationToken)
		{
			Student student = null;
			student = await _Repository.GetEntityAsync<Student>(s => s.Id == id, include: q => q.Include(s => s.EnrolledSubjects),cancellationToken);
			//if (Enrolled)
			//{
			//}
			//else
			//{
			//	student = await _Repository.GetEntityAsync<Student>(s => s.Id == id,cancellationToken: cancellationToken);
			//}
			if(student != null)
			{
				int disability = student.Disability switch
				{
					DisablityType.Hearing => 2,
					DisablityType.Speech => 3,
					_ => 1
				};
				List<Guid> guids = student.EnrolledSubjects.Select(s=>s.SUBFK).ToList();
				if (Enrolled)
				{
					
					if (guids.Count == 0)
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status404NotFound,
							Message = "No subjects yet 😊 Let’s add one and start learning!"
						};
					else
					{
						return await _subjectServices.ViewSubjectsAsync(guids: guids, disability: disability, cancellationToken: cancellationToken,enrolled: Enrolled);
					}
				}
				else
				{
					return await _subjectServices.ViewSubjectsAsync(guids: guids,disability: disability, cancellationToken: cancellationToken);
				}
				//if(student.EnrolledSubjects == null)
				//{
				//	switch(student.Disability)
				//	{
				//		case Student.DisablityType.Hearing:
				//			return await _subjectServices.ViewSubjectsAsync(disability: 2,cancellationToken: cancellationToken);
				//		case Student.DisablityType.Speech:
				//			return await _subjectServices.ViewSubjectsAsync(disability: 3,cancellationToken: cancellationToken);
				//		default:
				//			return await _subjectServices.ViewSubjectsAsync(disability: 1,cancellationToken: cancellationToken);
				//	}
				//}
				//else
				//{
				//	List<Guid> guids = student.EnrolledSubjects.Select(s => s.Id).ToList();

				//	if (guids.Count == 0)
				//		return new ResultDTO
				//		{
				//			StatusCode = StatusCodes.Status404NotFound,
				//			Message = "No subjects yet 😊 Let’s add one and start learning!"
				//		};
				//	else
				//	{
				//int disability = student.Disability switch
				//		{
				//			Student.DisablityType.Hearing => 2,
				//			Student.DisablityType.Speech => 3,
				//			_ => 1
				//		};
				//		return await _subjectServices.ViewSubjectsAsync(guids: guids,disability,Enrolled, cancellationToken: cancellationToken);
				//	}
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status401Unauthorized,
					Message = "Something went Wrong"
				};
			}
		}

		public async Task<ResultDTO> viewSubject(Guid Sid,Guid guid,CancellationToken cancellationToken)
		{
			Enrollement enrollement = await _Repository.GetEntityAsync<Enrollement>(e => e.SUBFK == Sid && e.STUFK == guid,q=>q.Include(e=>e.studentProgresses), cancellationToken: cancellationToken);
			ResultDTO res = await _subjectServices.ViewSubjectAsync(Sid, false, cancellationToken);
			if (enrollement == null)
			{
				return res;
			}
			else
			{
				if (res.StatusCode == StatusCodes.Status200OK && enrollement.studentProgresses.Count() > 0)
				{
					
					SubjectDTO subjectDTO = (SubjectDTO)res.result;
					if (subjectDTO != null) 
					{
						subjectDTO.progress = subjectDTO.lessonsCount > 0 ? (enrollement.studentProgresses.Count() / (float)subjectDTO.lessonsCount) * 100:0;
					}
					res.result = subjectDTO;
					return res;
				}
				else
					return res;
			}
		}

		public async Task<ResultDTO> EnrollSubject(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken)
		{
			//enrollSubject.stdFK = enrollSubject.stdFK.Trim();
			//enrollSubject.subFK = enrollSubject.subFK.Trim();
			if (await _Repository.GetEntityAsync<Student>(s => s.Id == enrollSubject.stdFK) is Student student)
			{
				if (await _Repository.GetEntityAsync<Subject>(s => s.Id == enrollSubject.subFK) is Subject subject)
				{
					Enrollement application = await _Repository.GetEntityAsync<Enrollement>(e => e.STUFK == enrollSubject.stdFK && e.SUBFK == enrollSubject.subFK);
					if (application != null)
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status409Conflict,
							Message = "Student Already Enrolled in this Subject"
						};
					else
						application = new Enrollement
						{
							STUFK = student.Id,
							SUBFK = subject.Id,
						};
					_Repository.CreateEntityAsync<Enrollement>(application);
					int res = await _uowServices.SaveChangesAsync();
					if (res > 0)
					{
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status200OK,
							Message = "Enrolled Successfully"
						};
					}
					else
					{
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status500InternalServerError,
							Message = "Enrollment Failed"
						};
					}
				}
				else
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status404NotFound,
						Message = "Subject Not Found"
					};
				}
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Something went wrong please try again"
				};
			}
		}
		public async Task<ResultDTO> viewLessons(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken)
		{
			if (await _Repository.GetEntityAsync<Enrollement>(e => e.STUFK == enrollSubject.stdFK && e.SUBFK == enrollSubject.subFK) is Enrollement enrollement)
			{
				return await _lessonServices.ViewLessons(enrollement.SUBFK, cancellationToken);
			}
			else
			{
				return new ResultDTO
				{
					Message = "You dont have access to these lessons",
					StatusCode = StatusCodes.Status403Forbidden
				};
			}
		}

		public async Task<ResultDTO> viewLesson(LessonContentDTO lessonDTO, CancellationToken cancellationToken)
		{
			//return null;
			return	await _lessonServices.viewLesson(lessonDTO, cancellationToken);
		}

		public async Task<ResultDTO> completeLesson(CompletelessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			//return null;
			return await _lessonServices.completeLesson(lessonDTO, cancellationToken);
		}
	}
}
