using System.Reflection.Emit;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.ExerciseFeatures.Interfaces;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Grad.Application.SubjectFeatures.DTOs;

namespace Grad.Application.ExerciseFeatures.Services
{
	public class ExerciseServices : IExerciseServices
	{
		private readonly ICloudinaryServices _cloudinaryServices;
		private readonly IExerciseRepository _exerciseRepository;
		private readonly ISubjectRepository _subjectRepository;
		private readonly ISubjectServices _subjectServices;
		private readonly IPerquisiteServices _perquisiteServices;
		private readonly ILogger<ExerciseServices> _logger;
		//private readonly IlessonRepository _IlessonRepository;

		public ExerciseServices(ICloudinaryServices cloudinaryServices, ISubjectRepository subjectRepository, IExerciseRepository exerciseRepository, ISubjectServices subjectServices, IPerquisiteServices perquisiteServices, ILogger<ExerciseServices> logger)
		{
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
			_exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
			_subjectRepository = subjectRepository ?? throw new ArgumentNullException(nameof(subjectRepository));
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_perquisiteServices = perquisiteServices ?? throw new ArgumentNullException(nameof(perquisiteServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		//public async Task<ResultDTO> CreateExercise(CreateLevelDTO levelDTO, CancellationToken cancellationToken) //not tested yet
		//{
		//	// Validate the exercise data
		//	if (levelDTO == null)
		//	{
		//		return new ResultDTO
		//		{
		//			Message = "Exercise data cannot be null",
		//			StatusCode = 400
		//		};
		//	}
		//	else
		//	{
		//		if (levelDTO.ExerciseDTOs == null || !levelDTO.ExerciseDTOs.Any())
		//		{
		//			return new ResultDTO
		//			{
		//				Message = "Exercise must contain at least one question",
		//				StatusCode = 400
		//			};
		//		}
		//		// Map ExerciseDTO to Exercise domain model
		//		else
		//		{
		//			SubjectContent subjectContent = await _subjectRepository.GetSubjectContentAsync(levelDTO.Sid, cancellationToken);
		//			if (subjectContent == null)
		//			{
		//				return new ResultDTO
		//				{
		//					Message = "Subject isnt found",
		//					StatusCode = 400
		//				};
		//			}
		//			LessonContent lesson = subjectContent.Lessons.FirstOrDefault(l => l.Id == levelDTO.Lid);
		//			Level level = new Level
		//			{
		//				Name = levelDTO.Name,
		//				PassingPercentage = levelDTO.PassingGradePercentage,
		//				levelDifficulty = levelDTO.levelDifficulty
		//			};

		//			string directoryPath = $"subjects/{subjectContent.Id}/";
		//			if (lesson != null)
		//			{
		//				directoryPath += $"lessonContent/{lesson.Id}/";
		//			}
		//			directoryPath += $"exercise/{level.ID}/";
		//			foreach (var exerciseDTO in levelDTO.ExerciseDTOs)
		//			{
		//				//List<IFormFile> files = new List<IFormFile>();
		//				//List<string> publicIds = new List<string>();

		//				if (exerciseDTO != null && exerciseDTO.questions.Any())
		//				{
		//					Exercise exercise = new Exercise
		//					{
		//						Name = exerciseDTO.Name,
		//						Type = exerciseDTO.Type,
		//						//total_questions = exerciseDTO.total_questions
		//					};
		//					foreach (var questionDTO in exerciseDTO.questions)
		//					{
		//						bool failed = false;
		//						Question question = new Question
		//						{
		//							prompt_text = questionDTO.prompt_text,
		//							score = questionDTO.score

		//						};
		//						if (questionDTO.prompt_image != null)
		//						{
		//							// Save the image to a location and get the path
		//							question.prompt_image = await _cloudinaryServices.UploadImageAsync(questionDTO.prompt_image, directoryPath, $"{question.Qid}", cancellationToken);
		//							if (string.IsNullOrEmpty(question.prompt_image))
		//								failed = true;
		//						}
		//						switch (exercise.Type)
		//						{
		//							case ExerciseType.MCQ:
		//								if (questionDTO.Answers.Count() < 2)
		//									return new ResultDTO
		//									{
		//										Message = " invalid MCQ exercise",
		//										StatusCode = 400
		//									};
		//								foreach (AnswerDTO ans in questionDTO.Answers)
		//								{
		//									Answer answer = new Answer
		//									{
		//										answer = ans.answer
		//									};
		//									if (ans.IMG != null)
		//									{
		//										answer.IMG = await _cloudinaryServices.UploadImageAsync(ans.IMG, directoryPath, $"{answer.Id}", cancellationToken);
		//										if (string.IsNullOrEmpty(answer.IMG))
		//											failed = true;
		//									}
		//									if (ans.isCorrect)
		//										question.CorrectAnswer = answer;
		//									else
		//										question.Answers.Add(answer);
		//								}
		//								if (!failed)
		//								{
		//									exercise.questions.Add(question);
		//								}
		//								break;
		//							case ExerciseType.Matching:
		//								if (questionDTO.Answer is AnswerDTO a && a != null)
		//								{
		//									Answer answer = new Answer
		//									{
		//										answer = a.answer
		//									};
		//									if (a.IMG != null)
		//									{
		//										answer.IMG = await _cloudinaryServices.UploadImageAsync(a.IMG, directoryPath, $"{answer.Id}", cancellationToken);
		//										if (string.IsNullOrEmpty(answer.IMG))
		//											failed = true;
		//									}
		//									question.CorrectAnswer = answer;
		//									if (!failed)
		//									{
		//										exercise.questions.Add(question);
		//									}
		//								}
		//								break;
		//							default:
		//								break;
		//						}
		//					}
		//					level.Exercise.Add(exercise);
		//				}
		//			}

		//			long res = 0;
		//			if (lesson != null)
		//			{
		//				//lessonContent.Exercise = exercise;
		//				lesson.Level = level;
		//				res = await _exerciseRepository.addExerciseToLesson(subjectContent.Id, lesson, cancellationToken);
		//			}
		//			else
		//			{
		//				res = await _exerciseRepository.addQuizToSubject(subjectContent.Id, level, cancellationToken);
		//			}
		//			if (res == 0)
		//			{

		//				await _cloudinaryServices.DeleteAsync(directoryPath, false, true);
		//				return new ResultDTO
		//				{
		//					Message = "Something went wrong",
		//					StatusCode = 500
		//				};
		//			}
		//			else
		//			{
		//				return new ResultDTO
		//				{
		//					Message = "Exercise was added successfully",
		//					StatusCode = 201
		//				};
		//			}
		//		}
		//	}
		//}


		public async Task<ResultDTO> CreateExercise(CreateLevelDTO levelDTO, CancellationToken cancellationToken) //not tested yet
		{
			// Validate the exercise data
			if (levelDTO == null)
			{
				return new ResultDTO
				{
					Message = "Exercise data cannot be null",
					StatusCode = 400
				};
			}
			else
			{
				if (levelDTO.ExerciseDTOs == null || !levelDTO.ExerciseDTOs.Any())
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
					SubjectContent subjectContent = await _subjectRepository.GetSubjectContentAsync(levelDTO.Sid, cancellationToken);
					if (subjectContent == null)
					{
						return new ResultDTO
						{
							Message = "Subject isnt found",
							StatusCode = 400
						};
					}
					Dictionary<Guid, LessonContent> lessons = subjectContent.Lessons.ToDictionary(l => l.Id, l => l);
					lessons.TryGetValue(levelDTO.Lid ?? Guid.Empty, out LessonContent lesson);
					Level level = new Level
					{
						Name = levelDTO.Name,
						PassingPercentage = levelDTO.PassingGradePercentage,
						levelDifficulty = levelDTO.levelDifficulty,
						//Perquisite = levelDTO.PerquisiteID,
						PerquisiteType = levelDTO.Lid == null ? levelDTO.PerquisiteType : PerquisiteType.None,
						Perquisite = levelDTO.Lid == null ? levelDTO.PerquisiteID : null
					};
					int perquisiteResult = await _perquisiteServices.UpdatePerquisite(levelDTO.Sid, level.PerquisiteType, level.Perquisite ?? Guid.Empty,Guid.Empty,PerquisiteType.None,level.ID,true,cancellationToken: cancellationToken);

					//switch (level.PerquisiteType)
					//{
					//	case PerquisiteType.Lesson:
					//		lessons.TryGetValue(levelDTO.PerquisiteID ?? Guid.Empty, out LessonContent Plesson);
					//		if (Plesson != null)
					//		{
					//			Plesson.NextType = PerquisiteType.Quiz;
					//			Plesson.Next = level.ID;
					//			perquisiteResult = await _IlessonRepository.editLesson(subjectContent.Id, Plesson, cancellationToken);
					//		}
					//		break;
					//	default:
					//		break;
					//}
					string directoryPath = $"subjects/{subjectContent.Id}/";
					if (lesson != null)
					{
						directoryPath += $"lessonContent/{lesson.Id}/";
					}
					directoryPath += $"exercise/{level.ID}/";
					foreach (var exerciseDTO in levelDTO.ExerciseDTOs)
					{
						//List<IFormFile> files = new List<IFormFile>();
						//List<string> publicIds = new List<string>();

						Exercise exercise = await editExercise(exerciseDTO, null, directoryPath, cancellationToken);
						//foreach (var questionDTO in exerciseDTO.questions)
						//{
						//	Question question = await editQuestion(questionDTO, null, exercise.Type, directoryPath, cancellationToken);
						//	if (question != null)
						//		exercise.questions.Add(question);/// need to add a way to let them know which question failed to be added
						//}
						level.Exercise.Add(exercise);
					}

					long res = 0;
					if (lesson != null)
					{
						//lessonContent.Exercise = exercise;
						lesson.Level = level;
						res = await _exerciseRepository.addExerciseToLesson(subjectContent.Id, lesson, cancellationToken);
					}
					else
					{
						if (level.PerquisiteType != PerquisiteType.None && perquisiteResult <= 0)
						{
							level.PerquisiteType = PerquisiteType.None;
							level.Perquisite = null;
						}
						else
							perquisiteResult = 1;
						res = await _exerciseRepository.addQuizToSubject(subjectContent.Id, level, cancellationToken);
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
							Message = perquisiteResult == 0 ? "Exercise was added successfully" : "Exercise was added successfully, but perquisite was not saved",
							StatusCode = 201
						};
					}
				}
			}
		}

		public async Task<List<LevelDTO>> GetQuizes(Guid sid, CancellationToken CT)
		{
			List<Level> levels = await _exerciseRepository.viewLevels(sid, CT);
			List<LevelDTO> levelDTOs = new List<LevelDTO>();
			if (levels == null)
				return null;
			foreach (Level level in levels)
			{
				levelDTOs.Add(
					new LevelDTO
					{
						ID = level.ID,
						Sid = sid,
						Name = level.Name,
						levelDifficulty = level.levelDifficulty,
						PassingPercentage = level.PassingPercentage,
					});
			}
			return levelDTOs;
		}

		public async Task<ResultDTO> viewLevel(LevelDTO levelDTO, bool teacher, CancellationToken CT)
		{
			Level level = await _exerciseRepository.GetLevel(levelDTO.Sid, levelDTO.Lid, levelDTO.ID, CT);
			var res =  await _subjectServices.ViewSubjectAsync(levelDTO.Sid,false,CT);
			if(res.StatusCode != 200)
			{ 	
				return new ResultDTO
				{
					Message = "Subject wasnt found",
					StatusCode = 404
				};
			}
			SubjectDTO subject = (SubjectDTO)res.result;
			if (level == null)
				return new ResultDTO
				{
					Message = "Level wasnt found",
					StatusCode = 404
				};
			List<ExerciseDTO> exerciseDTOs = new List<ExerciseDTO>();
			foreach (Exercise exercise in level.Exercise)
			{
				ExerciseDTO exerciseDTO = new ExerciseDTO
				{
					Id = exercise.Id,
					Name = exercise.Name,
					Type = exercise.Type,
				};
				List<QuestionDTO> questionDTOs = new List<QuestionDTO>();
				switch (exercise.Type)
				{
					case ExerciseType.AI:
						exerciseDTO.AI_letters = exercise.AI_letters;
						break;
					case ExerciseType.Matching:
						List<AnswerDTO> answerDTOs = new List<AnswerDTO>();
						foreach (Question question in exercise.questions)
						{
							QuestionDTO questionDTO = new QuestionDTO
							{
								Qid = question.Qid,
								prompt_text = question.prompt_text,
								imgPath = question.prompt_image,
								score = question.score
							};
							if (teacher)
							{
								questionDTO.Answer = new AnswerDTO
								{
									Id = question.Answer.Id,
									imgPath = question.Answer.IMG,
									answer = question.Answer.answer
								};
							}
							else
							{
								answerDTOs.Add(
									new AnswerDTO
									{
										Id = question.Answer.Id,
										imgPath = question.Answer.IMG,
										answer = question.Answer.answer
									});
							}
							questionDTOs.Add(questionDTO);
						}
						exerciseDTO.questions = questionDTOs;
						exerciseDTO.answers = answerDTOs;
						break;
					case ExerciseType.MCQ:
						foreach (Question question in exercise.questions)
						{
							QuestionDTO questionDTO = new QuestionDTO
							{
								Qid = question.Qid,
								prompt_text = question.prompt_text,
								imgPath = question.prompt_image,
								score = question.score
							};
							List<AnswerDTO> answers = new List<AnswerDTO>();
							foreach (Answer answer in question.Answers)
							{
								answers.Add(new AnswerDTO
								{
									Id = answer.Id,
									imgPath = answer.IMG,
									answer = answer.answer,
									isCorrect = teacher ? question.CorrectAnswer == answer.Id : false
								});
							}
							questionDTO.Answers = answers;
							questionDTOs.Add(questionDTO);
						}
						exerciseDTO.questions = questionDTOs;
						break;
				}
				exerciseDTOs.Add(exerciseDTO);
			}
			levelDTO.Exercise = exerciseDTOs;
			levelDTO.Name = level.Name;
			levelDTO.PassingPercentage = level.PassingPercentage;
			levelDTO.levelDifficulty = level.levelDifficulty;
			levelDTO.SubjectName = subject.SubjectName;
			return new ResultDTO
			{
				Message = $"{levelDTO.Exercise.Count()} Exercises were found",
				result = levelDTO,
				StatusCode = 200
			};
		}

		public async Task<ResultDTO> EditLevel(EditLevelDTO editLevel, CancellationToken cancellationToken)//update edit to handle perquisites
		{
			Level level = await _exerciseRepository.GetLevel(editLevel.Sid, editLevel.Lid, editLevel.Id, cancellationToken);
			if (level == null)
				return new ResultDTO
				{
					Message = "Level wasnt found",
					StatusCode = 404
				};
			level.Name = editLevel.Name == null ? level.Name : editLevel.Name;
			level.levelDifficulty = editLevel.levelDifficulty == null ? level.levelDifficulty : editLevel.levelDifficulty.Value;
			level.PassingPercentage = editLevel.PassingPercentage == -1 ? level.PassingPercentage : editLevel.PassingPercentage;
			string directoryPath = $"subjects/{editLevel.Sid}/";
			if (editLevel.Lid != null)
			{
				directoryPath += $"lessonContent/{editLevel.Lid}/";
			}
			directoryPath += $"exercise/{level.ID}/";
			Dictionary<Guid, Exercise> Exercises = level.Exercise.ToDictionary(e => e.Id, e => e);
			List<Exercise> exercises = new();
			if (editLevel.ExerciseDTOs != null && editLevel.ExerciseDTOs.Count > 0)
			{
				foreach(ExerciseDTO exerciseDTO in editLevel.ExerciseDTOs)
				{
					Exercise exercise = Exercises.TryGetValue(exerciseDTO.Id, out var e) ? e : null;
					if (exercise != null)
					{
						level.Exercise.Remove(exercise);
						exercise = await editExercise(exerciseDTO, exercise, directoryPath, cancellationToken);
					}
					else
					{
						exercise = await editExercise(exerciseDTO, null, directoryPath, cancellationToken);
					}
					if (exercise != null)
						exercises.Add(exercise);
					else
					{
						_logger.LogWarning("Failed to update or add exercise {ExerciseId}", exerciseDTO.Id);
						continue;
					}
				}
			}
			level.Exercise = exercises;
			int res = await _exerciseRepository.editLevel(editLevel.Sid,editLevel.Lid,level, cancellationToken);
			if (res == 0)
			{
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
					Message = "Level was updated successfully",
					StatusCode = 200
				};
			}
		}

		public async Task<ResultDTO> DeleteLevel(LevelDTO levelDTO, CancellationToken cancellationToken)
		{
			Level level = await _exerciseRepository.GetLevel(levelDTO.Sid, levelDTO.Lid, levelDTO.ID, cancellationToken);
			int res = 0;
			if (level != null)
			{
				string directoryPath = $"subjects/{levelDTO.Sid}/";
				if (levelDTO.Lid != null)
				{
					directoryPath += $"lessonContent/{levelDTO.Lid}/";
				}
				directoryPath += $"exercise/{level.ID}/";
				if (levelDTO.Lid == null || levelDTO.Lid == Guid.Empty)
				{
					res = await _perquisiteServices.removeDependency(levelDTO.Sid, level: level ,cancellationToken: cancellationToken);
					switch(res)
					{
						case 1:
							if(!await _cloudinaryServices.DeleteAsync(directoryPath, false, true, cancellationToken))
							{
								return new ResultDTO
								{
									Message = "failed to remove quiz content , quiz wasnt removed",
									StatusCode = 500
								};
							}
							res = await _exerciseRepository.DeleteQuiz(levelDTO.Sid, null, levelDTO.ID, cancellationToken);
							if(res == 0 || res == -1)
								return new ResultDTO
								{
									Message = "failed to remove quiz",
									StatusCode = 500
								};
							else
								return new ResultDTO
								{
									Message = "Quiz was removed successfully",
									StatusCode = 200
								};
						default:
							return new ResultDTO
							{
								Message = "failed to remove quiz",
								StatusCode = 500
							};
					}
				}
				else
				{
					if (!await _cloudinaryServices.DeleteAsync(directoryPath, false, true, cancellationToken))
					{
						return new ResultDTO
						{
							Message = "failed to remove exercise content , exercise wasnt removed",
							StatusCode = 500
						};
					}
					res = await _exerciseRepository.DeleteQuiz(levelDTO.Sid,levelDTO.Lid, levelDTO.ID, cancellationToken);
					if (res == 0 || res == -1)
						return new ResultDTO
						{
							Message = "failed to remove quiz",
							StatusCode = 500
						};
					else
						return new ResultDTO
						{
							Message = "Quiz was removed successfully",
							StatusCode = 200
						};
				}
			}
			else
			{
				return new ResultDTO
				{
					Message = levelDTO.Lid != null ?  "Exercise wasnt found" : "Quiz wasnt found",
					StatusCode = 404
				};
			}
		}

		private async Task<Exercise> editExercise(ExerciseDTO exerciseDTO,Exercise ?exercise, string directory, CancellationToken cancellationToken)
		{
			string directoryPath = directory;
			Exercise Nexercise = null;
			if (exercise != null)
			{
				Nexercise = exercise;
				Nexercise.Name = exerciseDTO.Name != null ? exerciseDTO.Name : exercise.Name;
				Nexercise.Type = exerciseDTO.Type != 0 ? exerciseDTO.Type : exercise.Type;
			}
			else
			{
				Nexercise = new Exercise
				{
					Name = exerciseDTO.Name,
					Type = exerciseDTO.Type,
					//total_questions = exerciseDTOs.
				};
				if (Nexercise.Type == 0)
				{
					_logger.LogWarning("Exercise {ExerciseName} has invalid type", exerciseDTO.Name);
					return null;
				}
			}
			if(exerciseDTO.Type == ExerciseType.AI)
			{
				//Dictionary<string,int> letters = exercise?.AI_letters?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, int>();
				//foreach(var letter in exerciseDTO.AI_letters)
				//{
				//	if (!letters.ContainsKey(letter.Key))
				//		letters.Add(letter.Key, letter.Value);
				//	else
				//		letters[letter.Key] = letter.Value;
				//}
				Dictionary<string, int> letters = new Dictionary<string, int>();
				foreach(var letter in exerciseDTO.AI_letters)
				{
					letters[letter.Key] = letter.Value;
				}
				Nexercise.AI_letters = letters;
			}
			else
			{
				Dictionary<Guid, Question> Questions = exercise != null ? exercise.questions.ToDictionary(q => q.Qid, q => q) : new Dictionary<Guid, Question>();
				foreach (QuestionDTO questionDTO in exerciseDTO.questions)
				{

					Question question = Questions.TryGetValue(questionDTO.Qid, out var q) ? q : null;
					if (question == null)
					{
						question = await editQuestion(questionDTO, null, Nexercise.Type, directoryPath, cancellationToken);
					}
					else
					{
						exercise.questions.Remove(question);
						question = await editQuestion(questionDTO, question, Nexercise.Type, directoryPath, cancellationToken);
					}
					if (question != null)
					{
						Nexercise.questions.Add(question);
					}
					else
					{
						_logger.LogWarning("Failed to update or add question {QuestionId}", questionDTO.Qid);
						continue;
					}

				}
			}
			return Nexercise;
		}


		private async Task<Question> editQuestion(QuestionDTO questionDTO, Question? question, ExerciseType type, string directory, CancellationToken cancellation)
		{
			Question Nquestion = null;
			if (question != null)
				Nquestion = question;
			else
				Nquestion = new Question();
			if (questionDTO.prompt_text != null)
			{
				Nquestion.prompt_text = questionDTO.prompt_text;
			}
			Nquestion.score = questionDTO.score != 0 ? questionDTO.score : question.score;
			if (questionDTO.prompt_image != null)
			{
				Nquestion.prompt_image = await _cloudinaryServices.UploadImageAsync(questionDTO.prompt_image, directory, Nquestion.Qid.ToString(), cancellation);
				if (string.IsNullOrEmpty(Nquestion.prompt_image))
				{
					_logger.LogWarning("Failed to upload prompt image for question {QuestionId}", questionDTO.Qid);
				}
				else
				{
					if (question != null && question.prompt_image != null)
					{
						string directorypath = directory + $"{question.Qid}";
						if (await _cloudinaryServices.DeleteAsync(directorypath))
							_logger.LogInformation("Deleted old prompt image for question {QuestionId}", question.Qid);
					}
					Nquestion.prompt_image = Nquestion.prompt_image;
				}
			}
			switch (type)
			{
				case ExerciseType.Matching:
					if (questionDTO.Answer != null)
					{
						Answer answer = question != null ? question.Answer : null;
						if (answer != null)
						{
							Nquestion.Answer = await editAnswer(questionDTO.Answer, answer, directory, cancellation);
						}
						else
						{
							Nquestion.Answer = await editAnswer(questionDTO.Answer, null, directory, cancellation);
						}
						if (Nquestion.Answer == null)
						{
							_logger.LogWarning("Failed to update or add matching answer for question {QuestionId}", questionDTO.Qid);
						}
						Nquestion.CorrectAnswer = Nquestion.Answer.Id;
					}
					break;
				case ExerciseType.MCQ:
					Dictionary<Guid,Answer> answers = question != null ? question.Answers.ToDictionary(a=>a.Id,a=>a) : new Dictionary<Guid, Answer>();
					Nquestion.Answers = new List<Answer>();
					foreach (AnswerDTO answerDTO in questionDTO.Answers)
					{
						Answer answer = (answerDTO.Id != null && answers.TryGetValue(answerDTO.Id, out var a)) ? a : null;
						if (answer != null)
						{
							answer = await editAnswer(answerDTO, answer, directory, cancellation);
							if (answer != null)
								Nquestion.Answers.Add(answer);
							else
							{
								_logger.LogWarning("Failed to update existing MCQ answer for question {QuestionId}", questionDTO.Qid);
								continue;
							}
						}
						else
						{
							answer = await editAnswer(answerDTO, null, directory, cancellation);
							if (answer != null)
								Nquestion.Answers.Add(answer);
							else
							{
								_logger.LogWarning("Failed to add new MCQ answer for question {QuestionId}", questionDTO.Qid);
								continue;
							}
						}
						if (answerDTO.isCorrect)
							Nquestion.CorrectAnswer = answer.Id;
					}
					break;
			}
			return Nquestion;
		}

			//if (question == null)
			//{
			//	foreach (AnswerDTO answerDTO in questionDTO.Answers)
			//	{
			//		Answer answer = await editAnswer(answerDTO, null, directory, cancellation);
			//		if (answer != null)
			//		{
			//			if (answerDTO.isCorrect)
			//				Nquestion.CorrectAnswer = answer.Id;
			//			Nquestion.Answers.Add(answer);
			//		}
			//		else
			//		{
			//			if (!Nquestion.prompt_image.IsNullOrEmpty())
			//			{
			//				if (await _cloudinaryServices.DeleteAsync(directory + $"{Nquestion.Qid}"))
			//					Console.WriteLine($"deleted image with id {Nquestion.Qid}");
			//			}
			//			Console.WriteLine($"failed to add question {Nquestion.Qid}");
			//			return null;
			//		}
			//	}
			//}
			//else
			//{
			//	if (questionDTO.Answer != null)
			//	{

			//	}
			//	if (questionDTO.Answers != null && questionDTO.Answers.Count() > 0)
			//	{
			//		foreach (AnswerDTO answerDTO in questionDTO.Answers)
			//		{
			//			Answer answer = question.Answers.FirstOrDefault(a => a.Id == answerDTO.Id);
			//			if (answer != null)
			//			{
			//				question.Answers.Remove(answer);
			//				answer = await editAnswer(answerDTO, answer, directory, cancellation);
			//				if (answer != null)
			//					question.Answers.Add(answer);
			//				else
			//				{
			//					if (!Nquestion.prompt_image.IsNullOrEmpty())
			//					{
			//						if (await _cloudinaryServices.DeleteAsync(directory + $"{Nquestion.Qid}"))
			//							Console.WriteLine($"deleted image with id {Nquestion.Qid}");
			//					}
			//					Console.WriteLine($"failed to update question {Nquestion.Qid}");
			//					return null;
			//				}
			//			}
			//			else
			//			{
			//				answer = await editAnswer(answerDTO, null, directory, cancellation);
			//				if (answer != null)
			//					question.Answers.Add(answer);
			//				else
			//				{
			//					if (!Nquestion.prompt_image.IsNullOrEmpty())
			//					{
			//						if (await _cloudinaryServices.DeleteAsync(directory + $"{Nquestion.Qid}"))
			//							Console.WriteLine($"deleted image with id {Nquestion.Qid}");
			//					}
			//					Console.WriteLine($"failed to update question {Nquestion.Qid}");
			//					return null;
			//				}
			//			}
			//		}
			//	}

		//}
		//	return Nquestion;
		//}

		private async Task<Answer> editAnswer(AnswerDTO answerDTO, Answer? answer, string directory, CancellationToken cancellation)
		{
			Answer Nanswer = null;
			if (answer != null)
				Nanswer = answer;
			else
				Nanswer = new Answer();
			if (answerDTO.IMG != null)
			{
				Nanswer.IMG = await _cloudinaryServices.UploadImageAsync(answerDTO.IMG, directory, answerDTO.Id.ToString(), cancellation);
				if (string.IsNullOrEmpty(Nanswer.IMG))
				{
					_logger.LogWarning("Failed to upload image for answer {AnswerId}", answerDTO.Id);
					return null;
				}
				else
				{
					if (answer != null && answer.IMG != null)
					{
						string directorypath = directory + $"{answer.Id}";
						if (await _cloudinaryServices.DeleteAsync(directorypath))
							_logger.LogInformation("Deleted old image for answer {AnswerId}", answer.Id);
					}
					Nanswer.IMG = Nanswer.IMG;
				}
			}
			if (answerDTO.answer != null)
			{
				Nanswer.answer = answerDTO.answer;
			}
			return Nanswer;
		}
	}
}
				

			// Save the exercise to the database (this is just a placeholder, implement your own data access logic)
			// _exerciseRepository.Save(exercise);
