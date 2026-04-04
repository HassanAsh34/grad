using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.DTOs;
using Grad.Application.SubmissionFeatures.DTOs;

namespace Grad.Application.SubmissionFeatures.Interfaces
{
	public interface ISubmissionServices
	{
		public Task<ResultDTO> createSubmission(CreateSubmissionDTO createSubmission, CancellationToken CT);
	}
}
