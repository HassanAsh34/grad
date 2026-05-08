using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.Interfaces;
using Grad.Application.ExerciseFeatures.Interfaces;
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




		public async Task<int> UpdatePerquisite(Guid subjectId, PerquisiteType CperquisiteType, Guid CperquisiteID, Guid NperquisiteID, PerquisiteType NperquisiteType,Guid nextID,bool Create ,CancellationToken cancellationToken)// we might copy it to a seperate services
		{
			if (!Create)
			{
				if (NperquisiteID != Guid.Empty)
				{
					if (CperquisiteType != PerquisiteType.None)
					{
						if (CperquisiteID == NperquisiteID && CperquisiteType == NperquisiteType)
							return -1; //conflict no changes where made
						int res = await RemovePerquisite(subjectId, CperquisiteID, CperquisiteType, cancellationToken);
						if (res > 0)
						{
							CperquisiteID = NperquisiteID;
							CperquisiteType = NperquisiteType;
						}
						else
						{
							return -2; //failed to update the perquisite
						}
					}
					else
					{
						CperquisiteID = NperquisiteID;
						CperquisiteType = NperquisiteType;
					}
				}
				else
				{
					if (NperquisiteType == PerquisiteType.None)
					{
						if (CperquisiteType != PerquisiteType.None)
						{
							int res = await RemovePerquisite(subjectId, CperquisiteID, CperquisiteType, cancellationToken);
							if (res > 0)
							{
								CperquisiteID = NperquisiteID;
								CperquisiteType = NperquisiteType;
							}
							else
							{
								return -2; //failed to update the perquisite
							}
						}
						else
						{
							return -1; // no changes were made
						}
					}
				}
			}
			switch (CperquisiteType)
			{
				case PerquisiteType.Lesson:
					LessonContent Plesson = await _lessonRepository.viewLesson(subjectId, CperquisiteID, cancellationToken);
					if (Plesson == null)
						return 0;
					else
					{
						if (Plesson.Id == nextID)
							return -3; //cant have a lesson as a perquisite to itself
						else if (Plesson.Perquisite == nextID && Plesson.PerquisiteType == PerquisiteType.Lesson)
							return -4; //cant have a lesson as a perquisite to its own perquisite
						else
						{
							Plesson.Next = nextID;
							Plesson.NextType = PerquisiteType.Lesson;
							return await _lessonRepository.editLesson(subjectId, Plesson, cancellationToken);
						}
					}
				case PerquisiteType.Quiz: // needs to be handled
					Level pquiz = await _exerciseRepository.GetLevel(subjectId, null, CperquisiteID, cancellationToken);
					if (pquiz != null)
					{
						if (pquiz.Perquisite == nextID && pquiz.PerquisiteType == PerquisiteType.Lesson)
							return -4;
						else
						{
							pquiz.Next = nextID;
							pquiz.NextType = PerquisiteType.Quiz;
							int res = await _exerciseRepository.editLevel(subjectId, null, pquiz, cancellationToken);
							return res > 0 ? 1 : 0;	
						}
					}
					else
						return 0;

				default:
					return 1;
			}
		}




		private async Task<int> RemovePerquisite(Guid subjectId,Guid perquisiteID ,PerquisiteType perquisiteType,CancellationToken cancellationToken)
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

		public async Task<int> removeDependency(Guid subjectId,LessonContent ?lesson,Level ?level,CancellationToken cancellationToken)
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
								res = Task.WhenAll(_lessonRepository.editLesson(subjectId, lesson, cancellationToken), _lessonRepository.editLesson(subjectId, plesson, cancellationToken)).Result;
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
								res = Task.WhenAll(_lessonRepository.editLesson(subjectId, lesson, cancellationToken), _exerciseRepository.editLevel(subjectId, null, pquiz, cancellationToken)).Result;
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
							LessonContent Nlesson = await _lessonRepository.viewLesson(subjectId,level.Next ?? Guid.Empty, cancellationToken);
							if (Nlesson != null)
							{
								level.Next = null;
								level.NextType = PerquisiteType.None;
								Nlesson.Perquisite = null;
								Nlesson.PerquisiteType = PerquisiteType.None;
								res = Task.WhenAll(_exerciseRepository.editLevel(subjectId,null,level,cancellationToken), _lessonRepository.editLesson(subjectId, Nlesson, cancellationToken)).Result;
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
								res = Task.WhenAll(_exerciseRepository.editLevel(subjectId, null,level, cancellationToken), _lessonRepository.editLesson(subjectId, plesson, cancellationToken)).Result;
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
	}
}
