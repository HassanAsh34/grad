using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.Interfaces;
using Grad.Infrastructure.Persistence;
using Grad.Domain.Model;
using Grad.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Grad.Application.QAFeature.Interfaces;

namespace Grad.Infrastructure.Repository
{
	public class InqueryRepository :  Repository , IInqueryRepository
	{
		Db_Context _context;
		IUowServices _UowServices;
		public InqueryRepository(Db_Context context, IUowServices uow) : base(context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_UowServices = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<int> CreateInqueryAsync(Inquery inquery, CancellationToken CT)
		{
			base.CreateEntityAsync<Inquery>(inquery, CT);
			return await _UowServices.SaveChangesAsync();
		}

		public async Task<IEnumerable<Inquery>> GetInquery(Guid Id,CancellationToken CT)
		{
			return await base.GetEntitiesAsync<Inquery>(i => i.Id == Id || i.RepliedToId == Id, CT);
		}
		


		public async Task<IEnumerable<Inquery>> GetInqueries(Guid? Id, bool Admin = false, bool teacher = false,CancellationToken CT = default)
		{
			if(Admin)
			{
				return await base.GetEntitiesAsync<Inquery>(i => i.Type == InqueryType.Suggestion || i.Type == InqueryType.Complaint || i.Type == InqueryType.TechnicalSupport, CT);
			}
			else if(teacher)
			{
				return await base.GetEntitiesAsync<Inquery>(i => i.SubjectID == Id && i.Type == InqueryType.Question, CT);
			}
			else
				return await base.GetEntitiesAsync<Inquery>(i => i.submitterId == Id , CT);
		}

		public async Task<int> updateInqueryStatusAsync(Guid Id, InqueryStatus status, CancellationToken CT)
		{
			if (!Enum.IsDefined(typeof(InqueryStatus), status))
				return -1; // Invalid status value

			return await _context.Set<Inquery>()
				.Where(i => i.Id == Id)
				.ExecuteUpdateAsync(
					i => i.SetProperty(p => p.Status, status),
					CT);
		}

		public async Task<bool> deleteInqueryAsync(Guid Id, CancellationToken CT)
		{
			base.DeleteEntitiesAsync<Inquery>(i => i.Id == Id || i.RepliedToId == Id);
			return await _UowServices.SaveChangesAsync() > 0;
		}

		public async Task<bool> DeleteInqueryBySubject(Guid sid,CancellationToken CT)
		{
			base.DeleteEntitiesAsync<Inquery>(i => i.SubjectID == sid, CT);
			return await _UowServices.SaveChangesAsync() > 0;
		}

		//implement edit inquery
	}
}
