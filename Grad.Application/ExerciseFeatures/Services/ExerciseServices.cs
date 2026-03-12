using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.ExerciseFeatures.Interfaces;
using Grad.Domain.Model;
using Grad.Application.SubjectFeatures.Interfaces;

namespace Grad.Application.ExerciseFeatures.Services
{
	public class ExerciseServices : IExerciseServices
	{
		private readonly ICloudinaryServices _cloudinaryServices;
		private readonly IExerciseRepository _exerciseRepository;
		private readonly ISubjectRepository _subjectRepository;

		public ExerciseServices(ICloudinaryServices cloudinaryServices,ISubjectRepository subjectRepository,IExerciseRepository exerciseRepository)
		{
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
			_exerciseRepository = exerciseRepository ?? throw new ArgumentNullException( nameof(exerciseRepository));
			_subjectRepository =  subjectRepository ?? throw new ArgumentNullException(nameof(subjectRepository));
		}

		public async Task<ResultDTO> CreateExercise(CreateExerciseDTO exerciseDTO, CancellationToken cancellationToken) //not tested yet
		{
			// Validate the exercise data
			if (exerciseDTO == null)
			{
				return new ResultDTO
				{
					Message = "Exercise data cannot be null",
					StatusCode = 400
				};
			}
			else
			{
				if (exerciseDTO.questions == null || !exerciseDTO.questions.Any())
				{
					return new ResultDTO
					{
						Message = "Exercise must contain at least one question",
						StatusCode = 400
					};
				}
				// Map ExerciseDTO to Exercise domain model
				else
				{
					SubjectContent subjectContent = await _subjectRepository.GetSubjectContentAsync(exerciseDTO.Sid, cancellationToken);
					if (subjectContent == null)
					{
						return new ResultDTO
						{
							Message = "Subject isnt found",
							StatusCode = 400
						};
					}
					LessonContent lesson = subjectContent.Lessons.FirstOrDefault(l => l.Id == exerciseDTO.Lid);
					Exercise exercise = new Exercise
					{
						//Id = exerciseDTO.Id,
						Name = exerciseDTO.Name,
						total_questions = exerciseDTO.questions.Count,
						PassingGrade = (exerciseDTO.PassingGradePercentage / 100) * exerciseDTO.total_score,
						questions = new List<Question>(),
						levelDifficulty = exerciseDTO.levelDifficulty
					};
					string directoryPath = $"subjects/{subjectContent.Id}/";
					if (lesson != null)
					{
						directoryPath += $"lessonContent/{lesson.Id}/";
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
							question.prompt_image = await _cloudinaryServices.UploadImageAsync(questionDTO.prompt_image,directoryPath, $"{question.Qid}", cancellationToken);
							if (string.IsNullOrEmpty(question.prompt_image))
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
								if (string.IsNullOrEmpty(answer.IMG))
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
					if (lesson != null)
					{
						//lessonContent.Exercise = exercise;
						lesson.Exercise = exercise;
						res = await _exerciseRepository.addExerciseToLesson(subjectContent.Id, lesson, cancellationToken);
					}
					else
					{
						res = await _exerciseRepository.addQuizToSubject(subjectContent.Id, exercise, cancellationToken);	
					}
					if (res == 0)
					{
						
						await _cloudinaryServices.DeleteAsync(directoryPath, false, true);
						return new ResultDTO
						{
							Message = "Something went wrong",
							StatusCode = 500
						};
					}
					else
					{
						return new ResultDTO
						{
							Message = "Exercise was added successfully",
							StatusCode = 201
						};
					}
				}
			}
		}
	}
}
				

			// Save the exercise to the database (this is just a placeholder, implement your own data access logic)
			// _exerciseRepository.Save(exercise);
