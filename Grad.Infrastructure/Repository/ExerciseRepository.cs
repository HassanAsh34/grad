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

		public async Task<int> addExerciseToLesson(Guid sid, LessonContent lesson, CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lesson.Id));
			var update = Builders<SubjectContent>.Update.Set("Lessons.$.Exercise", lesson.Exercise);
			var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: CT);
			return (int)result.ModifiedCount;
		}

		public async Task<int> addQuizToSubject(Guid sid,Exercise exercise,CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.Eq(s => s.Id,sid);
			var update = Builders<SubjectContent>.Update.Push(s => s.Quizzes, exercise);
			var result = await _subjects.UpdateOneAsync(filter, update);
			return (int)result.ModifiedCount;
		}
	}
}
