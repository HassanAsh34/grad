using Grad.Application.Common.Interfaces;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Grad.Infrastructure.Repository
{
	public class SubjectRepository : Repository, ISubjectRepository
	{
		private readonly Db_Context _context;
		private readonly IMongoCollection<SubjectContent>? _subjects;
		private readonly IUowServices _uowServices;

		public SubjectRepository(Db_Context context,IUowServices uowServices, MongoDBContext? mongoDB = null) : base(context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_subjects = mongoDB != null ? mongoDB.Subjects : throw new ArgumentNullException(nameof(mongoDB));
			_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));
		}


		public async Task<int> AddSubject(Subject subject,CancellationToken CT)
		{
			base.CreateEntityAsync(subject, CT);
			int res = await _uowServices.SaveChangesAsync();
			if (res > 0)
			{
				try
				{
					SubjectContent subjectContent = new SubjectContent
					{
						Id = subject.Id
					};
					await _subjects.InsertOneAsync(subjectContent);
					return res;
				}
				catch (Exception e)
				{
					base.DeleteEntityAsync<Subject>(subject);
					res = await _uowServices.SaveChangesAsync();
					return 0;
				};
			}
			else
				return 0;
		}

		public async Task<List<Subject>> GetSubjectsByTeacherAsync(Guid teacherId, CancellationToken ct = default)
		{
			return await _context.AssignedSubjects
				.Where(a => a.TeacherId == teacherId)
				.Include(a => a.Subject)
				.Select(a => a.Subject)
				.ToListAsync(ct);
		}

		public async Task<List<Subject>> GetSubjectsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
		{
			return await _context.subjects
				.Where(s => ids.Contains(s.Id))
				.ToListAsync(ct);
		}

		public async Task<List<Subject>> GetSubjectsFilteredAsync(bool? deafMute, IEnumerable<Guid>? excludeIds = null, CancellationToken ct = default)
		{
			var query = _context.subjects.AsQueryable();

			if (deafMute.HasValue)
			{
				query = query.Where(s => s.deaf_mute == deafMute.Value);
			}

			if (excludeIds != null && excludeIds.Any())
			{
				query = query.Where(s => !excludeIds.Contains(s.Id));
			}

			return await query.ToListAsync(ct);
		}

		public async Task<List<Subject>> GetAllSubjectsAsync(CancellationToken ct = default)
		{
			return await _context.subjects.ToListAsync(ct);
		}

		public async Task<Subject?> GetSubjectWithRelationsAsync(Guid subjectId, CancellationToken ct = default)
		{
			return await _context.subjects
				.Include(s => s.AssignedSubjects)
				.Include(s => s.Students)
				.Include(s => s.Submissions)
				.FirstOrDefaultAsync(s => s.Id == subjectId, ct);
		}

		public async Task<SubjectContent?> GetSubjectContentAsync(Guid subjectId, CancellationToken ct = default)
		{
			if (_subjects == null) return null;
			
			return await _subjects.Find(s => s.Id == subjectId).FirstOrDefaultAsync(ct);
		}

		public async Task<bool> IsSubjectExistAsync(string? subjectName = "", bool? deaf_mute = false, Guid? subjectId = null, CancellationToken cancellationToken = default)
		{
			if (subjectId.HasValue)
			{
				return await _context.subjects.AnyAsync(s => s.Id == subjectId.Value, cancellationToken);
			}

			if (!string.IsNullOrWhiteSpace(subjectName))
			{
				var normalizedName = subjectName.Trim().ToLower();
				return await _context.subjects.AnyAsync(s => s.Name.ToLower() == normalizedName && s.deaf_mute == deaf_mute, cancellationToken);
			}

			return false;
		}

		public async Task<int> AddVocabularyAsync(Guid subjectId, Vocabulary vocabulary, CancellationToken ct = default)
		{
			if (_subjects == null) return -1;

			var filter = Builders<SubjectContent>.Filter.Eq(s => s.Id, subjectId);
			var subject = await _subjects.Find(filter).FirstOrDefaultAsync(ct);

			if (subject == null) return 0;

			UpdateResult res;
			if (subject.Dictionary == null)
			{
				var update = Builders<SubjectContent>.Update.Set(s => s.Dictionary, vocabulary);
				res = await _subjects.UpdateOneAsync(filter, update, cancellationToken: ct);
			}
			else
			{
				var update = Builders<SubjectContent>.Update.PushEach(s => s.Dictionary.wordItems, vocabulary.wordItems);
				res = await _subjects.UpdateOneAsync(filter, update, cancellationToken: ct);
			}

			return (int)res.ModifiedCount;
		}

		public async Task<bool> updateLessonCountAsync(Guid subjectId, CancellationToken ct = default)
		{
			Subject subject = await base.GetEntityAsync<Subject>(s=>s.Id == subjectId, ct);
			if (subject == null) return false;	
			SubjectContent subjectContent =await GetSubjectContentAsync(subject.Id, ct);
			if (subjectContent == null) return false;
			subject.LessonCount = subjectContent.Lessons.Count;
			base.UpdateEntityAsync(subject, ct);
			int res = await _uowServices.SaveChangesAsync();
			return res > 0;
		}

		public async Task<int> DeleteSubject(Subject subject, CancellationToken cancellationToken = default)
		{
			base.DeleteEntityAsync(subject, cancellationToken);
			 _subjects.DeleteOne(s => s.Id == subject.Id, cancellationToken);
			int res =await _uowServices.SaveChangesAsync();
			return res;
		}
	}
}
