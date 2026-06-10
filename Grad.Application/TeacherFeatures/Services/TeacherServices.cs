using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Threading;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.ExerciseFeatures.Interfaces;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Application.QAFeature.DTO;
using Grad.Application.QAFeature.Interfaces;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.SubmissionFeatures.DTOs;
using Grad.Application.SubmissionFeatures.Interfaces;
using Grad.Application.TeacherFeatures.DTOs;
using Grad.Application.TeacherFeatures.Interfaces;
using Grad.Application.Users.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace Grad.Application.TeacherFeatures.Services
{
	public class TeacherServices : ITeacherServices
	{
		private readonly ISubjectServices _subjectServices;

		private readonly ILessonServices _lessonServices;

		private readonly ITeacherRepository _teacherRepository;

		private readonly IUserServices _userServices;

		private readonly IExerciseServices _exerciseServices;

		private readonly ILogger<TeacherServices> _logger;

		private readonly IRedisServices _redisServices;

		private readonly ICummunicationServices _inqueryServices; //replace it with interface

		private readonly ICloudinaryServices _cloudinaryServices;

		private readonly IUowServices _uowServices;

		private readonly ISubmissionServices _submissionServices;

		public TeacherServices(ISubjectServices subjectServices, ITeacherRepository repository, ILessonServices lessonServices, IUserServices userServices, IExerciseServices exerciseServices, ILogger<TeacherServices> logger, IRedisServices redisServices,ICummunicationServices cummunicationServices, ICloudinaryServices cloudinaryServices,IUowServices uowServices, ISubmissionServices submissionServices)
		{
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_teacherRepository = repository ?? throw new ArgumentNullException(nameof(repository));
			_lessonServices = lessonServices ?? throw new ArgumentNullException(nameof(lessonServices));
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_exerciseServices = exerciseServices ?? throw new ArgumentNullException(nameof(exerciseServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
			_inqueryServices = cummunicationServices ?? throw new ArgumentNullException(nameof(cummunicationServices));
			_cloudinaryServices	= cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));	
			_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));
			_submissionServices = submissionServices ?? throw new ArgumentNullException(nameof(submissionServices));
        }

		public async Task<ResultDTO> ShowStudents(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(teacherSubject.TeacherId, teacherSubject.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				IEnumerable<Student> students = await _teacherRepository.showStudents(teacherSubject.SubjectId, cancellationToken);
				IEnumerable<LISTStudentDTO> dtos = students.Select(e => new LISTStudentDTO
				{
					Id = e.Id,
					name = $"{e.FName} {e.LName}",
					email = e.EmailorUserName,
					parent = e.PID != null ? true : false
				}).ToList();

				int stdCount = students.Count();

				return new ResultDTO
				{
					Message = $"{stdCount} students were found",
					StatusCode = stdCount > 0 ? 200 : 204,
					result = dtos
				};
			}

		}

		public async Task<ResultDTO> ViewStudent(ProfileDTO profile, CancellationToken cancellationToken)
		{
			return await _userServices.ViewProfile(profile, cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> ViewSubjects(Guid teacherId, CancellationToken cancellationToken)
		{
			List<Guid> AssignedSubjectIDs = await _teacherRepository.GetAssignedSubjectIDs(teacherId, cancellationToken);
			if (AssignedSubjectIDs.Count > 0)
			{
				return await _subjectServices.ViewSubjectsAsync(true,AssignedSubjectIDs, cancellationToken: cancellationToken);
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = 404,
					Message = "You Are not assigned to any subjects at the moment"
				};
			}
		} //done

		public async Task<ResultDTO> viewStudentProgress(Guid stdID,TeacherSubjectDTO teacherSubjectDTO,CancellationToken cancellationToken)
		{
            if (!await _teacherRepository.CanAccess(teacherSubjectDTO.TeacherId,teacherSubjectDTO.SubjectId, cancellationToken))
                return new ResultDTO
                {
                    StatusCode = 400,
                    Message = "Subject wasnt found"
                };
            else
			{
                List<SubmissionDTO> submissionDTOs = await _submissionServices.GetSubmissions(stdID, teacherSubjectDTO.SubjectId,false, cancellationToken);
				
				return new ResultDTO
				{
					Message = submissionDTOs != null ? submissionDTOs.Count != 0 ? "Progress retrieved successfully" : "No result" : "Either Student or Subject doesnt exist",
					StatusCode = submissionDTOs != null ? submissionDTOs.Count != 0 ? 200 : 404 : 400,
					result = submissionDTOs
				};
            }
        }

        public async Task<ResultDTO> viewStudentsProgress(TeacherSubjectDTO teacherSubjectDTO, CancellationToken cancellationToken)
        {
            if (!await _teacherRepository.CanAccess(teacherSubjectDTO.TeacherId, teacherSubjectDTO.SubjectId, cancellationToken))
                return new ResultDTO
                {
                    StatusCode = 400,
                    Message = "Subject wasnt found"
                };
            else
            {
                List<SubmissionDTO> submissionDTOs = await _submissionServices.GetSubmissions(Sid: teacherSubjectDTO.SubjectId,teacher: true,cancellationToken: cancellationToken);

                return new ResultDTO
                {
                    Message = submissionDTOs != null ? submissionDTOs.Count != 0 ? "Progress retrieved successfully" : "No result" : "invalid subject id",
                    StatusCode = submissionDTOs != null ? submissionDTOs.Count != 0 ? 200 : 404 : 400,
                    result = submissionDTOs
                };
            }
        }

        public async Task<ResultDTO> ViewSubject(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(teacherSubject.TeacherId, teacherSubject.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
				return await _subjectServices.ViewSubjectAsync(teacherSubject.SubjectId, cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> ViewLessons(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(teacherSubject.TeacherId, teacherSubject.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				List<LessonContentDTO> lessons = await _lessonServices.ViewLessons(teacherSubject.SubjectId, cancellation: cancellationToken);
				return new ResultDTO
				{
					Message = lessons.Count != 0 ? "Lessons retrieved successfully" : "No result",
					StatusCode = lessons.Any() ? 200 : 404,
					result = lessons
				};
			}
		}

		public async Task<ResultDTO> ViewLesson(LessonContentDTO lessonContent, CancellationToken cancellationToken)
		{
			//AssignedSubject assignedSubject = await _teacherRepository.GetEntityAsync<AssignedSubject>(a => a.SubjectId == teacherSubject.SubjectId && a.TeacherId == teacherSubject.TeacherId, cancellationToken: cancellationToken);
			//if (assignedSubject == null)
			//	return new ResultDTO
			//	{
			//		StatusCode = StatusCodes.Status404NotFound,
			//		Message = "Subject wasnt found"
			//	};
			//else
			//return await _lessonServices.viewLesson(new LessonContentDTO { Id = lessonId,SubjectId = teacherSubject.SubjectId}, cancellationToken);
			return await _lessonServices.viewLesson(lessonContent, true, cancellationToken);
		}

		public async Task<ResultDTO> UploadVideo(VideoDTO video, CancellationToken cancellationToken)
		{
			return await _lessonServices.UploadVideo(video, cancellationToken);
		}

		public async Task<ResultDTO> AddLesson(AddLessonDTO lesson, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(lesson.UId, lesson.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
				return await _lessonServices.AddLesson(lesson, cancellationToken);
		}


		public async Task<ResultDTO> EditLesson(EditLessonDTO lesson, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(lesson.UId, lesson.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
				return await _lessonServices.editLesson(lesson, cancellationToken);
		}

		public async Task<ResultDTO> DeleteLesson(DeleteLessonDTO lesson, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(lesson.UId, lesson.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
				return await _lessonServices.DeleteLesson(lesson, cancellationToken);
		}

		public async Task<ResultDTO> addWords(AddVocabDTO vocabDTO, CancellationToken cancellationToken) // needs enhancements
		{
			if (!await _teacherRepository.CanAccess(vocabDTO.Tid, vocabDTO.sid, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				return await _subjectServices.addwords(vocabDTO, cancellationToken);
			}
		}



		public async Task<ResultDTO> CreateExercise(CreateLevelDTO createExerciseDTO, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(createExerciseDTO.Tid, createExerciseDTO.Sid, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				return await _exerciseServices.CreateExercise(createExerciseDTO, cancellationToken);
			}
		}

		public async Task<ResultDTO> GetQuizes(TeacherSubjectDTO teacherSubject, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(teacherSubject.TeacherId, teacherSubject.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				List<LevelDTO> levels = await _exerciseServices.GetQuizes(teacherSubject.SubjectId, cancellationToken);
				return new ResultDTO
				{
					Message = levels.Any() ? "Quizes retrieved successfully" : "No result",
					StatusCode = levels.Any() ? 200 : 404,
					result = levels
				};
			}
		}

		public async Task<ResultDTO> ListPerquisites(TeacherSubjectDTO teacherSubject, PerquisiteType type, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(teacherSubject.TeacherId, teacherSubject.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 403,
					Message = "Restricted Access"
				};
			else
			{
				List<Perquisite> perquisites = new();
				switch (type)
				{
					case PerquisiteType.Lesson:
						List<LessonContentDTO> lesson = await _lessonServices.ViewLessons(teacherSubject.SubjectId, cancellationToken);
						perquisites = lesson.Select(l => new Perquisite
						{
							id = l.Id,
							name = l.Title,
						}).ToList();
						return new ResultDTO
						{
							Message = perquisites.Any() ? "Perquisites retrieved successfully" : "No result",
							StatusCode = perquisites.Any() ? 200 : 404,
							result = perquisites
						};
					case PerquisiteType.Quiz:
						List<LevelDTO> levels = await _exerciseServices.GetQuizes(teacherSubject.SubjectId, cancellationToken);
						perquisites = levels.Select(l => new Perquisite
						{
							id = l.ID,
							name = l.Name,
						}).ToList();
						return new ResultDTO
						{
							Message = perquisites.Any() ? "Perquisites retrieved successfully" : "No result",
							StatusCode = perquisites.Any() ? 200 : 404,
							result = perquisites
						};
					default:
						return new ResultDTO
						{
							StatusCode = 400,
							Message = "Invalid perquisite type"
						};
				}
			}
		}


		public async Task<ResultDTO> ViewLevel(LevelDTO level, Guid Tid, CancellationToken CT)
		{
			if (!await _teacherRepository.CanAccess(Tid, level.Sid, CT))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				return await _exerciseServices.viewLevel(level, true, CT);
			}
		}

		public async Task<ResultDTO> EditLevel(EditLevelDTO editLevel, CancellationToken CT)
		{
			if (!await _teacherRepository.CanAccess(editLevel.Tid, editLevel.Sid, CT))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				return await _exerciseServices.EditLevel(editLevel, CT);
			}
		}

		public async Task<ResultDTO> DeleteLevel(LevelDTO level, Guid Tid, CancellationToken CT)
		{
			if (!await _teacherRepository.CanAccess(Tid, level.Sid, CT))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				return await _exerciseServices.DeleteLevel(level, CT);
			}
		}

		public async Task<ResultDTO> DeleteVideo(VideoDTO videoDTO, Guid Tid, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(Tid, videoDTO.subjectID, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				return await _lessonServices.DeleteVideo(videoDTO, cancellationToken);
			}
		}

		public async Task<ResultDTO> listInqueries(Guid TId,Guid Sid,CancellationToken CT)
		{
			if (!await _teacherRepository.CanAccess(TId, Sid, CT))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				IEnumerable<InqueryDTO> inqueries = await _inqueryServices.GetInqueries(Sid, false, true, CT);
				if (inqueries.Count() > 0)
					return new ResultDTO
					{
						StatusCode = 200,
						Message = $"{inqueries.Count()} inqueries were found",
						result = inqueries
					};
				else
					return new ResultDTO
					{
						StatusCode = 400,
						Message = "there are no inqueries at the moment"
					};
			}
		}

		public async Task<ResultDTO> viewInquery(Guid Id, Guid TId, CancellationToken cancellationToken)
		{
			InqueryDTO inquery = await _inqueryServices.ViewInquery(Id, cancellationToken);
			if (inquery == null)
				return new ResultDTO
				{
					StatusCode = 404,
					Message = "Inquery wasnt found"
				};
			else
			{
				await _redisServices.store(Id.ToString(), TId.ToString(), TimeSpan.FromMinutes(50));
				return new ResultDTO
				{
					StatusCode = 200,
					Message = "Inquery retrieved successfully",
					result = inquery
				};
			}
		}

		public async Task<ResultDTO> createInquery(InqueryDTO inquery, Guid TId, CancellationToken cancellationToken)
		{
			if(inquery.RepliedToId != Guid.Empty || inquery.RepliedToId !=  null)
			{
				string lockedBy = await _redisServices.get(inquery.RepliedToId.ToString());
				if (lockedBy != null && lockedBy != TId.ToString())
				{
					return new ResultDTO
					{
						StatusCode = 409,
						Message = "Inquery is currently being viewed or edited by another user"
					};
				}
			}
			InqueryStatus? status  = inquery.RepliedToId == null || inquery.RepliedToId == Guid.Empty ? null : InqueryStatus.Solved;
			if (await _inqueryServices.createInquery(inquery,status,cancellationToken))
			{
				return new ResultDTO
				{
					StatusCode = 201,
					Message = "Inquery created successfully"
				};
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = 500,
					Message = "Failed to create inquery"
				};
			}
		}

		public async Task<ResultDTO> uploadCV(CVDTO cv, CancellationToken cancellationToken)
		{
			if(cv.CV != null)
			{
				Teacher teacher = await _teacherRepository.GetEntityAsync<Teacher>(t=>t.Id == cv.Tid , cancellationToken);
				if(teacher == null)
				{
					return new ResultDTO
					{
						StatusCode = 401,
						Message = "Something went wrong"
					};
				}
				string folder = $"teacher/{teacher.EmailorUserName}/";
				string cvPath   = await _cloudinaryServices.UploadCV(cv.CV, folder, $"{teacher.EmailorUserName}_cv", cancellationToken);
				if(string.IsNullOrEmpty(cvPath))
					return new ResultDTO
					{
						StatusCode = 500,
						Message = "Error uploading CV"
					};
				else
				{
					teacher.cvPath = cvPath;
					int res = await _uowServices.SaveChangesAsync();
					return new ResultDTO
					{
						Message = res > 0 ? "Cv was Updated Successfully" : "Failed to update CV",
						StatusCode = res > 0 ? 200 : 500
					};
				}
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "No CV file provided"
				};
			}
		}

		public async Task<ResultDTO> ViewDictionary(TeacherSubjectDTO teacherSubjectDTO, CancellationToken cancellationToken)
		{
			if (!await _teacherRepository.CanAccess(teacherSubjectDTO.TeacherId, teacherSubjectDTO.SubjectId, cancellationToken))
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Subject wasnt found"
				};
			else
			{
				return await _subjectServices.ViewDictionary(teacherSubjectDTO.SubjectId, cancellationToken);
			}
		}

		//public async Task<ResultDTO> ViewCV(CVDTO cv,CancellationToken cancellationToken)
		//{
		//	Teacher teacher = await _teacherRepository.GetEntityAsync<Teacher>(t => t.Id == cv.Tid, cancellationToken);
		//	if (teacher == null)
		//	{
		//		return new ResultDTO
		//		{
		//			StatusCode = 401,
		//			Message = "Something went wrong"
		//		};
		//	}
		//	else
		//	{
		//		cv.cvPath = teacher.cvPath;
		//		return new ResultDTO
		//		{
		//			StatusCode = 200,
		//			Message = "CV retrieved successfully",
		//			result = cv
		//		};
		//	}
		//}
	}
}
