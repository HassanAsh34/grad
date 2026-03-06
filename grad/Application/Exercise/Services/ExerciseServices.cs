using grad.Application.Common.DTOs;
using grad.Application.Common.Interfaces;
using grad.Application.exercise.DTOs;
using grad.Application.lesson.DTOs;
using grad.Domain.Model;
using grad.Infrastructure.Persistence;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using grad.Application.exercise.Interfaces;

namespace grad.Application.exercise.Services
{
	public class ExerciseServices : IExerciseServices
	{
		private readonly ICloudinaryServices _cloudinaryServices;
		private readonly IMongoCollection<SubjectContent> _subjects;

		public ExerciseServices(ICloudinaryServices cloudinaryServices, MongoDBContext context)
		{
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
			_subjects = context.Subjects ?? throw new ArgumentNullException(nameof(context.Subjects));
		}
		//public async Task<ResultDTO> CreateExercise(CreateExerciseDTO exerciseDTO, CancellationToken cancellationToken) //not tested yet
		//{
		//	// Validate the exercise data
		//	if (exerciseDTO == null)
		//	{
		//		return new ResultDTO
		//		{
		//			Message = "Exercise data cannot be null",
		//			StatusCode = StatusCodes.Status400BadRequest
		//		};
		//	}
		//	else
		//	{
		//		if (exerciseDTO.questions == null || !exerciseDTO.questions.Any())
		//		{
		//			return new ResultDTO
		//			{
		//				Message = "Exercise must contain at least one question",
		//				StatusCode = StatusCodes.Status400BadRequest
		//			};
		//		}
		//		// Map ExerciseDTO to Exercise domain model
		//		else
		//		{
		//			var filter = exerciseDTO.Lid != null ? Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, exerciseDTO.Sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == exerciseDTO.Lid)) : Builders<SubjectContent>.Filter.Eq(s => s.Id, exerciseDTO.Sid);
		//			SubjectContent subjectContent = await _subjects.Find(filter).FirstOrDefaultAsync();
		//			if (subjectContent == null)
		//				return new ResultDTO
		//				{
		//					StatusCode = StatusCodes.Status500InternalServerError,
		//					Message = "Something went wrong"
		//				};
		//			LessonContent lessonContent = null;
		//			if (exerciseDTO.Lid != null)
		//			{
		//				lessonContent = subjectContent.Lessons.FirstOrDefault(l => l.Id == exerciseDTO.Lid);
		//				if (lessonContent == null)
		//				{
		//					return new ResultDTO
		//					{
		//						Message = "Lesson not found",
		//						StatusCode = StatusCodes.Status404NotFound
		//					};
		//				}
		//			}
		//			Exercise exercise = new Exercise
		//			{
		//				//Id = exerciseDTO.Id,
		//				Name = exerciseDTO.Name,
		//				total_questions = exerciseDTO.questions.Count,
		//				PassingGrade = (exerciseDTO.PassingGradePercentage / 100) * exerciseDTO.total_score,
		//				questions = new List<Question>(),
		//				levelDifficulty = exerciseDTO.levelDifficulty
		//			};
		//			string directoryPath = $"subjects/{subjectContent.Id}/";
		//			if (lessonContent != null)
		//			{
		//				directoryPath+=$"lessonContent/{lessonContent.Id}";
		//			}
		//			directoryPath+=$"exercise/{exercise.Id}/";
		//			foreach (var questionDTO in exerciseDTO.questions)
		//			{
		//				//List<IFormFile> files = new List<IFormFile>();
		//				//List<string> publicIds = new List<string>();
		//				bool failed = false;
		//				Question question = new Question
		//				{
		//					question_type = questionDTO.question_type,
		//					prompt_text = questionDTO.prompt_text
		//				};
		//				if (questionDTO.prompt_image != null)
		//				{
		//					// Save the image to a location and get the path
		//					question.prompt_image = await _cloudinaryServices.UploadImageAsync(questionDTO.prompt_image, directoryPath, $"{question.Qid}", cancellationToken);
		//					if (question.prompt_image.IsNullOrEmpty())
		//						failed = true;
		//				}
		//				foreach (AnswerDTO a in questionDTO.Answers)
		//				{
		//					Answer answer = new Answer
		//					{
		//						answer = a.answer
		//					};
		//					if (a.IMG != null)
		//					{
		//						answer.IMG = await _cloudinaryServices.UploadVideoAsync(a.IMG, directoryPath, $"{answer.Id}", cancellationToken);
		//						if (answer.IMG.IsNullOrEmpty())
		//							failed = true;
		//					}
		//					if (a.isCorrect)
		//						question.CorrectAnswer = answer;
		//					else
		//						question.Answers.Add(answer);
		//				}
		//				if (!failed)
		//				{
		//					exercise.questions.Add(question);
		//				}
		//			}
		//			long res = 0;
		//			if(lessonContent != null)
		//			{
		//				lessonContent.Exercise = exercise;
		//				var update = Builders<SubjectContent>.Update.Set("Lessons.$.Exercises", lessonContent.Exercise);
		//				var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
		//				res = result.ModifiedCount;
		//			}
		//			else
		//			{
		//				var update = Builders<SubjectContent>.Update.Push(s => s.Quizzes,exercise);
		//				var result = await _subjects.UpdateOneAsync(filter, update);
		//				res = result.ModifiedCount;
		//			}
		//			if (res == 0)
		//			{
		//				await _cloudinaryServices.DeleteAsync(directoryPath, false, true);
		//				return new ResultDTO
		//				{
		//					Message = "Something went wrong",
		//					StatusCode = StatusCodes.Status500InternalServerError
		//				};
		//			}
		//			else
		//			{
		//				return new ResultDTO
		//				{
		//					Message = "Exercise was added successfully",
		//					StatusCode = StatusCodes.Status201Created
		//				};
		//			}
		//		}
		//	}
		//}
		public async Task<ResultDTO> CreateExercise(CreateExerciseDTO exerciseDTO, CancellationToken cancellationToken) //not tested yet
		{
			// Validate the exercise data
			if (exerciseDTO == null)
			{
				return new ResultDTO
				{
					Message = "Exercise data cannot be null",
					StatusCode = StatusCodes.Status400BadRequest
				};
			}
			else
			{
				if (exerciseDTO.questions == null || !exerciseDTO.questions.Any())
				{
					return new ResultDTO
					{
						Message = "Exercise must contain at least one question",
						StatusCode = StatusCodes.Status400BadRequest
					};
				}
				// Map ExerciseDTO to Exercise domain model
				else
				{
					var filter = exerciseDTO.Lid != null ? Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, exerciseDTO.Sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == exerciseDTO.Lid)) : Builders<SubjectContent>.Filter.Eq(s => s.Id, exerciseDTO.Sid);
					Exercise exercise = new Exercise
					{
						//Id = exerciseDTO.Id,
						Name = exerciseDTO.Name,
						total_questions = exerciseDTO.questions.Count,
						PassingGrade = (exerciseDTO.PassingGradePercentage / 100) * exerciseDTO.total_score,
						questions = new List<Question>(),
						levelDifficulty = exerciseDTO.levelDifficulty
					};
					string directoryPath = $"subjects/{exerciseDTO.Sid}/";
					if (exerciseDTO.Lid != null)
					{
						directoryPath += $"lessonContent/{exerciseDTO.Lid}/";
					}
					directoryPath += $"exercise/{exercise.Id}/";
					foreach (var questionDTO in exerciseDTO.questions)
					{
						//List<IFormFile> files = new List<IFormFile>();
						//List<string> publicIds = new List<string>();
						bool failed = false;
						Question question = new Question
						{
							question_type = questionDTO.question_type,
							prompt_text = questionDTO.prompt_text
						};
						if (questionDTO.prompt_image != null)
						{
							// Save the image to a location and get the path
							question.prompt_image = await _cloudinaryServices.UploadImageAsync(questionDTO.prompt_image, directoryPath, $"{question.Qid}", cancellationToken);
							if (question.prompt_image.IsNullOrEmpty())
								failed = true;
						}
						foreach (AnswerDTO a in questionDTO.Answers)
						{
							Answer answer = new Answer
							{
								answer = a.answer
							};
							if (a.IMG != null)
							{
								answer.IMG = await _cloudinaryServices.UploadImageAsync(a.IMG, directoryPath, $"{answer.Id}", cancellationToken);
								if (answer.IMG.IsNullOrEmpty())
									failed = true;
							}
							if (a.isCorrect)
								question.CorrectAnswer = answer;
							else
								question.Answers.Add(answer);
						}
						if (!failed)
						{
							exercise.questions.Add(question);
						}
					}
					long res = 0;
					if (exerciseDTO.Lid != null)
					{
						//lessonContent.Exercise = exercise;
						var update = Builders<SubjectContent>.Update.Set("Lessons.$.Exercise", exercise);
						var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
						res = result.ModifiedCount;
					}
					else
					{
						var update = Builders<SubjectContent>.Update.Push(s => s.Quizzes, exercise);
						var result = await _subjects.UpdateOneAsync(filter, update);
						res = result.ModifiedCount;
					}
					if (res == 0)
					{
						
						await _cloudinaryServices.DeleteAsync(directoryPath, false, true);
						return new ResultDTO
						{
							Message = "Something went wrong",
							StatusCode = StatusCodes.Status500InternalServerError
						};
					}
					else
					{
						return new ResultDTO
						{
							Message = "Exercise was added successfully",
							StatusCode = StatusCodes.Status201Created
						};
					}
				}
			}
		}
	}
}
				

			// Save the exercise to the database (this is just a placeholder, implement your own data access logic)
			// _exerciseRepository.Save(exercise);
