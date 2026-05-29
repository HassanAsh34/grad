using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.QAFeature.DTO;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.QAFeature.Interfaces
{
	public interface ICummunicationServices
	{
		public Task<IEnumerable<InqueryDTO>> GetInqueries(Guid? id, bool Admin = false, bool teacher = false, CancellationToken ct = default);
		public Task<InqueryDTO> ViewInquery(Guid id, CancellationToken ct = default);

		public Task<bool> createInquery(InqueryDTO inquery, InqueryStatus? status = null, CancellationToken cancellationToken = default);
	}
}
