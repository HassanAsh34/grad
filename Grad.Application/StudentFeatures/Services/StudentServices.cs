using System.Diagnostics;
using System.Security.Cryptography;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.ExerciseFeatures.Interfaces;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Application.StudentFeatures.DTOs;
using Grad.Application.StudentFeatures.Interfaces;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.SubmissionFeatures.DTOs;
using Grad.Application.SubmissionFeatures.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Grad.Application.StudentFeatures.Services
{
	public class StudentServices : IStudentServices
	{
		private readonly ISubjectServices _subjectServices;
		private readonly IStudentRepository _studentRepository;
		private readonly ILessonServices _lessonServices;
		private readonly ISubmissionServices _submissionServices;
		private readonly IExerciseServices _exerciseServices;
		private readonly IlessonRepository _lessonRepository;
		private readonly IUowServices _uow;
		private readonly IRedisServices _redisServices;
		private readonly ILogger<StudentServices> _logger;
		private readonly IMemoryCache _memoryCache;

		public StudentServices(
			ISubjectServices subjectServices,
			IStudentRepository studentRepository,
			ILessonServices lessonServices,
			IlessonRepository lessonRepository,
			ISubmissionServices submissionServices,
			IExerciseServices exerciseServices,
			IUowServices uow,
			IRedisServices redisServices,
			ILogger<StudentServices> logger,
			IMemoryCache memoryCache)
		{
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_lessonServices = lessonServices ?? throw new ArgumentNullException(nameof(lessonServices));
			_studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
			_lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
			_submissionServices = submissionServices ?? throw new ArgumentNullException(nameof(submissionServices));
			_exerciseServices = exerciseServices ?? throw new ArgumentNullException(nameof(exerciseServices));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
			_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		}


		//public async Task<ResultDTO> ViewSubjects(Guid id, bool Enrolled, CancellationToken cancellationToken) //without loggers
		//{
		//	int disability = await _studentRepository.GetDisablityTypeAsync(id, cancellationToken) switch
		//	{
		//		DisablityType.Hearing => 2,
		//		DisablityType.Speech => 3,
		//		_ => 1
		//	};
		//	List<Enrollement> enrollements = await _studentRepository.GetEnrollementsAsync(id, cancellationToken);
		//	List<Guid> guids = enrollements.Select(e => e.SUBFK).ToList();
		//	if (Enrolled)
		//	{
		//		if (guids.Count == 0)
		//			return new ResultDTO
		//			{
		//				StatusCode = 200,
		//				Message = "No subjects yet 😊 Let’s add one and start learning!"
		//			};
		//		else
		//		{
		//			return await _subjectServices.ViewSubjectsAsync(guids: guids, disability: disability, cancellationToken: cancellationToken, enrollements: enrollements);
		//		}
		//	}
		//	else
		//	{
		//		return await _subjectServices.ViewSubjectsAsync(guids: guids, disability: disability, cancellationToken: cancellationToken);
		//	}
		//}

		//studentServices
		//public async Task<ResultDTO> ViewSubjects(Guid id, string Disability , bool Enrolled, CancellationToken cancellationToken)
		//{
		//	int disability = Disability switch
		//	{
		//		"Hearing" => 2,
		//		"Speech" => 3,
		//		_ => 1
		//	};
		//	// we will read enrollements out of redis
		//	string Key = $"enrollements_{id}";
		//	var sw = Stopwatch.StartNew();
		//	Dictionary<Guid, EnrollmentDTO> enrollements = await _redisServices.getDeserialized<Dictionary<Guid, EnrollmentDTO>>(Key);
		//	_logger.LogInformation("After enrollment Redis read: {ms}ms", sw.ElapsedMilliseconds);
		//	sw.Restart();
		//	if (enrollements == null)
		//	{
		//		List<Enrollement> enrollementList = await _studentRepository.GetEnrollementsAsync(id, cancellationToken);
		//		_logger.LogInformation("After enrollment DB read: {ms}ms", sw.ElapsedMilliseconds);
		//		sw.Restart();
		//		enrollements = enrollementList.ToDictionary(e => e.SUBFK, e => new EnrollmentDTO
		//		{
		//			SubjectId = e.SUBFK,
		//			Progress = e.Progress
		//		});
		//		if (enrollements != null)
		//		{
		//			if(!await _redisServices.storeSerialized(Key, enrollements, TimeSpan.FromHours(1)))
		//			{
		//				_logger.LogWarning("Failed to store enrollements in Redis for user {UserId}", id);
		//			}
		//		}
		//		else
		//		{
		//			enrollements = new Dictionary<Guid, EnrollmentDTO>();
		//		}
		//	}

		//	List<Guid> guids = enrollements.Keys.Count != null ? enrollements.Keys.ToList() : new List<Guid>();


		//	if (Enrolled)
		//	{
		//		if (enrollements.Keys.Count == 0)
		//			return new ResultDTO
		//			{
		//				StatusCode = 200,
		//				Message = "No subjects yet 😊 Let’s add one and start learning!"
		//			};
		//		else
		//		{
		//			return await _subjectServices.ViewSubjectsAsync(false, guids: guids, disability: disability, enrollements: enrollements, cancellationToken: cancellationToken);
		//		}
		//	}
		//	else
		//	{
		//		return await _subjectServices.ViewSubjectsAsync(false, guids: guids, disability: disability, cancellationToken: cancellationToken);
		//	}
		//}


		public async Task<ResultDTO> ViewSubjects(Guid id, string Disability, bool Enrolled, CancellationToken cancellationToken)
		{
			int disability = Disability switch
			{
				"Hearing" => 2,
				"Speech" => 3,
				_ => 1
			};
			// we will read enrollements out of redis
			Dictionary<Guid, EnrollmentDTO> enrollements = await getAllEnrollmentsCached(id,cancellationToken);
			List<Guid> guids = enrollements.Keys.Count != null ? enrollements.Keys.ToList() : new List<Guid>();
			if (Enrolled)
			{
				if (enrollements.Keys.Count == 0)
					return new ResultDTO
					{
						StatusCode = 200,
						Message = "No subjects yet 😊 Let's add one and start learning!"
					};
				else
				{
					var result = await _subjectServices.ViewSubjectsAsync(false, guids: guids, disability: disability, enrollements: enrollements, cancellationToken: cancellationToken);
					//_logger.LogInformation("After ViewSubjectsAsync (enrolled): {ms}ms", sw.ElapsedMilliseconds);
					return result;
				}
			}
			else
			{
				var result = await _subjectServices.ViewSubjectsAsync(false, guids: guids, disability: disability, cancellationToken: cancellationToken);
				//_logger.LogInformation("After ViewSubjectsAsync (not enrolled): {ms}ms", sw.ElapsedMilliseconds);
				return result;
			}
		}

		//public async Task<ResultDTO> viewSubject(Guid Sid,Guid stdid,CancellationToken cancellationToken)
		//{
		//	if(await _studentRepository.IsEnrolled(Sid, stdid, cancellationToken))
		//	{
		//		ResultDTO res = await _subjectServices.ViewSubjectAsync(Sid, false, cancellationToken);
		//		if (res.StatusCode == 200)
		//		{

		//			SubjectDTO subjectDTO = (SubjectDTO)res.result;
		//			if (subjectDTO != null)
		//			{
		//				subjectDTO.progress = subjectDTO.lessonsCount > 0 ? (enrollement.studentProgresses.Count() / (float)subjectDTO.lessonsCount) * 100 : 0;
		//			}
		//			res.result = subjectDTO;
		//			return res;
		//		}
		//		else
		//			return res;
		//		}
		//	}
		//}

		public async Task<ResultDTO> EnrollSubject(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken)
		{
			Enrollment enrollement = new Enrollment
			{
				STUFK = enrollSubject.stdFK,
				SUBFK = enrollSubject.subFK
			};
			if (!await _subjectServices.IsSubjectExist(subjectId: enrollement.SUBFK, cancellationToken: cancellationToken))
			{
				return new ResultDTO
				{
					Message = "Subject wasnt found",
					StatusCode = 404
				};
			}
			//else if (await _studentRepository.IsEnrolled(enrollement.SUBFK, enrollement.STUFK, cancellationToken))
			else if(await IsEnrolled(enrollement.SUBFK, enrollement.STUFK, cancellationToken))
			{
				return new ResultDTO
				{
					StatusCode = 409,
					Message = "You are already enrolled to that subject"
				};
			}
			else
			{
				int res = await _studentRepository.EnrollSubject(enrollement, cancellationToken);
				if (res > 0)
				{
					await bustCachedEnrollments(enrollement.STUFK, cancellationToken);
					return new ResultDTO
					{
						StatusCode = 200,
						Message = "Enrolled Successfully"
					};
				}
				else
				{
					return new ResultDTO
					{
						StatusCode = 500,
						Message = "Enrollment Failed"
					};
				}
			}
		}

		public async Task<ResultDTO> viewLessons(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken)//Enhanced
		{
			Enrollment enrollement = await _studentRepository.GetEnrollementAsync(enrollSubject.subFK, enrollSubject.stdFK, cancellationToken);
			if (enrollement != null)
			{
				Dictionary<Guid, LessonContentDTO> lessons = (await _lessonServices.ViewLessons(enrollSubject.subFK, cancellationToken)).ToDictionary(l => l.Id, l => l);
				if (lessons.Count == 0)
				{
					return new ResultDTO
					{
						Message = "No lessons yet 😊",
						StatusCode = 200
					};
				}
				else
				{
					List<Guid> submittedLessons = enrollement.studentProgresses.Select(e => e.lid).ToList();
					foreach (Guid Lid in submittedLessons)
					{
						LessonContentDTO lesson = lessons.TryGetValue(Lid, out var l) ? l : null;
						if (lesson != null)
						{
							Guid Nlid = lesson.Nlid ?? Guid.Empty;
							LessonContentDTO Nlesson = lessons.TryGetValue(Nlid, out var nl) ? nl : null;
							if (Nlesson != null)
							{
								Nlesson.locked = false;
								lessons[Nlid] = Nlesson;
							}
						}
					}
					return new ResultDTO
					{
						Message = "Lessons retrieved successfully",
						StatusCode = 200,
						result = lessons.Values.ToList()
					};
				}
			}
			else
			{
				return new ResultDTO
				{
					Message = "You dont have access to these lessons",
					StatusCode = 403
				};
			}
		}

		public async Task<ResultDTO> viewLesson(LessonContentDTO lessonDTO, CancellationToken cancellationToken)
		{
			//return null;
			var completed = await _studentRepository.GetEntityAsync<StudentProgress>(s => s.lid == lessonDTO.Id, cancellationToken);
			if (completed != null)
			{
				return await _lessonServices.viewLesson(lessonDTO,true, cancellationToken);
			}
			else
				return await _lessonServices.viewLesson(lessonDTO,false,cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> ViewExerciseQuize(LevelDTO levelDTO, Guid stdID, CancellationToken cancellationToken)
		{
			if (await IsEnrolled(levelDTO.Sid, stdID, cancellationToken))
			{
				return await _exerciseServices.viewLevel(levelDTO, false, cancellationToken);
			}
			else
				return new ResultDTO
				{
					Message = "Restricted access",
					StatusCode = 403
				};
		}

		public async Task<ResultDTO> createSubmission(CreateSubmissionDTO createSubmission, CancellationToken cancellationToken)
		{
			if (await IsEnrolled(createSubmission.SubjectFK, createSubmission.SubmittedBy, cancellationToken))
			{
				ResultDTO res = await _submissionServices.createSubmission(createSubmission, cancellationToken);
				if (res.StatusCode == 200)
				{
					if (res.result is Grade grade && grade.Passed && createSubmission.LessonID != null)
					{
						switch(await updatecompletion(new CompletelessonDTO
						{
							Lid = createSubmission.LessonID.Value,
							Sid = createSubmission.SubjectFK,
							uid = createSubmission.SubmittedBy
						}, cancellationToken))
						{
							case -1:
								_logger.LogWarning("User {UserId} is not enrolled in subject {SubjectId}", createSubmission.SubmittedBy, createSubmission.SubjectFK);
								break;
							case -2:
								_logger.LogWarning("Lesson {LessonId} not found for subject {SubjectId}", createSubmission.LessonID, createSubmission.SubjectFK);
								break;
							case -3:
								_logger.LogInformation("Lesson {LessonId} already marked as completed for user {UserId}", createSubmission.LessonID, createSubmission.SubmittedBy);
								break;
							default:
								_logger.LogInformation("Lesson {LessonId} marked as completed successfully for user {UserId}", createSubmission.LessonID, createSubmission.SubmittedBy);
								break;
						}
						if(createSubmission.LessonID != null)
						{
							LessonContent lessonContent = await _lessonRepository.viewLesson(createSubmission.SubjectFK, (Guid)createSubmission.LessonID, cancellationToken);
							grade.NextType = lessonContent.NextType;
							grade.NextId = lessonContent.Next;
						}
					}
				}
				return res;
			}
			else
			{
				return new ResultDTO
				{
					Message = "You dont have access to submit to this subject",
					StatusCode = 403
				};
			}
		}

		public async Task<ResultDTO> viewSubmissions(Guid stdID, CancellationToken cancellationToken)
		{
			List<SubmissionDTO> submissionDTOs = await _submissionServices.GetSubmissions(stdID,null,false,cancellationToken);
			return new ResultDTO
			{
				Message = "Submissions retrieved successfully",
				StatusCode = 200,
				result = submissionDTOs
			};
		}

		public async Task<ResultDTO> completeLesson(CompletelessonDTO completelesson, CancellationToken cancellationToken)
		{
			// Step 1: Verify lesson exists via IlessonRepository (Option A — no service-to-service call)
			int result = await updatecompletion(completelesson, cancellationToken);

			switch (result)
			{
				case -1:
					return new ResultDTO
					{
						Message = "You are not enrolled in this subject",
						StatusCode = 403
					};
				case -2:
					return new ResultDTO
					{
						Message = "Lesson not found",
						StatusCode = 404
					};
				default:
					LessonContent lesson = await _lessonRepository.viewLesson(completelesson.Sid, completelesson.Lid, cancellationToken);
					if (result == 0)
					{
						_logger.LogCritical("Failed to save changes to the database when marking lesson {LessonId} as completed for user {UserId}", completelesson.Lid, completelesson.uid);
						return new ResultDTO { Message = "Something went wrong", StatusCode = 500 };
					}
					else
					{
						await bustCachedEnrollments(completelesson.uid, cancellationToken);
						_logger.LogInformation("Lesson {LessonId} marked as completed successfully for user {UserId}", completelesson.Lid, completelesson.uid);
						return result == -3 ? new ResultDTO { Message = "Lesson already marked as completed", StatusCode = 200, result = new { lesson.NextType, lesson.Next } } : new ResultDTO { Message = "Lesson marked as completed successfully", StatusCode = 200, result = new { lesson.NextType, lesson.Next } };
					} 
			}
		}


		private async Task<int> updatecompletion(CompletelessonDTO completelesson, CancellationToken cancellationToken)//===============need to be fixed
		{
			Enrollment enrollement = await _studentRepository.GetEnrollementAsync(completelesson.Sid, completelesson.uid, cancellationToken);
			if (enrollement == null)
			{
				return -1;
			}
			LessonContent lesson = await _lessonRepository.viewLesson(enrollement.SUBFK, completelesson.Lid, cancellationToken);
			if (lesson == null)
			{
				return -2;
			}

			// Step 3: Check if already completed

			if (enrollement.studentProgresses?.FirstOrDefault(p=>p.lid == completelesson.Lid) != null)
			{
				return -3;
			}


			// Step 4: Create progress record
			StudentProgress progress = new StudentProgress
			{
				lid = lesson.Id,
				Eid_fk = enrollement.Id,
				Completed_At = DateTime.UtcNow
			};
			_studentRepository.CreateEntityAsync<StudentProgress>(progress, cancellationToken);


			int count = enrollement.studentProgresses?.Count ?? 0;
			enrollement.Progress =  count + 1;
			_studentRepository.UpdateEntityAsync(enrollement, cancellationToken);
			int result = await _uow.SaveChangesAsync();
			return result;
		}

		private async Task<bool> IsEnrolled(Guid Sid, Guid StdId, CancellationToken CT)
		{
			Dictionary<Guid, EnrollmentDTO> enrollements = await getAllEnrollmentsCached(StdId, CT);
			return enrollements.TryGetValue(Sid, out EnrollmentDTO e);
		}

		private async Task bustCachedEnrollments(Guid Sid, CancellationToken cancellationToken = default)
		{
			string Key = $"enrollements_{Sid}";
			_memoryCache.Remove(Key);                  // clear L1
			await _redisServices.delete(Key);          // clear L2
		}

		private async Task<Dictionary<Guid, EnrollmentDTO>> getAllEnrollmentsCached(Guid Sid, CancellationToken cancellationToken = default)
		{
			string Key = $"enrollements_{Sid}";
			var sw = Stopwatch.StartNew();

			// L1 — MemoryCache, nanoseconds, no network
			if (_memoryCache.TryGetValue(Key, out Dictionary<Guid, EnrollmentDTO> enrollements))
			{
				_logger.LogInformation("After enrollment MemoryCache read: {ms}ms", sw.ElapsedMilliseconds);
				return enrollements;
			}

			// L2 — Redis
			enrollements = await _redisServices.getDeserialized<Dictionary<Guid, EnrollmentDTO>>(Key);
			_logger.LogInformation("After enrollment Redis read: {ms}ms", sw.ElapsedMilliseconds);
			sw.Restart();

			if (enrollements != null)
			{
				// warm L1 from Redis
				_memoryCache.Set(Key, enrollements, TimeSpan.FromMinutes(2));
				return enrollements;
			}

			// L3 — DB
			List<Enrollment> enrollementList = await _studentRepository.GetEnrollementsAsync(Sid, cancellationToken);
			_logger.LogInformation("After enrollment DB read: {ms}ms", sw.ElapsedMilliseconds);
			sw.Restart();

			enrollements = enrollementList.ToDictionary(e => e.SUBFK, e => new EnrollmentDTO
			{
				SubjectId = e.SUBFK,
				Progress = e.Progress
			});

			if (enrollements != null)
			{
				if (!await _redisServices.storeSerialized(Key, enrollements, TimeSpan.FromHours(1)))
				{
					_logger.LogWarning("Failed to store enrollements in Redis for user {UserId}", Sid);
				}
				_logger.LogInformation("After enrollment Redis store: {ms}ms", sw.ElapsedMilliseconds);
				sw.Restart();

				// warm L1
				_memoryCache.Set(Key, enrollements, TimeSpan.FromMinutes(2));
			}
			else
			{
				enrollements = new Dictionary<Guid, EnrollmentDTO>();
			}

			return enrollements;
		}

		//private async Task<Dictionary<Guid, EnrollmentDTO>> getAllEnrollmentsCached(Guid Sid,CancellationToken cancellationToken = default)
		//{
		//	string Key = $"enrollements_{Sid}";
		//	var sw = Stopwatch.StartNew();
		//	Dictionary<Guid, EnrollmentDTO> enrollements = await _redisServices.getDeserialized<Dictionary<Guid, EnrollmentDTO>>(Key);
		//	_logger.LogInformation("After enrollment Redis read: {ms}ms", sw.ElapsedMilliseconds);
		//	sw.Restart();
		//	if (enrollements == null)
		//	{
		//		List<Enrollment> enrollementList = await _studentRepository.GetEnrollementsAsync(Sid, cancellationToken);
		//		_logger.LogInformation("After enrollment DB read: {ms}ms", sw.ElapsedMilliseconds);
		//		sw.Restart();
		//		enrollements = enrollementList.ToDictionary(e => e.SUBFK, e => new EnrollmentDTO
		//		{
		//			SubjectId = e.SUBFK,
		//			Progress = e.Progress
		//		});
		//		if (enrollements != null)
		//		{
		//			if (!await _redisServices.storeSerialized(Key, enrollements, TimeSpan.FromHours(1)))
		//			{
		//				_logger.LogWarning("Failed to store enrollements in Redis for user {UserId}", Sid);
		//			}
		//			_logger.LogInformation("After enrollment Redis store: {ms}ms", sw.ElapsedMilliseconds);
		//			sw.Restart();
		//		}
		//		else
		//		{
		//			enrollements = new Dictionary<Guid, EnrollmentDTO>();
		//		}
		//	}
		//	return enrollements;
		//}



		//private async Task bustCachedEnrollments(Guid Sid,CancellationToken cancellationToken = default)
		//{
		//	string Key = $"enrollements_{Sid}";
		//	await _redisServices.delete(Key);
		//}

		//inquery for student need to be implemented
	}
}

