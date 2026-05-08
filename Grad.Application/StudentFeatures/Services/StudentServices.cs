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
		private readonly ILogger<StudentServices> _logger;

		public StudentServices(
			ISubjectServices subjectServices,
			IStudentRepository studentRepository,
			ILessonServices lessonServices,
			IlessonRepository lessonRepository,
			ISubmissionServices submissionServices,
			IExerciseServices exerciseServices,
			IUowServices uow,
			ILogger<StudentServices> logger)
		{
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_lessonServices = lessonServices ?? throw new ArgumentNullException(nameof(lessonServices));
			_studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
			_lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
			_submissionServices = submissionServices ?? throw new ArgumentNullException(nameof(submissionServices));
			_exerciseServices = exerciseServices ?? throw new ArgumentNullException(nameof(exerciseServices));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}


		public async Task<ResultDTO> ViewSubjects(Guid id, bool Enrolled, CancellationToken cancellationToken)
		{
			int disability = await _studentRepository.GetDisablityTypeAsync(id, cancellationToken) switch
			{
				DisablityType.Hearing => 2,
				DisablityType.Speech => 3,
				_ => 1
			};
			List<Enrollement> enrollements = await _studentRepository.GetEnrollementsAsync(id, cancellationToken);
			List<Guid> guids = enrollements.Select(e => e.SUBFK).ToList();
			if (Enrolled)
			{
				if (guids.Count == 0)
					return new ResultDTO
					{
						StatusCode = 200,
						Message = "No subjects yet 😊 Let’s add one and start learning!"
					};
				else
				{
					return await _subjectServices.ViewSubjectsAsync(guids: guids, disability: disability, cancellationToken: cancellationToken, enrollements: enrollements);
				}
			}
			else
			{
				return await _subjectServices.ViewSubjectsAsync(guids: guids, disability: disability, cancellationToken: cancellationToken);
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
			Enrollement enrollement = new Enrollement
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
			else if (await _studentRepository.IsEnrolled(enrollement.SUBFK, enrollement.STUFK, cancellationToken))
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
			Enrollement enrollement = await _studentRepository.GetEnrollementAsync(enrollSubject.subFK, enrollSubject.stdFK, cancellationToken);
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
							LessonContentDTO Nlesson = Nlid != Guid.Empty ? lessons[Nlid] : null;
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
			return await _lessonServices.viewLesson(lessonDTO, cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> ViewExerciseQuize(LevelDTO levelDTO, Guid stdID, CancellationToken cancellationToken)
		{
			if (await _studentRepository.IsEnrolled(levelDTO.Sid, stdID, cancellationToken))
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
			if (await _studentRepository.IsEnrolled(createSubmission.SubjectFK, createSubmission.SubmittedBy, cancellationToken))
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
			List<SubmissionDTO> submissionDTOs = await _submissionServices.GetSubmissions(stdID, cancellationToken);
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

			switch(result)
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
				case -3:
					return new ResultDTO
					{
						Message = "Lesson already marked as completed",
						StatusCode = 400
					};
				default:
					LessonContent lesson = await _lessonRepository.viewLesson(completelesson.Sid, completelesson.Lid, cancellationToken);
					return result == 0
					? new ResultDTO { Message = "Something went wrong", StatusCode = 500 }
					: new ResultDTO { Message = "Lesson marked as completed successfully", StatusCode = 200, result = new { lesson.NextType, lesson.Next } };
			}
		}


		private async Task<int> updatecompletion(CompletelessonDTO completelesson, CancellationToken cancellationToken)
		{
			Enrollement enrollement = await _studentRepository.GetEnrollementAsync(completelesson.Sid, completelesson.uid, cancellationToken);
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
			if (enrollement.studentProgresses?.FirstOrDefault
				(s => s.lid == completelesson.Lid) != null)
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

			_studentRepository.CreateEntityAsync(progress, cancellationToken);
			int result = await _uow.SaveChangesAsync();
			return result;
		}
	}
}

