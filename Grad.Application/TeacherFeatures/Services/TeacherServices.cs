using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Threading;
using Grad.Application.Common.DTOs;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.ExerciseFeatures.Interfaces;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.TeacherFeatures.DTOs;
using Grad.Application.TeacherFeatures.Interfaces;
using Grad.Application.Users.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
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

		public TeacherServices(ISubjectServices subjectServices, ITeacherRepository repository, ILessonServices lessonServices, IUserServices userServices, IExerciseServices exerciseServices, ILogger<TeacherServices> logger)
		{
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_teacherRepository = repository ?? throw new ArgumentNullException(nameof(repository));
			_lessonServices = lessonServices ?? throw new ArgumentNullException(nameof(lessonServices));
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_exerciseServices = exerciseServices ?? throw new ArgumentNullException(nameof(exerciseServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
			return await _subjectServices.ViewSubjectsAsync(Tid: teacherId, cancellationToken: cancellationToken);
		} //done

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
					Message = lessons.Any() ? "Lessons retrieved successfully" : "No result",
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
	}
}
