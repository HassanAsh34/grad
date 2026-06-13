using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.LessonFeatures.Interfaces
{
	public interface IlessonRepository
	{
		Task<int> addLesson(LessonContent lesson, Guid sid, Guid lid, CancellationToken CT = default);

		Task<IEnumerable<LessonContent>> viewLessons(Guid sid, CancellationToken CT = default);

		Task<LessonContent> viewLesson(Guid sid, Guid ?lid, CancellationToken CT = default);

		Task<int> uploadVideo(Guid sid, LessonContent lesson, CancellationToken CT = default);

		Task<Video> viewVideo(Guid sid, Guid lid, Guid ?Vid = null, CancellationToken CT = default);

		Task<int> DeleteLesson(Guid sid, LessonContent lesson, CancellationToken CT = default);

		Task<int> DeleteVideo(Guid sid, Guid lid, Guid ?vid = null, CancellationToken CT = default);

		Task<int> editLesson(Guid sid, LessonContent lesson, CancellationToken CT = default);

		//Task<int> updateNext(Guid sid, Guid lid, Guid Nlid, PerquisiteType type, CancellationToken CT = default);

	}
}
