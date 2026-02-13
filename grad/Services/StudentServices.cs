using grad.Data;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace grad.Services
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
					Student.DisablityType.Hearing => 2,
					Student.DisablityType.Speech => 3,
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
						return await _subjectServices.ViewSubjectsAsync(guids: guids, disability, Enrolled, cancellationToken: cancellationToken);
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

		public async Task<ResultDTO> viewSubject(Guid Sid,CancellationToken cancellationToken)
		{
			return await _subjectServices.ViewSubjectAsync(Sid,false,cancellationToken);
		}

		public async Task<ResultDTO> EnrollSubject(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken)
		{
			//enrollSubject.stdFK = enrollSubject.stdFK.Trim();
			//enrollSubject.subFK = enrollSubject.subFK.Trim();
			if (await _Repository.GetEntityAsync<Enrollement>(e => e.STUFK == enrollSubject.stdFK && e.SUBFK == enrollSubject.subFK) is Enrollement)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status409Conflict,
					Message = "Student Already Enrolled in this Subject"
				};
			}
			else if(await _Repository.GetEntityAsync<Student>(s => s.Id == enrollSubject.stdFK) is Student student)
			{
				if(await _Repository.GetEntityAsync<Subject>(s => s.Id == enrollSubject.subFK) is Subject subject)
				{
					Enrollement application = new Enrollement
					{
						STUFK = student.Id,
						SUBFK = subject.Id,
					};
					_Repository.CreateEntityAsync<Enrollement>(application);
					int res = await _uowServices.SaveChangesAsync();
					if(res>0)
					{
						StudentProgress studentProgress = new StudentProgress
						{
							Id = application.Id,
							CompletedLessonIds = new List<string>(),
							ProgressPercent = 0
						};
						try
						{
							await _studentProgress.InsertOneAsync(studentProgress, cancellationToken: cancellationToken);
						}
						catch (Exception ex)
						{
							_Repository.DeleteEntityAsync<Enrollement>(application);
							await _uowServices.SaveChangesAsync();
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status500InternalServerError,
								Message = "Enrollment Failed: " + ex.Message
							};
						}
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
					StatusCode = StatusCodes.Status404NotFound,
					Message = "Student Not Found"
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

		public async Task<ResultDTO> viewLesson(LessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			//return null;
			return	await _lessonServices.viewLesson(lessonDTO, cancellationToken);
		}
	}
}
