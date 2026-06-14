using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.Extensions.Logging;

namespace grad.Application.LessonFeatures.Services
{
	public class LessonServices : ILessonServices
	{
		private readonly ICloudinaryServices _cloudinaryServices;
		private readonly ISubjectServices _subjectServices;
		private readonly IlessonRepository _lessonRepository;
		private readonly IPerquisiteServices _perquisiteServices;
		private readonly ILogger<LessonServices> _logger;
		//private readonly IUowServices _Uow;

		public LessonServices(ICloudinaryServices cloudinaryServices, ISubjectServices subjectServices, IlessonRepository repository,IPerquisiteServices perquisiteServices, ILogger<LessonServices> logger)
		{
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_lessonRepository = repository ?? throw new ArgumentNullException(nameof(repository));
			_perquisiteServices = perquisiteServices ?? throw new ArgumentNullException(nameof(perquisiteServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			//_Uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<ResultDTO> AddLesson(AddLessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			LessonContent Nlesson = new LessonContent
			{
				Title = lessonDTO.Title,
				Description = lessonDTO.Description,
				Perquisite = null,
				PerquisiteType = PerquisiteType.None,
				Next = null,
			};

			int res = await _lessonRepository.addLesson(Nlesson, lessonDTO.SubjectId, Nlesson.Id, cancellationToken);
			if (res == 0)
			{
				return new ResultDTO
				{
					Message = "Lesson already exists or subject not found",
					StatusCode = 409
				};
			}
			
			if(lessonDTO.PerquisiteType != PerquisiteType.None)
			{
				res = await _perquisiteServices.UpdatePerquisite(lessonDTO.SubjectId,Nlesson.Id,PerquisiteType.Lesson,Nid:lessonDTO.Perquisite,NType:lessonDTO.PerquisiteType,Create: true,cancellationToken: cancellationToken);
				if (res <= 0)
				{
					await _lessonRepository.DeleteLesson(lessonDTO.SubjectId, Nlesson, cancellationToken);
					return new ResultDTO
					{
						Message = $"Perquisite {Nlesson.Perquisite.ToString()} not found, lesson was not added",
						StatusCode = 404
					};
				}
			}
			await _subjectServices.updateCountAsync(lessonDTO.SubjectId, cancellationToken);
			return new ResultDTO
			{
				Message = "Lesson was added successfully",
				StatusCode = 201
			};
		}

		public async Task<ResultDTO> UploadVideo(VideoDTO videoDTO, CancellationToken cancellationToken)
		{

			LessonContent lessonContent = await _lessonRepository.viewLesson(videoDTO.subjectID, videoDTO.LId, cancellationToken);
			if (lessonContent == null)
			{
				return new ResultDTO
				{
					Message = "Lesson not found",
					StatusCode = 404
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
					string directory = $"subjects/{videoDTO.subjectID}/lessonContent/{lessonContent.Id}/Videos/";
					string videoUrl = await _cloudinaryServices.UploadVideoAsync(videoDTO.VideoFile,directory, $"{lesson.Id}", cancellationToken);
					if(!string.IsNullOrEmpty(videoUrl))
					{
						lesson.VideoPath = videoUrl;
						lessonContent.Videos.Add(lesson);
					}
					else
					{
						return new ResultDTO
						{
							Message = $"Failed to upload video",
							StatusCode = 400
						};
					}

					int res = await _lessonRepository.uploadVideo(videoDTO.subjectID, lessonContent, cancellationToken);
					if (res == 0)
					{
						return new ResultDTO { Message = "Something went wrong while updating the lesson with videos", StatusCode =500 };
					}
					else
					{
						return new ResultDTO
						{
							Message = "Video uploaded and lesson updated successfully",
							StatusCode = 200
						};
					}
				}
				else
				{
					return new ResultDTO
					{
						Message = "Failed To upload, please try again",
						StatusCode = 400
					};
				}
			}
		}

		public async Task<ResultDTO> DeleteVideo(VideoDTO videoDTO, CancellationToken cancellationToken)// delete need to be updated to handle the next
		{
			
			if (videoDTO == null)
			{
				return new ResultDTO
				{
					Message = "video not found",
					StatusCode = 404
				};
			}
			else
			{
				int res = await _lessonRepository.DeleteVideo(videoDTO.subjectID, videoDTO.LId, videoDTO.VId, cancellationToken);
				if(res == 0)
				{
					return new ResultDTO
					{
						StatusCode = 400,
						Message = "failed to delete either the video is removed or not found"
					};
				}
				else
				{
					string directory = $"subjects/{videoDTO.subjectID}/lessonContent/{videoDTO.LId}/Videos/{videoDTO.VId}";
					bool videoDeleted = await _cloudinaryServices.DeleteAsync(directory, true);
					if (!videoDeleted)
					{
						return new ResultDTO { Message = "Failed to delete video from cloud storage", StatusCode =500 };
					}
					else
					{
						return new ResultDTO { Message = "Video deleted successfully", StatusCode = 200 };
					}
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
		//				return new ResultDTO { Message = "Something went wrong while updating the lesson with videos", StatusCode =500 };
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
		//			StatusCode = 409
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
		//				return new ResultDTO { Message = "Something went wrong", StatusCode =500 }; 
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
		//				return new ResultDTO { Message = "Something went wrong", StatusCode =500 };
		//			}
		//			else
		//			{
		//				return new ResultDTO
		//				{
		//					Message = "lesson was added successfully",
		//					StatusCode = 201
		//				};
		//			}
		//		}
		//	}
		//}


		public async Task<List<LessonContentDTO>> ViewLessons(Guid sid, CancellationToken cancellation)
		{
			//sid = sid.Trim();

			IEnumerable<LessonContent> lessons = await _lessonRepository.viewLessons(sid, cancellation);
			if (lessons == null || lessons.Count() == 0)
				return new();
			List<LessonContentDTO> lessonContentDTOs = new List<LessonContentDTO>();
			foreach (LessonContent l in lessons)
			{
				lessonContentDTOs.Add(new LessonContentDTO
				{
					Id = l.Id,
					SubjectId = sid,
					Title = l.Title,
					Description = l.Description,
					VideosCount = l.Videos.Count,
					NextType = l.NextType,
					Nlid = l.Next,
					Plid = l.Perquisite,
					PreviousType = l.PerquisiteType,
					locked = l.Perquisite != null ? true : false
				});
			}
			return lessonContentDTOs;	
		}


		public async Task<ResultDTO> viewLesson(LessonContentDTO lessonContentDTO,bool completed = false,CancellationToken cancellationToken = default)
		{

			LessonContent lesson = await _lessonRepository.viewLesson(lessonContentDTO.SubjectId, lessonContentDTO.Id, cancellationToken);
			if (lesson == null)
			{
				return new ResultDTO
				{
					Message = "Lesson not found",
					StatusCode = 404
				};
			}
			lessonContentDTO.Title = lesson.Title;
			lessonContentDTO.Description = lesson.Description;
			lessonContentDTO.VideosCount = lesson.Videos.Count;
			lessonContentDTO.completed = completed;
			if(completed)
			{
				lessonContentDTO.Nlid = lesson.Next;
				lessonContentDTO.NextType = lesson.NextType;
			}

			if (lesson.Level != null)
			{
				lessonContentDTO.Levels = new LevelDTO
				{
					ID = lesson.Level.ID,
					levelDifficulty = lesson.Level.levelDifficulty,
					Name = lesson.Level.Name,
					Lid = lesson.Id,
					Sid = lessonContentDTO.SubjectId
				};
			}
			List<VideoDTO> videoDTOs = new List<VideoDTO>();
			foreach (Video l in lesson.Videos)
			{
				videoDTOs.Add(new VideoDTO
				{
					VId = l.Id,
					LId = lesson.Id,
					subjectID = lessonContentDTO.SubjectId,
					Description = l.Description,
					ReleaseDate = l.ReleaseDate,
					Title = l.Title,
					videoUrl = l.VideoPath
				});
			}
			lessonContentDTO.Videos = videoDTOs;
			return new ResultDTO
			{
				Message = "Lesson found",
				result = lessonContentDTO,
				StatusCode = 200
			};
		}

		public async Task<ResultDTO> editLesson(EditLessonDTO lessonDTO, CancellationToken cancellationToken) // needs to be updated
		{
			LessonContent lesson = await _lessonRepository.viewLesson(lessonDTO.SubjectId, lessonDTO.Lid, cancellationToken);
			if (lesson == null)
				return new ResultDTO
				{
					Message = "lessson wasnt found",
					StatusCode = 400
				};

			bool changed = false;
			if (!string.IsNullOrEmpty(lessonDTO.Title))
			{
				lesson.Title = lessonDTO.Title;
				changed = true;
			}
			
			if (lessonDTO.Description != null)
			{
				lesson.Description = lessonDTO.Description;
				changed = true;
			}

			int updated = await _perquisiteServices.UpdatePerquisite(lessonDTO.SubjectId, lesson.Id, PerquisiteType.Lesson, lesson.Perquisite, lesson.PerquisiteType, lessonDTO.Perquisite, lessonDTO.PerquisiteType, false, cancellationToken);

			switch(updated)
			{
				case -1:
					changed = false;
					break;
				case -2:
					return new ResultDTO
					{
						Message = "Failed to update Perquisite, No changes were made",
						StatusCode = 500
					};
				case -3:
					return new ResultDTO
					{
						Message = "Lesson cant be a perquisite of itself",
						StatusCode = 400
					};
				case -4:
					return new ResultDTO
					{
						Message = $"this {lessonDTO.PerquisiteType.ToString()} can't be a perquisite because this lesson is already a perquisite of that {lessonDTO.PerquisiteType.ToString()}",
						StatusCode = 400
					};
				case 0:
					return new ResultDTO
					{
						Message = $"this {lessonDTO.PerquisiteType.ToString()} wasnt found, No changes were made",
						StatusCode = 400
					};
				default:
					changed = true;
					lesson.Perquisite = lessonDTO.PerquisiteType  != PerquisiteType.None ? lessonDTO.Perquisite : null;
					lesson.PerquisiteType = lessonDTO.PerquisiteType;
					break;
			}
			if (!changed)
			{
				return new ResultDTO
				{
					Message = "No changes were made.",
					StatusCode = 200
				};
			}

			int res = await _lessonRepository.editLesson(lessonDTO.SubjectId, lesson, cancellationToken);
			return new ResultDTO
			{
				Message = res > 0 ? "Lesson updated successfully" : "No changes were made.",
				StatusCode = 200
			};
		}

		public async Task<ResultDTO> DeleteLesson(DeleteLessonDTO deleteLesson, CancellationToken cancellationToken)
		{
			LessonContent lesson = await _lessonRepository.viewLesson(deleteLesson.SubjectId, deleteLesson.Lid, cancellationToken);
			if (lesson == null)
			{
				return new ResultDTO { Message = "Lesson not found", StatusCode = 404 };
			}
			else
			{
				string directory = $"uploads/subjects/{deleteLesson.SubjectId}/lessonContent/{lesson.Id}";
				bool videoDeleted = await _cloudinaryServices.DeleteAsync(directory, true, true);
				if (!videoDeleted)
				{
					return new ResultDTO { Message = "Failed to delete video from cloud storage", StatusCode = 500 };
				}
				int result = 0;
				result = await _perquisiteServices.removeDependency(deleteLesson.SubjectId, lesson, null , cancellationToken);
				if (result != 1)
				{
					return new ResultDTO { Message = "Failed to update perquisites, lesson was not deleted", StatusCode = 500 };
				}
				result = await _lessonRepository.DeleteLesson(deleteLesson.SubjectId, lesson, cancellationToken);
				if (result == 0) 
				{ 
					return new ResultDTO { Message = "Something went wrong", StatusCode = 500 }; 
				}
				else
				{
					await _subjectServices.updateCountAsync(deleteLesson.SubjectId, cancellationToken);
					return new ResultDTO { Message = "Lesson deleted successfully", StatusCode = 200 };
				}
			}
		}
	}
}	
