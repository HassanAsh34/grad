using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.ExerciseFeatures.Interfaces;

using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Grad.Infrastructure.Repository
{
	public class ExerciseRepository : IExerciseRepository
	{
		private readonly IMongoCollection<SubjectContent> _subjects;

		public ExerciseRepository(MongoDBContext Mcontext)
		{
			_subjects = Mcontext != null ? Mcontext.Subjects : throw new ArgumentNullException(nameof(Mcontext));
		}

		public async Task<List<Level>> viewLevels(Guid sid, CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.Eq(s => s.Id, sid);
			SubjectContent subject = await _subjects.Find(filter).FirstOrDefaultAsync(CT);
			return subject != null ? subject.Quizzes : new List<Level>();
		}

		public async Task<int> addExerciseToLesson(Guid sid, LessonContent lesson, CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lesson.Id));
			var update = Builders<SubjectContent>.Update.Set("Lessons.$.Level", lesson.Level);
			var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: CT);
			return (int)result.ModifiedCount;
		}

		public async Task<int> addQuizToSubject(Guid sid, Level exercise, CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.Eq(s => s.Id, sid);
			var update = Builders<SubjectContent>.Update.Push(s => s.Quizzes, exercise);
			var result = await _subjects.UpdateOneAsync(filter, update);
			return (int)result.ModifiedCount;
		}

		public async Task<Level> GetLevel(Guid Sid, Guid? Lid, Guid LVLid, CancellationToken CT = default)
		{
			Level level = null;
			if (Lid == null || Lid == Guid.Empty)
			{
				var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, Sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Quizzes, q => q.ID == LVLid));
				SubjectContent s = await _subjects.Find(filter).FirstOrDefaultAsync(CT);
				if (s != null)
				{
					level = s.Quizzes != null ? s.Quizzes.FirstOrDefault(q => q.ID == LVLid) : null;
				}
			}
			else
			{
				var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, Sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == Lid));
				SubjectContent s = await _subjects.Find(filter).FirstOrDefaultAsync(CT);
				if (s != null)
				{
					LessonContent lesson = s.Lessons.FirstOrDefault(l=>l.Id == Lid);
					level = lesson != null ? lesson.Level != null ? lesson.Level : null : null;
				}
			}
			return level;
		}

		public async Task<int> DeleteQuiz(Guid sid, Guid? Lid, Guid LVLid, CancellationToken cancellationToken)
		{
			if (Lid == null || Lid == Guid.Empty)
			{
				var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Quizzes, q => q.ID == LVLid));
				SubjectContent subject = await _subjects.Find(filter).FirstOrDefaultAsync(cancellationToken);
				if (subject != null)
				{
					Level level = subject.Quizzes.FirstOrDefault(q => q.ID == LVLid);
					if (level != null)
					{
						var update = Builders<SubjectContent>.Update.PullFilter(s => s.Quizzes, q => q.ID == LVLid);
						var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
						return (int)result.ModifiedCount;
					}
					else
						return -1; // quiz wasnt found
				}
				else
					return -1; // subject wasnt found
			}
			else
			{
				var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == Lid));
				SubjectContent subject = await _subjects.Find(filter).FirstOrDefaultAsync(cancellationToken);
				if (subject != null)
				{
					LessonContent lesson = subject.Lessons.FirstOrDefault(l => l.Id == Lid);
					if (lesson != null && lesson.Level != null && lesson.Level.ID == LVLid)
					{
						var update = Builders<SubjectContent>.Update.Unset("Lessons.$.Level");
						var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
						return (int)result.ModifiedCount;
					}
					else
						return -1; // quiz wasnt found
				}
				else
					return -1; // subject wasnt found
			}
		}

		//public async Task<int> editLevel(Guid Sid, Guid? Lid, Level exercise, CancellationToken CT)
		//{
		//	FilterDefinition<SubjectContent> filter;
		//	UpdateDefinition<SubjectContent> updateBuilder;
		//	if (Lid == null || Lid == Guid.Empty)
		//	{
		//		filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, Sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Quizzes, q => q.ID == exercise.ID));
		//		updateBuilder = Builders<SubjectContent>.Update.PullFilter(l => l.Quizzes, q => q.ID == exercise.ID);
		//		var res = await _subjects.UpdateOneAsync(filter, updateBuilder, cancellationToken: CT);
		//		if (res.MatchedCount == 0)
		//			return 0;
		//		updateBuilder = Builders<SubjectContent>.Update.Push(s => s.Quizzes, exercise);
		//	}
		//	else
		//	{
		//		filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, Sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == Lid));
		//		updateBuilder = Builders<SubjectContent>.Update.Set("Lessons.$.Level", exercise);
		//	}
		//	var result = await _subjects.UpdateOneAsync(filter, updateBuilder, cancellationToken: CT);
		//	return (int)result.ModifiedCount;
		//}

		public async Task<int> editLevel(Guid Sid, Guid? Lid, Level exercise, CancellationToken CT)
		{
			FilterDefinition<SubjectContent> filter;
			UpdateDefinition<SubjectContent> updateBuilder;

			if (Lid == null || Lid == Guid.Empty)
			{
				// ✅ Update specific quiz inside Quizzes array
				filter = Builders<SubjectContent>.Filter.And(
					Builders<SubjectContent>.Filter.Eq(s => s.Id, Sid),
					Builders<SubjectContent>.Filter.ElemMatch(s => s.Quizzes, q => q.ID == exercise.ID)
				);

				updateBuilder = Builders<SubjectContent>.Update
					.Set("Quizzes.$", exercise); // 🔥 THIS is the fix
			}
			else
			{
				// ✅ Lessons case (already correct)
				filter = Builders<SubjectContent>.Filter.And(
					Builders<SubjectContent>.Filter.Eq(s => s.Id, Sid),
					Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == Lid)
				);

				updateBuilder = Builders<SubjectContent>.Update
					.Set("Lessons.$.Level", exercise);
			}

			var result = await _subjects.UpdateOneAsync(filter, updateBuilder, cancellationToken: CT);

			return (int)result.ModifiedCount;
		}
	}
}
