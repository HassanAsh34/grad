using System.Collections;
using Google.Cloud.Firestore;
using grad.Data;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SharpCompress.Common;
using static Grpc.Core.Metadata;

namespace grad.Services
{
	public class SubjectServices : ISubjectServices
	{
		private readonly IRepository _repository;
		private readonly IUowServices _uowServices;
		private readonly IMongoCollection<SubjectContent> _subjects;

		public SubjectServices(IRepository repository, IUowServices uowServices, MongoDBContext context)
		{
			_repository = repository ?? throw new ArgumentNullException();
			_subjects = context.Subjects ?? throw new ArgumentNullException();
			_uowServices = uowServices ?? throw new ArgumentNullException();
		}

		public async Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			if (await IsSubjectExist(subjectName: subject.SubjectName,deaf_mute: subject.deaf_mute,cancellationToken: cancellationToken))
			{
				return new ResultDTO
				{
					Message = "Subject already exists",
					StatusCode = StatusCodes.Status409Conflict
				};
			}
			else
			{
				Subject sub = new Subject
				{
					Name = subject.SubjectName,
					deaf_mute = subject.deaf_mute
				};
				_repository.CreateEntityAsync<Subject>(sub, cancellationToken);
				int res = await _uowServices.SaveChangesAsync();
				if(res != 0)
				{
					SubjectContent subjectContent = new SubjectContent
					{
						Id = sub.Id
					};
					try 
					{
						await _subjects.InsertOneAsync(subjectContent);
					}
					catch(Exception e)
					{
						Console.WriteLine(e);
						_repository.DeleteEntityAsync<Subject>(sub);
						res = await _uowServices.SaveChangesAsync();
						if (res != 0)
							return new ResultDTO
							{
								Message = "something went wrong with mongo db",
								StatusCode = 500
							};
						else
							return new ResultDTO
							{
								Message = "failed to remove the subject",
								StatusCode = 500
							};
					}
					;
				}
				return new ResultDTO
				{
					Message = res != 0 ? "Subject added successfully" : "Failed to add subject",
					StatusCode = res != 0 ? StatusCodes.Status201Created : StatusCodes.Status500InternalServerError,
					result = res
				};
			}
		}



		public Task<ResultDTO> UpdateSubject(SubjectDTO subject,string SubjectName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<ResultDTO> ViewSubjectsAsync(CancellationToken cancellationToken)
		{
			IEnumerable<Subject> subjects =await _repository.GetEntitiesAsync<Subject>(cancellationToken: cancellationToken);
			IEnumerable<SubjectDTO> subjectDTOs = subjects.Select(s => new SubjectDTO
			{
				SubjectId = s.Id,
				SubjectName = s.Name,
				deaf_mute = s.deaf_mute
			});
			return new ResultDTO
			{
				Message = subjectDTOs.Count() > 0 ? "Subjects retrieved successfully" : "No result",
				StatusCode = subjectDTOs.Count() >0 ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
				result = subjectDTOs 
			};
		}



		public async Task<ResultDTO> ViewSubjectAsync(string sid, CancellationToken cancellationToken)
		{
			Subject? res = await _repository.GetEntityAsync<Subject>(s =>s.Id.ToLower().Equals(sid) , q => q.Include(s => s.Teachers).Include(s => s.Students), cancellationToken: cancellationToken);
			if (res != null)
			{
				//var lessonRef = _firestoreDb._Db.Collection("Subjects").Document(res.Id).Collection("Lessons");
				SubjectContent subjectContent = await _subjects.Find(s => s.Id.Equals(res.Id)).FirstOrDefaultAsync();
				Console.WriteLine(subjectContent);
				int lessonsCount = subjectContent.Lessons != null ? subjectContent.Lessons.Count():0;
				int levelsCount = subjectContent.Levels != null ? subjectContent.Levels.Count() : 0;

				//int lessonsCount = (await lessonRef.GetSnapshotAsync(cancellationToken)).Count;
				SubjectDTO subjectDTO = new SubjectDTO
				{
					SubjectId = res.Id,
					SubjectName = res.Name,
					deaf_mute = res.deaf_mute,
					studentsCount = res.Students != null ? res.Students.Count() : 0,
					teachersCount = res.Teachers != null ? res.Teachers.Count() : 0,
					lessonsCount = lessonsCount,
					levelsCount = lessonsCount
				};
				return new ResultDTO
				{
					Message = "Subject retrieved successfully",
					StatusCode = StatusCodes.Status200OK,
					result = subjectDTO
				};
			}
			else
			{
				return new ResultDTO
				{
					Message = "Subject not found",
					StatusCode = StatusCodes.Status404NotFound
				};
			}
		}

		public Task<ResultDTO> RemoveSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<ResultDTO> AddLesson(LessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			if (lessonDTO.VideoFile.Length > 0 && lessonDTO.VideoFile != null)
			{
				string directory = Path.Combine("uploads", "subjects", $"{lessonDTO.subjectID}", "lessons");
				if (!File.Exists(directory))
					Directory.CreateDirectory(directory);

				lessonDTO.VideoPath = Path.Combine(directory, $"{lessonDTO.Title}.mp4");

				using var stream = new FileStream(lessonDTO.VideoPath, FileMode.Create);
				await lessonDTO.VideoFile.CopyToAsync(stream, cancellationToken);
			}
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, lessonDTO.subjectID), Builders<SubjectContent>.Filter.Not(
				Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Title == lessonDTO.Title)));
			Lesson lesson = new Lesson
			{
				Description = lessonDTO.Description,
				Title = lessonDTO.Title,
				VideoPath = lessonDTO.VideoPath
			};
			var update = Builders<SubjectContent>.Update.Push(s => s.Lessons,lesson);
			var res = await _subjects.UpdateOneAsync(filter, update);
			if(res.ModifiedCount == 0)
			{
				if (File.Exists(lesson.VideoPath))
				{
					File.Delete(lesson.VideoPath);
				}
				return new ResultDTO
				{
					Message = "Lesson already exists or subject not found",
					StatusCode = StatusCodes.Status409Conflict
				};
			}
			else
			{
				return new ResultDTO
				{
					Message = "lesson was added successfully",
					StatusCode = StatusCodes.Status201Created
				};
			}
		}


		public async Task<ResultDTO> ViewLessons(LessonDTO lesson,CancellationToken cancellation)
		{
			SubjectContent subjectContent = await _subjects.Find(s=>s.Id.ToLower().Equals(lesson.subjectID)).FirstOrDefaultAsync();
			if(subjectContent.Lessons == null)
				return new ResultDTO
				{
					Message = "No lessons were found",
					StatusCode = StatusCodes.Status204NoContent
				};
			else
			{
				List<Lesson> lessons = subjectContent.Lessons;
				List<LessonDTO> lessonDTOs = new List<LessonDTO>();
				lessons.ForEach(l =>
				{
					lessonDTOs.Add(new LessonDTO
					{
						Description = l.Description,
						Id = l.Id,
						ReleaseDate = l.ReleaseDate,
						Title = l.Title,
						VideoPath = l.VideoPath,
					});
				});

				return new ResultDTO
				{
					Message = $"{lessonDTOs.Count} lessons were found",
					result = lessonDTOs,
					StatusCode = StatusCodes.Status200OK
				};
			}
		}


		//{
		//	bool subjectExists = await IsSubjectExist(subjectId: lessonDTO.SubjectID, cancellationToken: cancellationToken);
		//	if (!subjectExists)
		//	{
		//		return new ResultDTO
		//		{
		//			StatusCode = StatusCodes.Status404NotFound,
		//			Message = "Subject not found"
		//		};
		//	}
		//	else
		//	{

		//		// Reference to the Lessons collection under the subject
		//		SubjectContent subjectContent = await _subjects.Find(s => s.Id.Equals(lessonDTO.SubjectID)).FirstOrDefaultAsync();

		//		// Check if a lesson with the same title exists (case-insensitive)
		//		List<Lesson> lessons = subjectContent.lessons;
		//		Lesson res = null;
		//		if (lessons != null && lessons.Count > 0)
		//		{
		//			res = lessons.FirstOrDefault(l => l.Title.ToLower() == lessonDTO.Title.ToLower());
		//		}
		//		if (res != null)
		//			return new ResultDTO
		//			{
		//				Message = "The lesson already exists",
		//				StatusCode = StatusCodes.Status409Conflict
		//			};
		//		else
		//		{
		//			if(lessonDTO.VideoFile.Length > 0 && lessonDTO.VideoFile != null)
		//			{
		//				string directory = Path.Combine("uploads", "subjects",$"{lessonDTO.SubjectID}","lessons");
		//				if (!File.Exists(directory))
		//					Directory.CreateDirectory(directory);

		//				lessonDTO.VideoPath = Path.Combine(directory, $"{lessonDTO.Title}.mp4");

		//				using var stream = new FileStream(lessonDTO.VideoPath, FileMode.Create);
		//				await lessonDTO.VideoFile.CopyToAsync(stream,cancellationToken);

		//				Lesson lesson = new Lesson
		//				{
		//					Title = lessonDTO.Title,
		//					Description = lessonDTO.Description,
		//					VideoPath = lessonDTO.VideoPath
		//				};



		//			}

		//			}
		//		}

		//		if (querySnap.Count > 0)
		//		{
		//			return new ResultDTO
		//			{
		//				StatusCode = StatusCodes.Status409Conflict,
		//				Message = "Lesson with the same title already exists"
		//			};
		//		}

		//		// Create the new lesson
		//		var newLesson = new Lesson
		//		{
		//			Title = lessonDTO.Title.ToLower(),
		//			Description = lessonDTO.Description
		//			// Add other fields like VideoUrl, Content, etc.
		//		};

		//		// Save to Firestore
		//		await lesRef.Document(newLesson.Id).SetAsync(newLesson, cancellationToken: cancellationToken);

		//		// ✅ Return success with the created lesson
		//		return new ResultDTO
		//		{
		//			StatusCode = StatusCodes.Status201Created,
		//			Message = "Lesson added successfully",
		//			result = newLesson
		//		};
		//	}
		//}


		public async Task<bool> IsSubjectExist (string ?subjectName = "", bool ?deaf_mute=false,string ?subjectId="",CancellationToken cancellationToken = default)
		{
			Subject? subject;
			if (subjectId.IsNullOrEmpty()&& !subjectName.IsNullOrEmpty())
				subject = await _repository.GetEntityAsync<Subject>(s => s.Name.ToLower().Equals(subjectName.ToLower()) && s.deaf_mute == deaf_mute, cancellationToken: cancellationToken);
			else
				subject = await _repository.GetEntityAsync<Subject>(s=> s.Id.ToLower().Equals(subjectId.ToLower()), cancellationToken: cancellationToken);
			return subject != null;
		}
	}
}
