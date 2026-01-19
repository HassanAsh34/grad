using Google.Cloud.Firestore;
using System.Collections;
using grad.DTO;
using grad.Interfaces;
using grad.Model;
using static Grpc.Core.Metadata;
using grad.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace grad.Services
{
	public class SubjectServices : ISubjectServices
	{
		private readonly IRepository _repository;
		private readonly FireStoreContext _firestoreDb;
		private readonly IUowServices _uowServices;

		public SubjectServices(IRepository repository,FireStoreContext fireStoreDb ,IUowServices uowServices)
		{
			_repository = repository ?? throw new ArgumentNullException();
			_firestoreDb = fireStoreDb ?? throw new ArgumentNullException();
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
				await _repository.CreateEntityAsync<Subject>(sub, cancellationToken);
				int res = await _uowServices.SaveChangesAsync();
				if(res != 0)
				{
					var Collection = _firestoreDb._Db.Collection("Subjects");
					DocumentReference reference = Collection.Document(sub.Id);
					await reference.SetAsync(new
					{
						subjectId = sub.Id,
						subjectName = sub.Name,
						deaf_mute = sub.deaf_mute
					});
				}
				return new ResultDTO
				{
					Message = res != 0 ? "Subject added successfully" : "Failed to add subject",
					StatusCode = res != 0 ? StatusCodes.Status201Created : StatusCodes.Status500InternalServerError,
					result = res
				};
			}
		}

		public Task<ResultDTO> RemoveSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
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

		public async Task<ResultDTO> ViewSubjectAsync(SubjectDTO subject, CancellationToken cancellationToken)
		{
			Subject? res = await _repository.GetEntityAsync<Subject>(s => s.Name.ToLower().Equals(subject.SubjectName.ToLower()) && s.deaf_mute == subject.deaf_mute,q=>q.Include(s=>s.Teachers).Include(s=>s.Students),cancellationToken: cancellationToken);
			if (res != null)
			{
				var lessonRef = _firestoreDb._Db.Collection("Subjects").Document(res.Id).Collection("Lessons");
				int lessonsCount = (await lessonRef.GetSnapshotAsync(cancellationToken)).Count;
				SubjectDTO subjectDTO = new SubjectDTO
				{
					SubjectId = res.Id,
					SubjectName = res.Name,
					deaf_mute = res.deaf_mute,
					studentsCount = res.Students != null ? res.Students.Count() : 0,
					teachersCount = res.Teachers != null ? res.Teachers.Count() : 0,
					lessonsCount = lessonsCount
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

		public async Task<ResultDTO> AddLesson(AddLessonDTO lessonDTO, CancellationToken cancellationToken)
		{
			bool subjectExists = await IsSubjectExist(subjectId: lessonDTO.SubjectID, cancellationToken: cancellationToken);
			if (!subjectExists)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "Subject not found"
				};
			}
			else
			{

				// Reference to the Lessons collection under the subject
				var lesRef = _firestoreDb._Db.Collection("Subjects").Document(lessonDTO.SubjectID).Collection("Lessons");

				// Check if a lesson with the same title exists (case-insensitive)
				var querySnap = await lesRef
					.WhereEqualTo("title", lessonDTO.Title.ToLower())
					.GetSnapshotAsync(cancellationToken);

				if (querySnap.Count > 0)
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status409Conflict,
						Message = "Lesson with the same title already exists"
					};
				}

				// Create the new lesson
				var newLesson = new Lesson
				{
					Title = lessonDTO.Title.ToLower(),
					Description = lessonDTO.Description
					// Add other fields like VideoUrl, Content, etc.
				};

				// Save to Firestore
				await lesRef.Document(newLesson.Id).SetAsync(newLesson, cancellationToken: cancellationToken);

				// ✅ Return success with the created lesson
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status201Created,
					Message = "Lesson added successfully",
					result = newLesson
				};
			}
		}


		private async Task<bool> IsSubjectExist (string ?subjectName = "", bool ?deaf_mute=false,string ?subjectId="",CancellationToken cancellationToken = default)
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
