using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Domain.Model;
using Microsoft.Extensions.Logging;

namespace Grad.Application.SubjectFeatures.Services
{
	public class SubjectServices : ISubjectServices
	{
		private readonly ISubjectRepository _subjectRepository;
		private readonly IUowServices _uowServices;
		private readonly ICloudinaryServices _cloudinary;
		private readonly ILogger<SubjectServices> _logger;

		public SubjectServices(ISubjectRepository subjectRepository, IUowServices uowServices, ICloudinaryServices cloudinary, ILogger<SubjectServices> logger)
		{
			_subjectRepository = subjectRepository ?? throw new ArgumentNullException(nameof(subjectRepository));
			_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));
			_cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			if (await _subjectRepository.IsSubjectExistAsync(subjectName: subject.SubjectName, deaf_mute: subject.deaf_mute, cancellationToken: cancellationToken))
			{
				return new ResultDTO
				{
					Message = "Subject already exists",
					StatusCode = 409
				};
			}

			Subject sub = new Subject
			{
				Name = subject.SubjectName,
				deaf_mute = subject.deaf_mute
			};

			int res = await _subjectRepository.AddSubject(sub, cancellationToken);

			return new ResultDTO
			{
				Message = res != 0 ? "Subject added successfully" : "Failed to add subject",
				StatusCode = res != 0 ? 200 : 500,
				result = sub.Id
			};
		}

		public Task<ResultDTO> UpdateSubject(Guid subjectid, string SubjectName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<ResultDTO> ViewSubjectsAsync(Guid? Tid, List<Guid> ?guids, int disability = -1, IEnumerable<Enrollement> ?enrollements = null, CancellationToken cancellationToken = default)
		{
			List<Subject> subjects;

			if (Tid != null)
			{
				subjects = await _subjectRepository.GetSubjectsByTeacherAsync(Tid.Value, cancellationToken);
				if (!subjects.Any())
				{
					return new ResultDTO
					{
						StatusCode = 404,
						Message = "You Are not assigned to any subjects at the moment"
					};
				}
			}
			else if (guids != null)
			{
				if (enrollements != null)
				{
					subjects = await _subjectRepository.GetSubjectsByIdsAsync(guids, cancellationToken);
				}
				else
				{
					bool? deafMute = disability == -1 ? null : (disability > 1);
					subjects = await _subjectRepository.GetSubjectsFilteredAsync(deafMute, guids, cancellationToken);
				}
			}
			else if (disability == -1)
			{
				subjects = await _subjectRepository.GetAllSubjectsAsync(cancellationToken);
			}
			else
			{
				subjects = await _subjectRepository.GetSubjectsFilteredAsync(disability > 1, null, cancellationToken);
			}

			var subjectDTOs = subjects.Select(s => new SubjectDTO
			{
				SubjectId = s.Id,
				SubjectName = s.Name,
				deaf_mute = s.deaf_mute,
				progress = s.LessonCount != 0 ? enrollements != null ? (enrollements.Where(e => e.SUBFK == s.Id).Select(e=>e.studentProgresses).ToList().Count / s.LessonCount) * 100: 0 : 0, 
 			}).ToList();

			return new ResultDTO
			{
				Message = subjectDTOs.Any() ? "Subjects retrieved successfully" : "No result",
				StatusCode = subjectDTOs.Any() ? 200 : 404,
				result = subjectDTOs
			};
		}

		public async Task<ResultDTO> ViewSubjectAsync(Guid sid, bool all, CancellationToken cancellationToken)
		{
			var subject = await _subjectRepository.GetSubjectWithRelationsAsync(sid, cancellationToken);
			if (subject == null)
			{
				return new ResultDTO
				{
					Message = "Subject not found",
					StatusCode = 404
				};
			}

			var subjectContent = await _subjectRepository.GetSubjectContentAsync(sid, cancellationToken);
			int lessonsCount = subjectContent?.Lessons?.Count() ?? 0;
			int QuizCount = subjectContent?.Quizzes?.Count() ?? 0;
			int WordsCount = subjectContent?.Dictionary?.wordItems?.Count() ?? 0;
			int TotalSubmissions = subjectContent?.Lessons?.Count ?? 0;
			decimal AvgerageGrades = subject?.Submissions?.Where(s => s.LessonID == null).Average(s => s.Percentage) ?? 0;
			int failedSubmissions = subject?.Submissions?.Count(s => !s.Passed && s.LessonID == null) ?? 0;
			int passedSubmissions = subject?.Submissions?.Count(s => s.Passed && s.LessonID == null) ?? 0;


			var subjectDTO = new SubjectDTO
			{
				SubjectId = subject.Id,
				SubjectName = subject.Name,
				deaf_mute = subject.deaf_mute,
				studentsCount = subject.Students?.Count() ?? 0,
				teachersCount = all ? (subject.AssignedSubjects?.Count() ?? 0) : 0,
				lessonsCount = lessonsCount,
				levelsCount = all ? lessonsCount : 0,
				WordCount = WordsCount,
				submissionsCount = TotalSubmissions,
				AvgerageGrades = AvgerageGrades,
				failure_rate = TotalSubmissions > 0 ? failedSubmissions / (decimal)TotalSubmissions * 100 : 0,
				success_rate = TotalSubmissions > 0 ? passedSubmissions / (decimal)TotalSubmissions * 100 : 0
			};

			return new ResultDTO
			{
				Message = "Subject retrieved successfully",
				StatusCode = 200,
				result = subjectDTO
			};
		}

		public async Task<ResultDTO> RemoveSubject(Guid subjectid, CancellationToken cancellationToken)
		{
			Subject subject = await _subjectRepository.GetSubjectWithRelationsAsync(subjectid, cancellationToken);
			if (subject == null)
			{
				return new ResultDTO
				{
					Message = "Subject not found",
					StatusCode = 404
				};
			}
			else
			{
				SubjectContent subjectContent = await _subjectRepository.GetSubjectContentAsync(subjectid, cancellationToken);
				if(subjectContent != null)
				{
					if(!await _cloudinary.DeleteAsync($"subjects/{subjectid}", folder: true, cancellationToken: cancellationToken))
					{
						return new ResultDTO
						{
							Message = "Failed to delete subject media from cloud storage",
							StatusCode = 500
						};
					}
				}
				int res = await _subjectRepository.DeleteSubject(subject, cancellationToken);
				return new ResultDTO
				{
					Message = res != 0 ? "Subject deleted successfully" : "Failed to delete subject",
					StatusCode = res != 0 ? 200 : 500
				};
			}
		}

		public async Task<ResultDTO> addwords(AddVocabDTO vocabDTO, CancellationToken cancellationToken)
		{
			if (vocabDTO == null || vocabDTO.sid == null)
			{
				return new ResultDTO
				{
					Message = "Invalid request data",
					StatusCode = 400
				};
			}

			string folder = $"subjects/{vocabDTO.sid}/vocabulary";

			// Zip streams and filenames for Cloudinary


			var urls = await _cloudinary.UploadImagesAsync(vocabDTO.files, folder, vocabDTO.word, cancellationToken);
			var vocab = new Vocabulary();
			for (int i = 0; i < vocabDTO.word.Count; i++)
			{
				vocab.wordItems.Add(new WordItem
				{
					Word = vocabDTO.word[i],
					ImagePath = urls.ElementAtOrDefault(i) ?? string.Empty
				});
			}

			int modifiedCount = await _subjectRepository.AddVocabularyAsync(vocabDTO.sid.Value, vocab, cancellationToken);

			if (modifiedCount <= 0)
			{
				return new ResultDTO
				{
					Message = modifiedCount == 0 ? "These words may be already existing or subject was not found" : "Backend storage error",
					StatusCode = 409
				};
			}
			else if (modifiedCount < vocabDTO.word.Count)
			{
				return new ResultDTO
				{
					Message = "Some words were not added because they already exist or failed to save them",
					StatusCode = 207
				};
			}
			else
			{
				return new ResultDTO
				{
					Message = "Words were added successfully",
					StatusCode = 201
				};
			}
		}

		public async Task<bool> updateCountAsync(Guid sid,CancellationToken cancellationToken)
		{
			return await _subjectRepository.updateLessonCountAsync(sid, cancellationToken);
		}

		public async Task<bool> IsSubjectExist(string? subjectName = "", bool? deaf_mute = false, Guid? subjectId = null, CancellationToken cancellationToken = default)
		{
			return await _subjectRepository.IsSubjectExistAsync(subjectName, deaf_mute, subjectId, cancellationToken);
		}
	}
}


