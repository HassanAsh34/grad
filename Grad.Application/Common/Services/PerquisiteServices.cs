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

		//fix issue with perquisite

		//public async Task<int> UpdatePerquisite(Guid subjectId, PerquisiteType perquisiteType, Guid perquisiteID, Guid NperquisiteID, Guid nextID, PerquisiteType NperquisiteType, CancellationToken cancellationToken)// we might copy it to a seperate services
		//{

		//	if (NperquisiteID != Guid.Empty)
		//	{
		//		if (perquisiteType != PerquisiteType.None)
		//		{
		//			if (perquisiteID == NperquisiteID && perquisiteType == NperquisiteType)
		//				return -1; //conflict no changes where made
		//			int res = await RemovePerquisite(subjectId, perquisiteID, perquisiteType, cancellationToken);
		//			if (res > 0)
		//			{
		//				perquisiteID = NperquisiteID;
		//				perquisiteType = NperquisiteType;
		//			}
		//			else
		//			{
		//				return -2; //failed to update the perquisite
		//			}
		//		}
		//		else
		//		{
		//			perquisiteID = NperquisiteID;
		//			perquisiteType = NperquisiteType;
		//		}
		//	}
		//	else
		//	{
		//		if (NperquisiteType == PerquisiteType.None)
		//		{
		//			if (perquisiteType != PerquisiteType.None)
		//			{
		//				int res = await RemovePerquisite(subjectId, perquisiteID, perquisiteType, cancellationToken);
		//				if (res > 0)
		//				{
		//					perquisiteID = NperquisiteID;
		//					perquisiteType = NperquisiteType;
		//				}
		//				else
		//				{
		//					return -2; //failed to update the perquisite
		//				}
		//			}
		//			else
		//			{
		//				return -1;
		//			}
		//		}
		//	}
		//	switch (perquisiteType)
		//	{
		//		case PerquisiteType.Lesson:
		//			LessonContent Plesson = await _lessonRepository.viewLesson(subjectId, perquisiteID, cancellationToken);
		//			if (Plesson == null)
		//				return 0;
		//			else
		//			{
		//				if (Plesson.Id == nextID)
		//					return -3; //cant have a lesson as a perquisite to itself
		//				else if (Plesson.Perquisite == nextID && Plesson.PerquisiteType == PerquisiteType.Lesson)
		//					return -4; //cant have a lesson as a perquisite to its own perquisite
		//				else
		//				{
		//					Plesson.Next = nextID;
		//					Plesson.NextType = PerquisiteType.Lesson;
		//					return await _lessonRepository.editLesson(subjectId, Plesson, cancellationToken);
		//				}
		//			}
		//		case PerquisiteType.Quiz: // needs to be handled
		//			Level pquiz = await _exerciseRepository.GetLevel(subjectId, null, perquisiteID, cancellationToken);
		//			if (pquiz != null)
		//			{
		//				pquiz.Next = null;
		//				pquiz.NextType = PerquisiteType.None;
		//				int res = await _exerciseRepository.editLevel(subjectId, null, pquiz, cancellationToken);
		//				return res > 0 ? 1 : 0;
		//			}
		//			else
		//				return 0;

		//		default:
		//			return 1;
		//	}
		//}


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
							return -1;
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

		public async Task<int> RemovePerquisite(Guid subjectId,Guid perquisiteID ,PerquisiteType perquisiteType,CancellationToken cancellationToken)
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
	}
}
