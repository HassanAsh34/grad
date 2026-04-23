using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.LessonFeatures.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.Common.Interfaces
{
	public interface IPerquisiteServices
	{
		//public Task<int> UpdatePerquisite(Guid subjectId, PerquisiteType perquisiteType, Guid perquisiteID, Guid NperquisiteID, Guid nextID, PerquisiteType NperquisiteType = PerquisiteType.None, CancellationToken cancellationToken = default);

		public Task<int> UpdatePerquisite(Guid subjectId, PerquisiteType CperquisiteType, Guid CperquisiteID, Guid NperquisiteID, PerquisiteType NperquisiteType, Guid nextID, bool Create = false, CancellationToken cancellationToken = default);// we might copy it to a seperate services

		public Task<int> RemovePerquisite(Guid subjectId, Guid perquisiteID, PerquisiteType perquisiteType, CancellationToken cancellationToken = default);
	}
}
