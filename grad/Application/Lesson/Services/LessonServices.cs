using grad.Application.Lesson.DTOs;
using grad.Application.Common.DTOs;
using grad.Application.Common.Interfaces;
using grad.Application.lesson.DTOs;
using grad.Application.lesson.Interfaces;
using grad.Application.Lesson.DTOs;
using grad.Application.subject.Interfaces;
using grad.Domain.Model;
using grad.Infrastructure.Persistence;
using grad.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace grad.Application.lesson.Services
{
	public class LessonServices : ILessonServices
	{
		private readonly IMongoCollection<SubjectContent> _subjects;
		private readonly ICloudinaryServices _cloudinaryServices;
		private readonly ISubjectServices _subjectServices;
		private readonly IRepository _repository;
		private readonly IUowServices _Uow;

		public LessonServices(MongoDBContext context, ICloudinaryServices cloudinaryServices, ISubjectServices subjectServices, IRepository repository,IUowServices uow)
		{
			_subjects = context.Subjects ?? throw new ArgumentNullException(nameof(context));
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_Uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<ResultDTO> AddLesson(AddLessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, lessonDTO.SubjectId), Builders<SubjectContent>.Filter.Not(
				Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Title == lessonDTO.Title)));
			LessonContent lesson = new LessonContent
			{
				Title = lessonDTO.Title
			};
			var update = Builders<SubjectContent>.Update.Push(s => s.Lessons, lesson);
			var res = await _subjects.UpdateOneAsync(filter, update);
			if (res.ModifiedCount == 0)
			{
				return new ResultDTO
				{
					Message = "Lesson already exists or subject not found",
					StatusCode = StatusCodes.Status409Conflict
				};
			}
			else
			{
				await _subjectServices.IncreaseLessonCount(lessonDTO.SubjectId, cancellationToken);
				return new ResultDTO
				{
					Message = "lesson was added successfully",
					StatusCode = StatusCodes.Status201Created
				};
			}
		}

		public async Task<ResultDTO> UploadVideo(VideoDTO videoDTO, CancellationToken cancellationToken)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, videoDTO.subjectID), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == videoDTO.LId));
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
				LessonContent lessonContent = subjectContent.Lessons.FirstOrDefault(l => l.Id == videoDTO.LId);
				if (lessonContent == null)
				{
					return new ResultDTO
					{
						Message = "Lesson not found",
						StatusCode = StatusCodes.Status404NotFound
					};
				}
				else
				{
					if (videoDTO.VideoFile != null)
					{
						Video lesson = new Video
						{
							Description = videoDTO.Description,
							Title = videoDTO.Title,
							Uploaded_by = videoDTO.Uploaded_by
						};
						string directory = $"subjects/{subjectContent.Id}/lessonContent/{lessonContent.Id}/Videos/";
						string videoUrl = await _cloudinaryServices.UploadVideoAsync(videoDTO.VideoFile, directory, $"{lesson.Id}.mp4", cancellationToken);
						if (!videoUrl.IsNullOrEmpty())
						{
							lesson.VideoPath = videoUrl;
							lessonContent.Videos.Add(lesson);
						}
						else
						{
							return new ResultDTO
							{
								Message = $"Failed to upload video",
								StatusCode = StatusCodes.Status400BadRequest
							};
						}
						var update = Builders<SubjectContent>.Update.Set("Lessons.$.Videos",lessonContent.Videos);
						var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
						if (result.ModifiedCount == 0)
						{
							return new ResultDTO { Message = "Something went wrong while updating the lesson with videos", StatusCode = StatusCodes.Status500InternalServerError };
						}
						else
						{
							return new ResultDTO
							{
								Message = "Video uploaded and lesson updated successfully",
								StatusCode = StatusCodes.Status200OK
							};
						}
					}
					else
					{
						return new ResultDTO
						{
							Message = "Failed To upload, please try again",
							StatusCode = StatusCodes.Status400BadRequest
						};
					}
				}
			}
		}

		public async Task<ResultDTO> DeleteVideo(VideoDTO videoDTO, CancellationToken cancellationToken)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, videoDTO.subjectID), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == videoDTO.LId));
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
				LessonContent lessonContent = subjectContent.Lessons.FirstOrDefault(l => l.Id == videoDTO.LId);
				if (lessonContent == null)
				{
					return new ResultDTO
					{
						Message = "Lesson not found",
						StatusCode = StatusCodes.Status404NotFound
					};
				}
				else
				{
					Video lesson = lessonContent.Videos.FirstOrDefault(l => l.Id == videoDTO.VId);
					if (lesson == null)
					{
						return new ResultDTO
						{
							Message = "Video not found",
							StatusCode = StatusCodes.Status404NotFound
						};
					}
					else
					{
						string directory = $"subjects/{subjectContent.Id}/lessonContent/{lessonContent.Id}/Videos/{lesson.Id}";
						bool videoDeleted = await _cloudinaryServices.DeleteAsync(directory, true);
						if (!videoDeleted)
						{
							return new ResultDTO { Message = "Failed to delete video from cloud storage", StatusCode = StatusCodes.Status500InternalServerError };
						}
						else
						{
							lessonContent.Videos.Remove(lesson);
							var update = Builders<SubjectContent>.Update.Set("Lessons.$.Lessons", lessonContent.Videos);
							var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
							if (result.ModifiedCount == 0)
							{
								return new ResultDTO { Message = "Something went wrong", StatusCode = StatusCodes.Status500InternalServerError };
							}
							else
							{
								return new ResultDTO { Message = "Video deleted successfully", StatusCode = StatusCodes.Status200OK };
							}
						}
					}
				}
			}
		}

		public async Task<ResultDTO> DeleteLesson(LessonContentDTO lessonContent, CancellationToken cancellationToken)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, lessonContent.SubjectId), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lessonContent.Id));
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
				LessonContent l = subjectContent.Lessons.FirstOrDefault(l => l.Id == lessonContent.Id);
				foreach (Video lesson in l.Videos)
				{
					string directory = $"subjects/{subjectContent.Id}/lessonContent/{l.Id}/Videos/{lesson.Id}";
					bool videoDeleted = await _cloudinaryServices.DeleteAsync(directory, true);
					if (!videoDeleted)
					{
						return new ResultDTO { Message = "Failed to delete videos from cloud storage", StatusCode = StatusCodes.Status500InternalServerError };
					}
					else
					{
						continue;
					}
				}
				var result = await _subjects.DeleteOneAsync(filter, cancellationToken);
				if (result.DeletedCount == 0)
				{
					return new ResultDTO { Message = "Something went wrong", StatusCode = StatusCodes.Status500InternalServerError };
				}
				else
				{
					return new ResultDTO { Message = "Video deleted successfully", StatusCode = StatusCodes.Status200OK };
				}
			}
		}


		//public async Task<ResultDTO> updateVideo(VideoDTO videoDTO, CancellationToken cancellationToken)	

		//public async Task<ResultDTO> UploadVideos(UploadVideoDTO addLessonDTO, CancellationToken cancellationToken)
		//{
		//	var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, addLessonDTO.SubjectID), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == addLessonDTO.Cid));
		//	SubjectContent subjectContent = await _subjects.Find(filter).FirstOrDefaultAsync();
		//	if (subjectContent == null || subjectContent.Lessons.Count == 0)
		//	{
		//		return new ResultDTO
		//		{
		//			Message = "Lesson not found",
		//			StatusCode = StatusCodes.Status404NotFound
		//		};
		//	}
		//	else
		//	{
		//		LessonContent lessonContent = subjectContent.Lessons.FirstOrDefault(l => l.Id == addLessonDTO.Cid);
		//		if(lessonContent == null)
		//		{
		//			return new ResultDTO
		//			{
		//				Message = "Lesson not found",
		//				StatusCode = StatusCodes.Status404NotFound
		//			};
		//		}
		//		if (addLessonDTO.VideoFiles.Count == addLessonDTO.lesson.Count )
		//		{
		//			for(int i = 0; i < addLessonDTO.VideoFiles.Count; i++)
		//			{
		//				Lesson lesson = new Lesson
		//				{
		//					Description = lessonContent.Lessons[i].Description,
		//					Title = lessonContent.Lessons[i].Title
		//				};
		//				string directory = $"subjects/{addLessonDTO.SubjectID}/lessonContent/{lessonContent.Id}/Videos/";
		//				string videoUrl = await _cloudinaryServices.UploadVideoAsync(addLessonDTO.VideoFiles[i], directory, $"{lesson.Id}.mp4", cancellationToken);
		//				if (!videoUrl.IsNullOrEmpty())
		//				{
		//					lesson.VideoPath = videoUrl;
		//					lessonContent.Lessons.Add(lesson);
		//				}
		//				else
		//				{
		//					return new ResultDTO
		//					{
		//						Message = $"Failed to upload video: {videoFile.FileName}",
		//						StatusCode = StatusCodes.Status400BadRequest
		//					};
		//				}
		//			}
		//			var update = Builders<SubjectContent>.Update.Set("Lessons.$.Lessons", lesson.Lessons);
		//			var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
		//			if (result.ModifiedCount == 0)
		//			{
		//				return new ResultDTO { Message = "Something went wrong while updating the lesson with videos", StatusCode = StatusCodes.Status500InternalServerError };
		//			}
		//			else
		//			{
		//				return new ResultDTO
		//				{
		//					Message = "Videos uploaded and lesson updated successfully",



		//public async Task<ResultDTO> AddLesson(LessonDTO lessonDTO, CancellationToken cancellationToken)
		//{
		//	var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, lessonDTO.subjectID), Builders<SubjectContent>.Filter.Not(
		//		Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Title == lessonDTO.Title)));
		//	Lesson lesson = new Lesson
		//	{
		//		Description = lessonDTO.Description,
		//		Title = lessonDTO.Title,
		//		VideoPath = lessonDTO.VideoPath
		//	};
		//	var update = Builders<SubjectContent>.Update.Push(s => s.Lessons, lesson);
		//	var res = await _subjects.UpdateOneAsync(filter, update);
		//	if (res.ModifiedCount == 0)
		//	{
		//		return new ResultDTO
		//		{
		//			Message = "Lesson already exists or subject not found",
		//			StatusCode = StatusCodes.Status409Conflict
		//		};
		//	}
		//	else
		//	{
		//		if (lessonDTO.VideoFile.Length > 0 && lessonDTO.VideoFile != null)
		//		{
		//			string directory = $"subjects/{lessonDTO.subjectID}/lessons";
		//			//string directory = Path.Combine("uploads", "subjects", $"{lessonDTO.subjectID}", "lessons");

		//			//if (!File.Exists(directory))
		//			//	Directory.CreateDirectory(directory);
		//			lesson.VideoPath = await _cloudinaryServices.UploadVideoAsync(lessonDTO.VideoFile, directory, $"{lesson.Id}.mp4", cancellationToken);

		//			//lessonDTO.VideoPath = Path.Combine(directory, $"{lessonDTO.Title}.mp4");

		//			//using var stream = new FileStream(lessonDTO.VideoPath, FileMode.Create);
		//			//await lessonDTO.VideoFile.CopyToAsync(stream, cancellationToken);
		//		}
		//		if (lesson.VideoPath.IsNullOrEmpty())
		//		{
		//			update = Builders<SubjectContent>.Update.PullFilter(s => s.Lessons, l => l.Id == lesson.Id);
		//			var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken); 
		//			if (result.ModifiedCount == 0) 
		//			{
		//				return new ResultDTO { Message = "Something went wrong", StatusCode = StatusCodes.Status500InternalServerError }; 
		//			}
		//			else
		//			{
		//				return new ResultDTO
		//				{
		//					Message = "failed to uploud video, please try again",
		//					StatusCode = StatusCodes.Status400BadRequest
		//				};
		//			}
		//		}
		//		else
		//		{
		//			update = Builders<SubjectContent>.Update.Set("Lessons.$.VideoPath", lesson.VideoPath);
		//			var result = await _subjects.UpdateOneAsync(filter, update,cancellationToken: cancellationToken);
		//			if (result.ModifiedCount == 0)
		//			{
		//				return new ResultDTO { Message = "Something went wrong", StatusCode = StatusCodes.Status500InternalServerError };
		//			}
		//			else
		//			{
		//				return new ResultDTO
		//				{
		//					Message = "lesson was added successfully",
		//					StatusCode = StatusCodes.Status201Created
		//				};
		//			}
		//		}
		//	}
		//}


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
				List<LessonContent> lessons = subjectContent.Lessons;
				List<LessonContentDTO> lessonContentDTOs = new List<LessonContentDTO>();
				lessons.ForEach(l =>
				{
					lessonContentDTOs.Add(new LessonContentDTO
					{
						Id = l.Id,
						SubjectId = subjectContent.Id,
						Title = l.Title,
						VideosCount = l.Videos.Count
					});
				});

				return new ResultDTO
				{
					Message = $"{lessonContentDTOs.Count} lessons were found",
					result = lessonContentDTOs,
					StatusCode = StatusCodes.Status200OK
				};
			}
		}

		public async Task<ResultDTO> viewLesson(LessonContentDTO lessonContentDTO, CancellationToken cancellationToken)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, lessonContentDTO.SubjectId), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lessonContentDTO.Id));
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
				LessonContent lesson = subjectContent.Lessons.FirstOrDefault(l => l.Id == lessonContentDTO.Id);
				lessonContentDTO.Title = lesson.Title;
				lessonContentDTO.VideosCount = lesson.Videos.Count;
				if(lesson.Exercises != null)
					lessonContentDTO.Exercises = lesson.Exercises;
				foreach (Video l in lesson.Videos)
				{
					lessonContentDTO.Videos.Add(new VideoDTO
					{
						VId = l.Id,
						LId = lesson.Id,
						subjectID = subjectContent.Id,
						Description = l.Description,
						ReleaseDate = l.ReleaseDate,
						Title = l.Title,
						videoUrl = l.VideoPath
					});
				}
				return new ResultDTO
				{
					Message = "Lesson found",
					result = lessonContentDTO,
					StatusCode = StatusCodes.Status200OK
				};
			}
		}


		public async Task<ResultDTO> completeLesson(CompletelessonDTO Completelesson, CancellationToken cancellationToken)
		{
			Enrollement application = await _repository.GetEntityAsync<Enrollement>(e => e.STUFK == Completelesson.uid && e.SUBFK == Completelesson.Sid,q=>q.Include(e=>e.studentProgresses),cancellationToken);
			if(application == null)
				return new ResultDTO
				{
					Message = "Action can't be done",
					StatusCode = StatusCodes.Status403Forbidden
				};
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, application.SUBFK), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == Completelesson.Lid));
			SubjectContent subjectContent = await _subjects.Find(filter).FirstOrDefaultAsync();
			if (subjectContent == null || subjectContent.Lessons.Count == 0)
			{
				return new ResultDTO
				{
					Message = "Something went wrong please try again",
					StatusCode = StatusCodes.Status400BadRequest
				};
			}
			else
			{
				LessonContent lesson = subjectContent.Lessons.FirstOrDefault(l => l.Id == Completelesson.Lid);
				if(lesson == null)
				{
					return new ResultDTO
					{
						Message = "Something went wrong please try again",
						StatusCode = StatusCodes.Status400BadRequest
					};
				}
				else
				{
					if(application.studentProgresses.FirstOrDefault(s => s.lid == Completelesson.Lid) != null)
					{
						return new ResultDTO
						{
							Message = "Lesson already marked as completed",
							StatusCode = StatusCodes.Status400BadRequest
						};
					}
					else
					{
						StudentProgress studentProgresses = new StudentProgress
						{
							lid = lesson.Id,
							Eid_fk = application.Id,
							Completed_At = DateTime.UtcNow
						};
						_repository.CreateEntityAsync(studentProgresses);
					}
					int result = await _Uow.SaveChangesAsync();
					if (result == 0)
					{
						return new ResultDTO { Message = "Something went wrong", StatusCode = StatusCodes.Status500InternalServerError };
					}
					else
					{
						return new ResultDTO { Message = "Lesson marked as completed successfully", StatusCode = StatusCodes.Status200OK };
					}
				}
			}
		}

		public async Task<ResultDTO> editLesson(EditLessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, lessonDTO.SubjectId), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lessonDTO.Lid));
			SubjectContent subjectContent = await _subjects.Find(filter).FirstOrDefaultAsync();
			var updates = new List<UpdateDefinition<SubjectContent>>();
			if (!lessonDTO.Title.IsNullOrEmpty())
				updates.Add(
					Builders<SubjectContent>.Update.Set("Lessons.$.Title", lessonDTO.Title)
				);
			if (!updates.Any())
				return new ResultDTO
				{
					Message = "Nothing to update",
					StatusCode = StatusCodes.Status400BadRequest
				};
			var update = Builders<SubjectContent>.Update.Combine(updates);

			var result = await _subjects.UpdateOneAsync(
				filter,
				update,
				cancellationToken: cancellationToken
			);

			if (result.MatchedCount == 0)
			{
				return new ResultDTO
				{
					Message = "Lesson not found",
					StatusCode = StatusCodes.Status404NotFound
				};
			}

			return new ResultDTO
			{
				Message = "Lesson updated successfully",
				StatusCode = StatusCodes.Status200OK
			};
		}

		public async Task<ResultDTO> DeleteLesson(DeleteLessonDTO deleteLesson, CancellationToken cancellationToken)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, deleteLesson.SubjectId), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == deleteLesson.Lid));
			var lesson = await _subjects.Find(filter).FirstOrDefaultAsync();
			if (lesson == null)
			{
				return new ResultDTO { Message = "Lesson not found", StatusCode = StatusCodes.Status404NotFound };
			}
			else
			{
				string directory = $"uploads/subjects/{deleteLesson.SubjectId}/lessonContent/{lesson.Lessons.FirstOrDefault(l => l.Id == deleteLesson.Lid).Id}";
				bool videoDeleted = await _cloudinaryServices.DeleteAsync(directory, true,true);
				if (!videoDeleted)
				{
					return new ResultDTO { Message = "Failed to delete video from cloud storage", StatusCode = StatusCodes.Status500InternalServerError };
				}
				var update = Builders<SubjectContent>.Update.PullFilter(s => s.Lessons, l => l.Id == deleteLesson.Lid);
				var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken); 
				if (result.ModifiedCount == 0) 
				{ 
					return new ResultDTO { Message = "Something went wrong", StatusCode = StatusCodes.Status500InternalServerError }; 
				}
				else
				{
					//IEnumerable<StudentProgress> studentProgresses = await _repository.GetEntitiesAsync<StudentProgress>(s => s.lid == deleteLesson.Lid, cancellationToken: cancellationToken);
					//if (studentProgresses.Count() > 0)
					//{
					//	foreach (StudentProgress progress in studentProgresses)
					//	{
					//		_repository.DeleteEntityAsync(progress);
					//	}
					//	await _Uow.SaveChangesAsync();
					//}
					await _repository.DleteEntitiesAsync<StudentProgress>(filter: l=>l.Id == deleteLesson.Lid,cancellationToken: cancellationToken);
					return new ResultDTO { Message = "Lesson deleted successfully", StatusCode = StatusCodes.Status200OK };
				}
			}
		}
	}
}	
