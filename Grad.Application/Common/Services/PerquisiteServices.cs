using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.DTOs;
using Grad.Application.ExerciseFeatures.Interfaces;
using Grad.Application.LessonFeatures.DTOs;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Application.TeacherFeatures.DTOs;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.Common.Services
{
	public class PerquisiteServices : IPerquisiteServices
	{

		private readonly IlessonRepository _lessonRepository;
		private readonly IExerciseRepository _exerciseRepository;

		public PerquisiteServices(IlessonRepository lessonRepository, IExerciseRepository exerciseRepository)
		{
			_lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
			_exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
		}




		public async Task<int> UpdatePerquisite(Guid subjectId, Guid L_QID, PerquisiteType L_QType, Guid ? Oid, PerquisiteType ? Otype, Guid ? Nid, PerquisiteType NType, bool Create, CancellationToken cancellationToken)
		{
			int res = 0;
			if (!Create && Otype != null)
			{
				if(Nid != null && NType != PerquisiteType.None)
				{
					if (L_QID == Nid && L_QType == NType)
						return -3;// cant have a lesson as a perquisite to itself
					else if (Otype != PerquisiteType.None && Oid == Nid && Otype == NType)
						return -1;// conflict no changes where made

				}
				//else if (Oid == L_QID && Otype == L_QType)
				//add logger here
				else
				{
					res = await RemovePerquisite(subjectId, (Guid)Oid,	(PerquisiteType)Otype, cancellationToken);
					if (res <= 0)
						return -2; //failed to update the perquisite
				}
			}
			switch (L_QType)
			{
				case PerquisiteType.Lesson:

					LessonContent lesson = await _lessonRepository.viewLesson(subjectId, L_QID, cancellationToken);
					if (lesson == null)
						return -2;
					lesson.Perquisite = Nid;
					lesson.PerquisiteType = NType;
					res = await UpdateNext(subjectId, Nid, NType, lesson.Id, PerquisiteType.Lesson, cancellationToken);
					if (res <= 0)
						return res;
					res = await _lessonRepository.editLesson(subjectId,lesson, cancellationToken);
					if (res <= 0)
					{
						res = await RemovePerquisite(subjectId, (Guid)Nid, NType, cancellationToken);
						//if (res <= 0)
						//	//add logger here for problems
						return -2;
					}
					else
						return res;

				case PerquisiteType.Quiz:
					Level level = await _exerciseRepository.GetLevel(subjectId,null,LVLid: L_QID, CT: cancellationToken);
					if (level == null)
						return -2;
					level.Perquisite = Nid;
					level.PerquisiteType = NType;
					res = await UpdateNext(subjectId, Nid, NType, level.ID, PerquisiteType.Quiz, cancellationToken);
					if (res <= 0)
						return res;
					res = await _exerciseRepository.editLevel(subjectId,null,exercise: level,CT: cancellationToken);
					if (res <= 0)
					{
						res = await RemovePerquisite(subjectId, (Guid)Nid, NType, cancellationToken);
						//if (res <= 0)
						//	//add logger here for problems
						return -2;
					}
					else
						return res;
				default:
					return -2;
			}
		}



		private async Task<int> RemovePerquisite(Guid subjectId, Guid perquisiteID, PerquisiteType perquisiteType, CancellationToken cancellationToken)
		{
			int res = 0;
			if (perquisiteID == Guid.Empty && perquisiteType != PerquisiteType.None)
				return -1;
			switch (perquisiteType)
			{
				case PerquisiteType.Lesson:
					LessonContent Plesson = await _lessonRepository.viewLesson(subjectId, perquisiteID, cancellationToken);
					if (Plesson != null)
					{
						Plesson.Next = null;
						Plesson.NextType = PerquisiteType.None;
						res = await _lessonRepository.editLesson(subjectId, Plesson, cancellationToken);
					}
					return res > 0 ? 1 : 0;
				case PerquisiteType.Quiz:
					Level pquiz = await _exerciseRepository.GetLevel(subjectId, null, perquisiteID, cancellationToken);
					if (pquiz != null)
					{
						pquiz.Next = null;
						pquiz.NextType = PerquisiteType.None;
						res = await _exerciseRepository.editLevel(subjectId, null, pquiz, cancellationToken);
					}
					return res > 0 ? 1 : 0;
				default:
					return 1;

			}
		}



		public async Task<int> removeDependency(Guid subjectId, LessonContent? lesson, Level? level, CancellationToken cancellationToken)
		{
			IEnumerable<int> res = new List<int>();
			if (lesson != null)
			{
				if (lesson.Next != null && lesson.NextType != PerquisiteType.None)
				{
					switch (lesson.NextType)
					{
						case PerquisiteType.Lesson:
							LessonContent Nlesson = await _lessonRepository.viewLesson(subjectId, lesson.Next.Value, cancellationToken);
							if (Nlesson != null)
							{
								lesson.Next = null;
								lesson.NextType = PerquisiteType.None;
								Nlesson.Perquisite = null;
								Nlesson.PerquisiteType = PerquisiteType.None;
								res = Task.WhenAll(_lessonRepository.editLesson(subjectId, lesson, cancellationToken), _lessonRepository.editLesson(subjectId, Nlesson, cancellationToken)).Result;
								if (res.Any(r => r == 0))
								{
									return -3;// failed to update the next lesson
								}
								break;
							}
							else
								return -2; //failed to find the next lesson
						case PerquisiteType.Quiz:
							Level nquiz = await _exerciseRepository.GetLevel(subjectId, null, lesson.Next ?? Guid.Empty, cancellationToken);
							if (nquiz != null)
							{
								lesson.Next = null;
								lesson.NextType = PerquisiteType.None;
								nquiz.Perquisite = null;
								nquiz.PerquisiteType = PerquisiteType.None;
								res = Task.WhenAll(_lessonRepository.editLesson(subjectId, lesson, cancellationToken), _exerciseRepository.editLevel(subjectId, null, nquiz, cancellationToken)).Result;
								if (res.Any(r => r == 0))
								{
									return -3; // failed to update the next quiz
								}
								break;
							}
							else
								return -2; //failed to find the next quiz
					}

				}
				else if (lesson.NextType == PerquisiteType.None)
					res = new List<int> { 1, 1 };
				else
					return -3;

				if (lesson.Perquisite != null)
				{
					switch (lesson.PerquisiteType)
					{
						case PerquisiteType.None:
							res = new List<int> { 1, 1 };
							break;
						case PerquisiteType.Lesson:
							LessonContent plesson = await _lessonRepository.viewLesson(subjectId, lesson.Perquisite ?? Guid.Empty, cancellationToken);
							if (plesson != null)
							{
								lesson.Perquisite = null;
								lesson.PerquisiteType = PerquisiteType.None;
								plesson.Next = null;
								plesson.NextType = PerquisiteType.None;
								res = await Task.WhenAll(_lessonRepository.editLesson(subjectId, lesson, cancellationToken), _lessonRepository.editLesson(subjectId, plesson, cancellationToken));
								if (res.Any(r => r == 0))
								{
									return -3;// failed to update the perquisite lesson
								}
								break;
							}
							else
								return -2; //failed to find the next lesson
						case PerquisiteType.Quiz:
							Level pquiz = await _exerciseRepository.GetLevel(subjectId, null, lesson.Next ?? Guid.Empty, cancellationToken);
							if (pquiz != null)
							{
								lesson.Perquisite = null;
								lesson.PerquisiteType = PerquisiteType.None;
								pquiz.Next = null;
								pquiz.NextType = PerquisiteType.None;
								res = await Task.WhenAll(_lessonRepository.editLesson(subjectId, lesson, cancellationToken), _exerciseRepository.editLevel(subjectId, null, pquiz, cancellationToken));
								if (res.Any(r => r == 0))
								{
									return -3; // failed to update the next quiz
								}
								break;
							}
							else
								return -2; //failed to find the next quiz
					}
				}
				else if (lesson.PerquisiteType == PerquisiteType.None)
					res = new List<int> { 1, 1 };
				else
					return -3;
			}
			else if (level != null)
			{
				if (level.Next != null)
				{
					switch (level.NextType)
					{
						case PerquisiteType.Lesson:
							LessonContent Nlesson = await _lessonRepository.viewLesson(subjectId, level.Next ?? Guid.Empty, cancellationToken);
							if (Nlesson != null)
							{
								level.Next = null;
								level.NextType = PerquisiteType.None;
								Nlesson.Perquisite = null;
								Nlesson.PerquisiteType = PerquisiteType.None;
								res =await Task.WhenAll(_exerciseRepository.editLevel(subjectId, null, level, cancellationToken), _lessonRepository.editLesson(subjectId, Nlesson, cancellationToken));
								if (res.Any(r => r == 0))
								{
									return -3;// failed to update the next lesson
								}
								break;
							}
							else
								return -2; //failed to find the next lesson
					}

				}
				else if (level.NextType == PerquisiteType.None)
					res = new List<int> { 1, 1 };
				else
					return -3;
				if (level.Perquisite != null)
				{
					switch (level.PerquisiteType)
					{
						case PerquisiteType.Lesson:
							LessonContent plesson = await _lessonRepository.viewLesson(subjectId, level.Perquisite ?? Guid.Empty, cancellationToken);
							if (plesson != null)
							{
								level.Perquisite = null;
								level.PerquisiteType = PerquisiteType.None;
								plesson.Next = null;
								plesson.NextType = PerquisiteType.None;
								res = await Task.WhenAll(_exerciseRepository.editLevel(subjectId, null, level, cancellationToken), _lessonRepository.editLesson(subjectId, plesson, cancellationToken));
								if (res.Any(r => r == 0))
								{
									return -3;// failed to update the perquisite lesson
								}
								break;
							}
							else
								return -2; //failed to find the next lesson
					}
				}
				else if (level.PerquisiteType == PerquisiteType.None)
					res = new List<int> { 1, 1 };
				else
					return -3;
			}
			else
			{
				return -1;
			}
			return res.Any(r => r == 0) ? 0 : 1;
		}

		private async Task<int> UpdateNext(Guid sid,Guid ?pl_q,PerquisiteType pl_qType,Guid Nid,PerquisiteType NType,CancellationToken cancellationToken = default)
		{
			if (pl_qType != PerquisiteType.None && pl_q == null)
				return -2;

			switch(pl_qType)
			{
				case PerquisiteType.None:
					return 1;
				case PerquisiteType.Lesson:
					LessonContent lesson = await _lessonRepository.viewLesson(sid,pl_q, cancellationToken);
					if (lesson == null)
						return -2;
					else
					{
						lesson.Next = Nid;
						lesson.NextType = NType;
						int res = await _lessonRepository.editLesson(sid, lesson, cancellationToken);
						return res > 0 ? 1 : -2;
					}
				case PerquisiteType.Quiz:
					Level level = await _exerciseRepository.GetLevel(sid, null,(Guid)pl_q, cancellationToken);
					if(level == null || Nid == null)
						return -2;
					else
					{
						level.Next = Nid;
						level.NextType = NType;
						int res = await _exerciseRepository.editLevel(sid,null,level, cancellationToken);
						return res > 0 ? 1 : -2;
					}
				default:
					return -2;
			}
		}

	}
}