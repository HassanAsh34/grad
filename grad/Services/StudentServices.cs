using grad.Data;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using MongoDB.Driver;

namespace grad.Services
{
	public class StudentServices : IStudentServices
	{
		private readonly ISubjectServices _subjectServices;
		private readonly IRepository _Repository;
		private readonly IMongoCollection<StudentProgress> _studentProgress;
		private readonly IUowServices _uowServices;

		public StudentServices(ISubjectServices subjectServices,IRepository repository,IUowServices uow, MongoDBContext context)
		{
			_subjectServices = subjectServices ??  throw new ArgumentNullException(nameof(subjectServices));
			_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_uowServices = uow ?? throw new ArgumentNullException(nameof(uow));
			_studentProgress = context.StudentProgress ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<ResultDTO> ViewSubjects(Guid id, CancellationToken cancellationToken)
		{
			Student student = await _Repository.GetEntityAsync<Student>(s => s.Id == id);
			if(student != null)
			{
				switch(student.Disability)
				{
					case Student.DisablityType.Hearing:
						return await _subjectServices.ViewSubjectsAsync(2, cancellationToken);
					case Student.DisablityType.Speech:
						return await _subjectServices.ViewSubjectsAsync(3, cancellationToken);
					default:
						return await _subjectServices.ViewSubjectsAsync(1, cancellationToken);
				}
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
			return await _subjectServices.ViewSubjectAsync(Sid, cancellationToken);
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
	}
}
