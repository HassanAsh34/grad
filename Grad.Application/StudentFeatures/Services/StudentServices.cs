using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Application.StudentFeatures.DTOs;
using Grad.Application.StudentFeatures.Interfaces;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.StudentFeatures.Services
{
	public class StudentServices : IStudentServices
	{
		private readonly ISubjectServices _subjectServices;
		private readonly IStudentRepository _studentRepository;
		private readonly ILessonServices _lessonServices;
		private readonly IlessonRepository _lessonRepository;
		private readonly IUowServices _uow;

		public StudentServices(
			ISubjectServices subjectServices,
			IStudentRepository studentRepository,
			ILessonServices lessonServices,
			IlessonRepository lessonRepository,
			IUowServices uow)
		{
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_lessonServices = lessonServices ?? throw new ArgumentNullException(nameof(lessonServices));
			_studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
			_lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}


		public async Task<ResultDTO> ViewSubjects(Guid id,bool Enrolled,CancellationToken cancellationToken)
		{
			int disability = await _studentRepository.GetDisablityTypeAsync(id,cancellationToken) switch
			{
				DisablityType.Hearing => 2,
				DisablityType.Speech => 3,
				_ => 1
			};
			List<Enrollement> enrollements = await _studentRepository.GetEnrollementsAsync(id, cancellationToken);
			List<Guid> guids = enrollements.Select(e=>e.SUBFK).ToList();
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
					return await _subjectServices.ViewSubjectsAsync(guids: guids, disability: disability, cancellationToken: cancellationToken,enrollements: enrollements);
				}
			}
			else
			{
				return await _subjectServices.ViewSubjectsAsync(guids: guids,disability: disability, cancellationToken: cancellationToken);
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
			else if(await _studentRepository.IsEnrolled(enrollement.SUBFK,enrollement.STUFK,cancellationToken))
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
		
		public async Task<ResultDTO> viewLessons(EnrollSubjectDTO enrollSubject, CancellationToken cancellationToken)
		{
			if (await _studentRepository.IsEnrolled(enrollSubject.subFK,enrollSubject.stdFK,cancellationToken))
			{
				return await _lessonServices.ViewLessons(enrollSubject.subFK, cancellationToken);
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
			return	await _lessonServices.viewLesson(lessonDTO, cancellationToken);
		}

		public async Task<ResultDTO> completeLesson(CompletelessonDTO completelesson, CancellationToken cancellationToken)
		{
			// Step 1: Verify lesson exists via IlessonRepository (Option A — no service-to-service call)
			LessonContent lesson = await _lessonRepository.viewLesson(completelesson.Sid, completelesson.Lid, cancellationToken);
			if (lesson == null)
			{
				return new ResultDTO
				{
					Message = "Lesson not found",
					StatusCode = 404
				};
			}

			// Step 2: Fetch the student's enrollment (includes studentProgresses navigation property)
			List<Enrollement> enrollements = await _studentRepository.GetEnrollementsAsync(completelesson.uid, cancellationToken);
			Enrollement enrollement = enrollements.FirstOrDefault(e => e.SUBFK == completelesson.Sid);

			if (enrollement == null)
			{
				return new ResultDTO
				{
					Message = "You are not enrolled in this subject",
					StatusCode = 403
				};
			}

			// Step 3: Check if already completed
			if (enrollement.studentProgresses?.FirstOrDefault(s => s.lid == completelesson.Lid) != null)
			{
				return new ResultDTO
				{
					Message = "Lesson already marked as completed",
					StatusCode = 400
				};
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

			return result == 0
				? new ResultDTO { Message = "Something went wrong", StatusCode = 500 }
				: new ResultDTO { Message = "Lesson marked as completed successfully", StatusCode = 200 };
		}
	}
}

