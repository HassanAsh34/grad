using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.QAFeature.Interfaces
{
	public interface IInqueryRepository : IRepository
	{
		Task<int> CreateInqueryAsync(Inquery inquery, CancellationToken CT = default);
		Task<IEnumerable<Inquery>> GetInquery(Guid Id, CancellationToken CT = default);
		Task<IEnumerable<Inquery>> GetInqueries(Guid? Id, bool Admin = false, bool teacher = false, CancellationToken CT = default);
		Task<int> updateInqueryStatusAsync(Guid Id, InqueryStatus status, CancellationToken CT = default);
		Task<bool> deleteInqueryAsync(Guid Id, CancellationToken CT = default);
		Task<bool> DeleteInqueryBySubject(Guid sid, CancellationToken CT = default);
	}
}
