using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.QAFeature.DTO;
using Grad.Application.QAFeature.Interfaces;

namespace Grad.Application.QAFeature.Services
{
	public class CommunicationServices
	{
		private readonly IInqueryRepository _inqueryRepository;

		public CommunicationServices(IInqueryRepository inqueryRepository)
		{
			_inqueryRepository = inqueryRepository ?? throw new ArgumentNullException(nameof(inqueryRepository));
		}

		//continue the rest of service methods here

		public async IEnumerable<InqueryDTO> GetInqueries(Guid? id,bool Admin = false,bool teacher = false,CancellationToken ct = default)
		{
			var inqueries = await _inqueryRepository.GetInqueries(id, Admin, teacher, ct);
			// Map the domain model to DTO

		}
	}
}
