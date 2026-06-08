using System.Security.Cryptography;
using System.Threading;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;
using MongoDB.Driver;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Grad.Infrastructure.Repository
{
	public class lessonRepository : Repository , IlessonRepository
	{
		private readonly IMongoCollection<SubjectContent> _subjects;
		private readonly Db_Context _dbContext;


		public lessonRepository(MongoDBContext mongoDB, Db_Context db_Context) : base(db_Context)
		{
			_subjects = mongoDB != null ? mongoDB.Subjects : throw new ArgumentNullException(nameof(mongoDB));
			_dbContext = db_Context ?? throw new ArgumentNullException(nameof(db_Context));
		}

		public async Task<int> addLesson(LessonContent lesson,Guid sid,Guid lid,CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.Not(
				Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Title == lesson.Title)));
			var update = Builders<SubjectContent>.Update.Push(s => s.Lessons, lesson);
			var res = await _subjects.UpdateOneAsync(filter, update);
			return (int)res.ModifiedCount;
		}

		public async Task<IEnumerable<LessonContent>> viewLessons(Guid sid,CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid));
			SubjectContent subjectContent = await _subjects.Find(filter).FirstOrDefaultAsync(CT);
			return subjectContent.Lessons;
		}

		public async Task<LessonContent> viewLesson(Guid sid,Guid ?lid,CancellationToken CT)
		{
			if (lid == null)
				return null;
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lid));
			SubjectContent subjectContent = await _subjects.Find(filter).FirstOrDefaultAsync(CT);
			return subjectContent != null ? subjectContent.Lessons.FirstOrDefault(l => l.Id == lid) : null;
		}

		public async Task<int> uploadVideo(Guid sid,LessonContent lesson, CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id,sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lesson.Id));
			var update = Builders<SubjectContent>.Update.Set("Lessons.$.Videos", lesson.Videos);
			var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: CT);
			return (int)result.ModifiedCount;
		}

		public async Task<Video> viewVideo(Guid sid,Guid lid,Guid ?Vid,CancellationToken CT)
		{
			LessonContent lesson = await viewLesson(sid, lid, CT);
			return Vid != null ? lesson.Videos.FirstOrDefault(v => v.Id == Vid) : null;
		}
		public async Task<int> DeleteLesson(Guid sid, LessonContent lesson,CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lesson.Id));
			var update = Builders<SubjectContent>.Update.PullFilter(s => s.Lessons, l => l.Id == lesson.Id);
			var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: CT);
			if(result.ModifiedCount == 0)
				return 0;
			else
			{
				int res = await base.DeleteEntitiesAsync<StudentProgress>(filter: l => l.Id == lesson.Id, cancellationToken: CT);
				return res + (int)result.MatchedCount; 
			}
		}

		public async Task<int> DeleteVideo(Guid sid, Guid lid, Guid ?vid, CancellationToken CT)
		{
			LessonContent lesson = await viewLesson(sid, lid, CT);
			Video Vid = vid != null ? lesson.Videos.FirstOrDefault(v => v.Id == vid) : null;
			if (Vid != null)
			{
				var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lid));
				lesson.Videos.Remove(Vid);
				var update = Builders<SubjectContent>.Update.Set("Lessons.$.Videos", lesson.Videos);
				var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: CT);
				return (int)result.ModifiedCount;
			}
			return 0;
		}

		public async Task<int> editLesson(Guid sid,LessonContent lesson,CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lesson.Id));
			var update = Builders<SubjectContent>.Update.Combine(Builders<SubjectContent>.Update.Set("Lessons.$.Title", lesson.Title),
				Builders<SubjectContent>.Update.Set("Lessons.$.Description", lesson.Description),
				Builders<SubjectContent>.Update.Set("Lessons.$.Level", lesson.Level),
				Builders<SubjectContent>.Update.Set("Lessons.$.Perquisite", lesson.Perquisite),
				Builders<SubjectContent>.Update.Set("Lessons.$.PerquisiteType", lesson.PerquisiteType),
				Builders<SubjectContent>.Update.Set("Lessons.$.Next", lesson.Next), Builders<SubjectContent>.Update.Set("Lessons.$.NextType", lesson.NextType));
			var result = await _subjects.UpdateOneAsync(filter, update,cancellationToken: CT);
			return (int)result.ModifiedCount;
		}

		public async Task<int> updateNext(Guid sid,Guid lid,Guid Nlid,PerquisiteType type,CancellationToken CT)
		{
			var filter = Builders<SubjectContent>.Filter.And(Builders<SubjectContent>.Filter.Eq(s => s.Id, sid), Builders<SubjectContent>.Filter.ElemMatch(s => s.Lessons, l => l.Id == lid));
			var update = Builders<SubjectContent>.Update.Combine(
				Builders<SubjectContent>.Update.Set("Lessons.$.Next", Nlid),
				Builders<SubjectContent>.Update.Set("Lessons.$.NextType", type));
			var result = await _subjects.UpdateOneAsync(filter, update, cancellationToken: CT);
			return (int)result.ModifiedCount;
		}
		
	}
}
