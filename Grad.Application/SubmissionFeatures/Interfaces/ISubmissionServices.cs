using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.DTOs;
using Grad.Application.SubmissionFeatures.DTOs;
using Grad.Domain.Model;

namespace Grad.Application.SubmissionFeatures.Interfaces
{
	public interface ISubmissionServices
	{
		public Task<ResultDTO> createSubmission(CreateSubmissionDTO createSubmission, CancellationToken CT = default);

		public Task<List<SubmissionDTO>> GetSubmissions(Guid STDid,Guid ?sid = null,CancellationToken cancellationToken = default);
	}
}
