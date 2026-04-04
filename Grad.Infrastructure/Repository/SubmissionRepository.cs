using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Domain.Model;
using Grad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Grad.Application.SubmissionFeatures.Interfaces;
using Microsoft.Extensions.Options;

namespace Grad.Infrastructure.Repository
{
	public class SubmissionRepository :  Repository , ISubmissionRepository
	{
		private Db_Context _Context;
		public SubmissionRepository(Db_Context dbContext) : base(dbContext)
		{
			_Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<List<Submission>> GetSubmissions(Guid STDid,CancellationToken cancellationToken)
		{
			return await _Context.submissions.Where(s => s.SubmittedBy == STDid).Include(s => s.Subject).ToListAsync(cancellationToken);
		}

		
	}
}
