using System.Diagnostics;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.StudentFeatures.DTOs;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Domain.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Grad.Application.SubjectFeatures.Services
{
	public class SubjectServices : ISubjectServices
	{
		private readonly ISubjectRepository _subjectRepository;
		private readonly IUowServices _uowServices;
		private readonly ICloudinaryServices _cloudinary;
		private readonly ILogger<SubjectServices> _logger;
		private readonly IRedisServices _redisServices;
		private readonly IMemoryCache _memoryCache;

		private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
		private const string ALL_SUBJECTS_KEY = "subjects_all";

		public SubjectServices(
			ISubjectRepository subjectRepository,
			IUowServices uowServices,
			ICloudinaryServices cloudinary,
			ILogger<SubjectServices> logger,
			IRedisServices redisServices,
			IMemoryCache memoryCache)
		{
			_subjectRepository = subjectRepository ?? throw new ArgumentNullException(nameof(subjectRepository));
			_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));
			_cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
			_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		}

		// -------------------------------------------------------------------------
		// Subject CRUD
		// -------------------------------------------------------------------------

		public async Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			if (await _subjectRepository.IsSubjectExistAsync(
					subjectName: subject.SubjectName,
					deaf_mute: subject.deaf_mute,
					cancellationToken: cancellationToken))
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
				deaf_mute = subject.deaf_mute,
				AI_supported = subject.AI_supported
			};

			int res = await _subjectRepository.AddSubject(sub, cancellationToken);

			if (res != 0)
				await BustSubjectListCache();

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

		// -------------------------------------------------------------------------
		// View subjects list — uses cache + in-memory filtering, zero DB calls
		// -------------------------------------------------------------------------

		//public async Task<ResultDTO> ViewSubjectsAsync(
		//	bool Teacher,
		//	List<Guid>? guids,
		//	int disability = -1,
		//	Dictionary<Guid, EnrollmentDTO>? enrollements = null,
		//	CancellationToken cancellationToken = default) // without loggers
		//{
		//	var allSubjects = await GetAllSubjectsCachedAsync(cancellationToken);

		//	// All filtering happens in memory — no DB round trips
		//	List<SubjectItemDTO> subjects = null;

		//	if (Teacher)
		//	{
		//		//subjects = await _subjectRepository.GetSubjectsByTeacherAsync(Tid.Value, cancellationToken);
		//		subjects = GetSubjectsByIds(guids, allSubjects);
		//		if (subjects.Count == 0)   // avoid double-enumeration vs .Any()
		//			return new ResultDTO
		//			{
		//				StatusCode = 404,
		//				Message = "You Are not assigned to any subjects at the moment"
		//			};
		//	}
		//	else if (guids != null)
		//	{
		//		// BUG FIX: was || (always true when enrollements != null)
		//		//if (enrollements != null && (enrollements.Count == 0 || enrollements.Count > 0))
		//		if (enrollements?.Count >= 0)
		//		{
		//			subjects = GetSubjectsByIds(guids, allSubjects);
		//		}
		//		else
		//		{
		//			bool? deafMute = disability == -1 ? null : disability > 1;
		//			subjects = GetSubjectsFiltered(allSubjects, deafMute, guids);
		//		}
		//	}
		//	else if (disability == -1)
		//	{
		//		subjects = allSubjects;
		//	}
		//	else
		//	{
		//		subjects = GetSubjectsFiltered(allSubjects,disability > 1, null);
		//	}


		//	//if (Tid != null && subjects.Count == 0)
		//	//	return new ResultDTO
		//	//	{
		//	//		StatusCode = 404,
		//	//		Message = "You Are not assigned to any subjects at the moment"
		//	//	};

		//	bool hasEnrollements = enrollements is { Count: > 0 };
		//	var subjectDTOs = new List<SubjectDTO>(subjects.Count);

		//	foreach (SubjectItemDTO s in subjects)
		//	{
		//		float progress = 0f;
		//		if (s.LessonsCount > 0 &&
		//			hasEnrollements &&
		//			enrollements!.TryGetValue(s.Id, out var enrollment))
		//		{
		//			progress = (enrollment.Progress / (float)s.LessonsCount) * 100f;
		//		}

		//		subjectDTOs.Add(new SubjectDTO
		//		{
		//			SubjectId = s.Id,
		//			SubjectName = s.SubjectName,
		//			deaf_mute = s.deaf_mute,
		//			progress = progress
		//		});
		//	}

		//	bool hasResults = subjectDTOs.Count > 0;
		//	return new ResultDTO
		//	{
		//		Message = hasResults ? "Subjects retrieved successfully" : "No result",
		//		StatusCode = hasResults ? 200 : 404,
		//		result = hasResults ? subjectDTOs : null
		//	};
		//}

		// -------------------------------------------------------------------------
		// View single subject — always hits DB (full detail, not list view)
		// -------------------------------------------------------------------------

		public async Task<ResultDTO> ViewSubjectsAsync(
			bool Teacher,
			List<Guid>? guids,
			int disability = -1,
			Dictionary<Guid, EnrollmentDTO>? enrollements = null,
			CancellationToken cancellationToken = default)
		{
			var sw = Stopwatch.StartNew();
			var allSubjects = await GetAllSubjectsCachedAsync(cancellationToken);
			_logger.LogInformation("After subjects cache read: {ms}ms", sw.ElapsedMilliseconds);

			sw.Restart();

			// All filtering happens in memory — no DB round trips
			List<SubjectItemDTO> subjects = null;
			if (Teacher)
			{
				subjects = GetSubjectsByIds(guids, allSubjects);
				_logger.LogInformation("After teacher filter: {ms}ms", sw.ElapsedMilliseconds);
				sw.Restart();
				if (subjects.Count == 0)
					return new ResultDTO
					{
						StatusCode = 404,
						Message = "You Are not assigned to any subjects at the moment"
					};
			}
			else if (guids != null)
			{
				if (enrollements?.Count >= 0)
				{
					subjects = GetSubjectsByIds(guids, allSubjects);
				}
				else
				{
					bool? deafMute = disability == -1 ? null : disability > 1;
					subjects = GetSubjectsFiltered(allSubjects, deafMute, guids);
				}
				_logger.LogInformation("After guid filter: {ms}ms", sw.ElapsedMilliseconds);
				sw.Restart();
			}
			else if (disability == -1)
			{
				subjects = allSubjects;
				_logger.LogInformation("After all subjects assign: {ms}ms", sw.ElapsedMilliseconds);
				sw.Restart();
			}
			else
			{
				subjects = GetSubjectsFiltered(allSubjects, disability > 1, null);
				_logger.LogInformation("After disability filter: {ms}ms", sw.ElapsedMilliseconds);
				sw.Restart();
			}

			bool hasEnrollements = enrollements is { Count: > 0 };
			var subjectDTOs = new List<SubjectDTO>(subjects.Count);
			foreach (SubjectItemDTO s in subjects)
			{
				float progress = 0f;
				if (s.LessonsCount > 0 &&
					hasEnrollements &&
					enrollements!.TryGetValue(s.Id, out var enrollment))
				{
					progress = (enrollment.Progress / s.LessonsCount) * 100f;
				}
				subjectDTOs.Add(new SubjectDTO
				{
					SubjectId = s.Id,
					SubjectName = s.SubjectName,
					lessonsCount = s.LessonsCount,
					deaf_mute = s.deaf_mute,
					progress = progress,
					AI_supported = s.AI_supported
				});
			}
			_logger.LogInformation("After DTO mapping: {ms}ms", sw.ElapsedMilliseconds);
			sw.Restart();

			bool hasResults = subjectDTOs.Count > 0;
			return new ResultDTO
			{
				Message = hasResults ? "Subjects retrieved successfully" : "No result",
				StatusCode = hasResults ? 200 : 404,
				result = hasResults ? subjectDTOs : null
			};
		}

		public async Task<ResultDTO> ViewSubjectAsync(Guid sid, bool all, CancellationToken cancellationToken)
		{
			Subject subject;
			if (all)
				subject = await _subjectRepository.GetSubjectWithRelationsAsync(sid, cancellationToken);
			else
				subject = await _subjectRepository.GetEntityAsync<Subject>(s => s.Id == sid, cancellationToken: cancellationToken);

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
			List<Submission> submissions = subject?.Submissions?.Where(s => s.LessonID == null).ToList();
			decimal AvgerageGrades = 0;
			int failedSubmissions = 0;
			int passedSubmissions = 0;
			if (submissions.Count != 0)
			{
				AvgerageGrades = submissions.Select(s => s.Percentage).Average();
				failedSubmissions = submissions.Where(s => !s.Passed && s.LessonID == null).Count();
				passedSubmissions = submissions.Where(s => s.Passed && s.LessonID == null).Count();
			}

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

			SubjectContent subjectContent = await _subjectRepository.GetSubjectContentAsync(subjectid, cancellationToken);
			if (subjectContent != null)
			{
				if (!await _cloudinary.DeleteAsync($"subjects/{subjectid}", folder: true, cancellationToken: cancellationToken))
				{
					return new ResultDTO
					{
						Message = "Failed to delete subject media from cloud storage",
						StatusCode = 500
					};
				}
			}

			int res = await _subjectRepository.DeleteSubject(subject, cancellationToken);

			if (res != 0)
				await BustSubjectListCache();

			return new ResultDTO
			{
				Message = res != 0 ? "Subject deleted successfully" : "Failed to delete subject",
				StatusCode = res != 0 ? 200 : 500
			};
		}

		// -------------------------------------------------------------------------
		// Vocabulary
		// -------------------------------------------------------------------------

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

		// -------------------------------------------------------------------------
		// Helpers
		// -------------------------------------------------------------------------

		public async Task<bool> updateCountAsync(Guid sid, CancellationToken cancellationToken)
		{
			
			bool res = await _subjectRepository.updateLessonCountAsync(sid, cancellationToken);
			if(res)
			{
				await BustSubjectListCache();
				//await GetAllSubjectsCachedAsync(cancellationToken);
			}
			return res;
		}

		public async Task<bool> IsSubjectExist(string? subjectName = "", bool? deaf_mute = false, Guid? subjectId = null, CancellationToken cancellationToken = default)
		{
			var allSubjects = await GetAllSubjectsCachedAsync(cancellationToken);
			return allSubjects.Any(s =>
				(!subjectId.HasValue || s.Id == subjectId.Value) ||
				(!string.IsNullOrWhiteSpace(subjectName) &&
					s.SubjectName.Equals(subjectName, StringComparison.OrdinalIgnoreCase)) ||
				(deaf_mute.HasValue && s.deaf_mute == deaf_mute.Value)
			);
			//return await _subjectRepository.IsSubjectExistAsync(subjectName, deaf_mute, subjectId, cancellationToken);
		}

		public async Task<ResultDTO> ViewDictionary(Guid sid,CancellationToken cancellationToken)
		{
			Vocabulary vocabulary = await _subjectRepository.GetVocabularyAsync(sid, cancellationToken);
			VocabDTO vocabDTO = new VocabDTO
			{
				Words = vocabulary.wordItems.Select(w => new WordDTO
				{
					word = w.Word,
					url = w.ImagePath
				}).ToList()
			};
			if (vocabDTO == null)
				return new ResultDTO
				{
					Message = $"{vocabDTO.Words.Count} words were found",
					StatusCode = 200,
					result = vocabDTO
				};
			else
			{
				return new ResultDTO
				{
					Message = "Invalid Subject Id",
					StatusCode = 400
				};
			}
		}


		private List<SubjectItemDTO> GetSubjectsByIds(IEnumerable<Guid> ids,List<SubjectItemDTO> subjects)
		{
			return subjects
				.Where(s => ids.Contains(s.Id))
				.ToList();
		}

		private List<SubjectItemDTO> GetSubjectsFiltered(List<SubjectItemDTO> subjects,bool? deafMute, IEnumerable<Guid>? excludeIds = null)
		{
			var query = subjects.AsQueryable();

			if (deafMute.HasValue)
			{
				query = query.Where(s => s.deaf_mute == deafMute.Value);
			}

			if (excludeIds != null && excludeIds.Any())
			{
				query = query.Where(s => !excludeIds.Contains(s.Id));
			}

			return query.ToList();
		}

		// -------------------------------------------------------------------------
		// Cache helpers
		// -------------------------------------------------------------------------
		private async Task<List<SubjectItemDTO>> GetAllSubjectsCachedAsync(CancellationToken cancellationToken)
		{
			// L1 — MemoryCache, nanoseconds, no network
			if (_memoryCache.TryGetValue(ALL_SUBJECTS_KEY, out List<SubjectItemDTO> allSubjects))
				return allSubjects;

			// L2 — Redis
			allSubjects = await _redisServices.getDeserialized<List<SubjectItemDTO>>(ALL_SUBJECTS_KEY);
			if (allSubjects != null)
			{
				_memoryCache.Set(ALL_SUBJECTS_KEY, allSubjects, TimeSpan.FromMinutes(5));
				return allSubjects;
			}

			await _cacheLock.WaitAsync(cancellationToken);
			try
			{
				// Double-check inside lock
				if (_memoryCache.TryGetValue(ALL_SUBJECTS_KEY, out allSubjects))
					return allSubjects;

				allSubjects = await _redisServices.getDeserialized<List<SubjectItemDTO>>(ALL_SUBJECTS_KEY);
				if (allSubjects != null)
				{
					_memoryCache.Set(ALL_SUBJECTS_KEY, allSubjects, TimeSpan.FromMinutes(5));
					return allSubjects;
				}

				// L3 — DB, only one request reaches here at a time
				List<Subject> subjects = await _subjectRepository.GetAllSubjectsAsync(cancellationToken);
				allSubjects = subjects.Select(s => new SubjectItemDTO
				{
					Id = s.Id,
					SubjectName = s.Name,
					deaf_mute = s.deaf_mute,
					LessonsCount = s.LessonCount,
					AI_supported = s.AI_supported
				}).ToList();

				await _redisServices.storeSerialized<List<SubjectItemDTO>>(ALL_SUBJECTS_KEY, allSubjects, TimeSpan.FromHours(24));
				_memoryCache.Set(ALL_SUBJECTS_KEY, allSubjects, TimeSpan.FromMinutes(5));
				return allSubjects;
			}
			finally
			{
				_cacheLock.Release();
			}
		}

		private async Task BustSubjectListCache()
		{
			_memoryCache.Remove(ALL_SUBJECTS_KEY); // clear L1
			await _redisServices.delete(ALL_SUBJECTS_KEY); // clear L2
		}

		//private async Task<List<SubjectItemDTO>> GetAllSubjectsCachedAsync(CancellationToken cancellationToken)
		//{
		//	var allSubjects = await _redisServices.getDeserialized<List<SubjectItemDTO>>(ALL_SUBJECTS_KEY);
		//	if (allSubjects != null)
		//		return allSubjects;

		//	await _cacheLock.WaitAsync(cancellationToken);
		//	try
		//	{
		//		// Double-check inside lock
		//		allSubjects = await _redisServices.getDeserialized<List<SubjectItemDTO>>(ALL_SUBJECTS_KEY);
		//		if (allSubjects != null)
		//			return allSubjects;

		//		// DB — only one request reaches here at a time
		//		List<Subject> subjects = await _subjectRepository.GetAllSubjectsAsync(cancellationToken);
		//		allSubjects = subjects.Select(s => new SubjectItemDTO
		//		{
		//			Id = s.Id,
		//			SubjectName = s.Name,
		//			deaf_mute = s.deaf_mute,
		//			LessonsCount = s.LessonCount
		//		}).ToList();
		//		await _redisServices.storeSerialized<List<SubjectItemDTO>>(ALL_SUBJECTS_KEY, allSubjects, TimeSpan.FromHours(24));
		//		return allSubjects;
		//	}
		//	finally
		//	{
		//		_cacheLock.Release();
		//	}
		//}

		//private async Task BustSubjectListCache()
		//{
		//	await _redisServices.delete(ALL_SUBJECTS_KEY);
		//}
	}
}