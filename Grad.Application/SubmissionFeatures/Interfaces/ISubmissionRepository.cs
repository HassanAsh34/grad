using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.Interfaces;
using Grad.Domain.Model;

namespace Grad.Application.SubmissionFeatures.Interfaces
{
	public interface ISubmissionRepository : IRepository
	{
		public Task<List<Submission>> GetSubmissions(Guid STDid, CancellationToken cancellationToken);

		public Task<int> RetakeAttempted(Guid STDid, Guid Lvlid, CancellationToken cancellationToken);
	}
}
