using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.Interfaces;
using Grad.Application.StudentFeatures.Interfaces;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.SubmissionFeatures.DTOs;
using Grad.Application.SubmissionFeatures.Interfaces;
using Grad.Domain.Model;

namespace Grad.Application.SubmissionFeatures.Services
{
	public class SubmissionServices : ISubmissionServices
	{
		private readonly ISubmissionRepository _submissionRepository;
		private readonly ISubjectRepository _subjectRepository;
		private readonly IExerciseRepository _exerciseRepository;
		private readonly IStudentRepository _studentRepository;
		private readonly IUowServices _uow;

		public SubmissionServices(ISubmissionRepository submissionRepository, ISubjectRepository subjectRepository,IUowServices uow,IExerciseRepository exerciseRepository,IStudentRepository studentRepository)
		{
			_subjectRepository = subjectRepository ?? throw new ArgumentNullException(nameof(subjectRepository));
			_submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository)); 
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
			_exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
		}

		public async Task<ResultDTO> createSubmission(CreateSubmissionDTO createSubmission,CancellationToken CT)
		{
			//if(await _studentRepository.IsEnrolled(createSubmission.SubmittedBy, createSubmission.SubjectFK, CT))
			//{
			Level level = await _exerciseRepository.GetLevel(createSubmission.SubjectFK,createSubmission.LessonID,createSubmission.LevelFK,CT);
			if (level == null)
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Failed to Complete Level"
				};
			Grade grade = GradeLevel(level, createSubmission.sEDTOs);
			if(grade == null)
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Failed to Complete Level"
				};
			Submission submission = new Submission();
			submission.SubjectFK = createSubmission.SubjectFK;
			submission.SubmittedBy = createSubmission.SubmittedBy;
			submission.LevelFK = createSubmission.LevelFK;
			submission.LessonID = createSubmission.LessonID;
			submission.Percentage = grade.Percentage;
			submission.Passed = grade.Passed;
			_studentRepository.CreateEntityAsync(submission);
			return await _uow.SaveChangesAsync() > 0 ? new ResultDTO
			{
				StatusCode = 200,
				Message = submission.Passed ? "Yay! You did it!" : "Not this time, but that’s okay! Keep practicing—you’re getting closer!"
			} : new ResultDTO
			{
				StatusCode = 400,
				Message = "Failed to Complete Level"
			};
		}
			//else
			//{
			//	return new ResultDTO
			//	{
			//		StatusCode = 403,
			//		Message = "Action cannot be performed"
			//	};
			//}

		private Grade GradeLevel(Level level,List<SEDTO> SEDTOs)
		{
			Grade grade = new Grade();
			decimal score = 0;
			if (SEDTOs.Count == 0)
				return null;
			foreach(SEDTO se in SEDTOs)
			{
				Exercise e = level.Exercise.FirstOrDefault(e => e.Id == se.Eid);
				foreach(SADTO sa in se.SADTO)
				{
					Question q = e.questions.FirstOrDefault(q=>q.Qid == sa.Qid);
					if (q.CorrectAnswer == sa.Aid)
						score += q.score;
				}
			}
			decimal total_score = level.Exercise.SelectMany(e => e.questions).Sum(q => q.score);
			grade.Percentage = (score / total_score) * 100;
			if(grade.Percentage > level.PassingPercentage)
				grade.Passed = true;
			return grade;
		}

	}
}
