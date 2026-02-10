using grad.Data;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace grad.Services
{
	public class LessonServices : ILessonServices
	{
		private readonly IMongoCollection<SubjectContent> _subjects;
		private readonly ICloudinaryServices _cloudinaryServices;

		public LessonServices(MongoDBContext context,ICloudinaryServices cloudinaryServices)
		{
			_subjects = context.Subjects ?? throw new ArgumentNullException(nameof(context));
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
		}

		public async Task<ResultDTO> AddLesson(LessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			if (lessonDTO.VideoFile.Length > 0 && lessonDTO.VideoFile != null)
			{
				string directory = $"subjects/{lessonDTO.subjectID}/lessons";
				//string directory = Path.Combine("uploads", "subjects", $"{lessonDTO.subjectID}", "lessons");

				//if (!File.Exists(directory))
				//	Directory.CreateDirectory(directory);
				lessonDTO.VideoPath = await _cloudinaryServices.UploadVideoAsync(lessonDTO.VideoFile, directory, $"{lessonDTO.Title}.mp4",cancellationToken);

				//lessonDTO.VideoPath = Path.Combine(directory, $"{lessonDTO.Title}.mp4");

				//using var stream = new FileStream(lessonDTO.VideoPath, FileMode.Create);
				//await lessonDTO.VideoFile.CopyToAsync(stream, cancellationToken);
			}
			if (lessonDTO.VideoPath.IsNullOrEmpty())
				return new ResultDTO
				{
					Message = "failed to uploud video, please try again",
					StatusCode = StatusCodes.Status400BadRequest
				};
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, lessonDTO.subjectID), Builders<SubjectContent>.Filter.Not(
				Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Title == lessonDTO.Title)));
			Lesson lesson = new Lesson
			{
				Description = lessonDTO.Description,
				Title = lessonDTO.Title,
				VideoPath = lessonDTO.VideoPath
			};
			var update = Builders<SubjectContent>.Update.Push(s => s.Lessons, lesson);
			var res = await _subjects.UpdateOneAsync(filter, update);
			if (res.ModifiedCount == 0)
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


		public async Task<ResultDTO> ViewLessons(Guid sid, CancellationToken cancellation)
		{
			//sid = sid.Trim();
			SubjectContent subjectContent = await _subjects.Find(s => s.Id
			== sid).FirstOrDefaultAsync();
			if (subjectContent.Lessons == null)
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
						videoUrl = l.VideoPath
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

		public async Task<ResultDTO> viewLesson(LessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, lessonDTO.subjectID), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lessonDTO.Id));
			SubjectContent subjectContent = await _subjects.Find(filter).FirstOrDefaultAsync();
			if (subjectContent == null || subjectContent.Lessons.Count == 0)
			{
				return new ResultDTO
				{
					Message = "Lesson not found",
					StatusCode = StatusCodes.Status404NotFound
				};
			}
			else
			{
				Lesson lesson = subjectContent.Lessons.FirstOrDefault(l => l.Id == lessonDTO.Id);
				lessonDTO.Description = lesson.Description;	
				lessonDTO.ReleaseDate = lesson.ReleaseDate;
				lessonDTO.Title = lesson.Title;
				lessonDTO.VideoPath = lesson.VideoPath;
				return new ResultDTO
				{
					Message = "Lesson found",
					result = lessonDTO,
					StatusCode = StatusCodes.Status200OK
				};
			}
		}
	}
}	
