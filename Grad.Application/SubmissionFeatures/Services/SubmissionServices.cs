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
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Grad.Application.SubmissionFeatures.Services
{
	public class SubmissionServices : ISubmissionServices
	{
		private readonly ISubmissionRepository _submissionRepository;
		private readonly ISubjectRepository _subjectRepository;
		private readonly IExerciseRepository _exerciseRepository;
		private readonly IStudentRepository _studentRepository;
		private readonly IUowServices _uow;
		private readonly ILogger<SubmissionServices> _logger;

		public SubmissionServices(ISubmissionRepository submissionRepository, ISubjectRepository subjectRepository,IUowServices uow,IExerciseRepository exerciseRepository,IStudentRepository studentRepository, ILogger<SubmissionServices> logger)
		{
			_subjectRepository = subjectRepository ?? throw new ArgumentNullException(nameof(subjectRepository));
			_submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository)); 
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
			_exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<ResultDTO> createSubmission(CreateSubmissionDTO createSubmission,CancellationToken CT)
		{

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
			int AttemptsRemaining = 0;
			if (level.AttemptsAllowed != -1)
			{
				int attemptsMade = await _submissionRepository.RetakeAttempted( createSubmission.SubmittedBy,createSubmission.LevelFK,CT);
				AttemptsRemaining = level.AttemptsAllowed - attemptsMade - 1;
				if (AttemptsRemaining <= 0)
					return new ResultDTO
					{
						StatusCode = 400,
						Message = "No Attempts Remaining"
						//return right answers
					};
			}
			else
				AttemptsRemaining = -1;
			Submission submission = new Submission();
			submission.SubjectFK = createSubmission.SubjectFK;
			submission.SubmittedBy = createSubmission.SubmittedBy;
			submission.LevelFK = createSubmission.LevelFK;
			submission.LessonID = createSubmission.LessonID;
			submission.quizName = level.Name;
			submission.Percentage = grade.Percentage;
			submission.AttemptsRemaining = AttemptsRemaining;
			submission.Passed = grade.Passed;
			_studentRepository.CreateEntityAsync(submission);
			if(await _uow.SaveChangesAsync() > 0)
			{
				if(!submission.Passed)
				{
					//send email to parent or guardian if exists
				}
				return new ResultDTO
				{
					StatusCode = 200,
					Message = submission.Passed ? "Yay! You did it!" : "Not this time, but that’s okay! Keep practicing—you’re getting closer!",
					result = grade
				};
			}
			else
			{
				return	new ResultDTO
				{
					StatusCode = 400,
					Message = "Failed to Complete Level"
				};
			}
		}
			

		public async Task<List<SubmissionDTO>> GetSubmissions(Guid STDid,Guid? Sid,CancellationToken cancellationToken)
		{
			List<Submission> submissions = await _submissionRepository.GetSubmissions(STDid, Sid, cancellationToken: cancellationToken);
			List<SubmissionDTO> submissionDTOs = submissions.GroupBy(a => a.LevelFK)
			.Select(g => new SubmissionDTO
			{
				SubjectName = g.First().Subject != null
					? g.First().Subject.Name
					: "Unknown Subject",

				LevelName = g.First().quizName,

				SubjectFK = g.First().SubjectFK,

				LevelFK = g.Key,

				LessonID = g.First().LessonID,

				HighestPercentage = g.Max(x => x.Percentage),

				Percentage = g.Average(x => x.Percentage),

				RetakesRemaining = g.Max(x => x.AttemptsRemaining) != -1 ? g.Min(x => x.AttemptsRemaining) : 0,

				AttemptsUsed = g.Max(x => x.AttemptsRemaining) != -1 ?  g.Max(x => x.AttemptsRemaining) - g.Min(x => x.AttemptsRemaining) : 0,

				SubmittedAt = g.Max(x => x.SubmittedAt)
			})
			.ToList();
			//List<SubmissionDTO> submissionDTOs = new List<SubmissionDTO>();
			//foreach(Submission s in submissions)
			//{
			//	submissionDTOs.Add(new SubmissionDTO
			//	{
			//		SubjectName = s.Subject != null ? s.Subject.Name : "Unknown Subject",
			//		LevelName = s.quizName,
			//		SubjectFK = s.SubjectFK,
			//		LevelFK = s.LevelFK,
			//		LessonID = s.LessonID ?? null,
			//		Percentage = s.Percentage,
			//		SubmittedAt = s.SubmittedAt,
			//		RetakesRemaining = s.AttemptsRemaining, // This would require additional logic to calculate based on the student's history and the subject's retake policy
			//		TimeTakenInMinutes = 0, // This would require additional logic to calculate based on the submission's start and end times
			//	});
			//}
			return submissionDTOs;
		}

		private Grade GradeLevel(Level level,List<SEDTO> SEDTOs)
		{
			Grade grade = new Grade();
			decimal score = 0;
			if (SEDTOs.Count == 0)
				return null;
			Dictionary<Guid, Exercise> exerciseDict = level.Exercise.ToDictionary(e => e.Id,e=>e);
			Dictionary<Guid, Question> questionDict = level.Exercise.SelectMany(e => e.questions).ToDictionary(q => q.Qid, q => q);
			foreach (SEDTO se in SEDTOs)
			{
				Exercise e = exerciseDict.TryGetValue(se.Eid, out var ex) ? ex : null;
				foreach (SADTO sa in se.SADTO)
				{
					Question q = questionDict.TryGetValue(sa.Qid, out var ques) ? ques : null;
					if (q != null && q.CorrectAnswer == sa.Aid)
						score += q.score;
				}
			}
			decimal total_score = level.Exercise.SelectMany(e => e.questions).Sum(q => q.score);
			grade.Percentage = total_score != 0 ? (score / total_score) * 100 : 0;
			if(grade.Percentage > level.PassingPercentage)
				grade.Passed = true;
			return grade;
		}

	}
}
