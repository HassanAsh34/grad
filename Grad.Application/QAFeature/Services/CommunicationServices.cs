using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Application.Common.Interfaces;
using Grad.Application.QAFeature.DTO;
using Grad.Application.QAFeature.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.QAFeature.Services
{
	public class CommunicationServices : ICummunicationServices
	{
		private readonly IInqueryRepository _inqueryRepository;
		private readonly IRedisServices _redisServices;

		public CommunicationServices(IInqueryRepository inqueryRepository, IRedisServices redisServices)
		{
			_inqueryRepository = inqueryRepository ?? throw new ArgumentNullException(nameof(inqueryRepository));
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
		}

		//continue the rest of service methods here

		public async Task<IEnumerable<InqueryDTO>> GetInqueries(Guid? id,bool Admin = false,bool teacher = false,CancellationToken ct = default)
		{
			return (await _inqueryRepository.GetInqueries(id,Admin,teacher,ct)).Select(inquery => new InqueryDTO
			{
				Id = inquery.Id,
				RepliedToId = inquery.RepliedToId,
				Submitted_By = inquery.SubmitterName,
				Type = inquery.Type,
				Submitted_At = inquery.Submitted_At,
				Resovled_AT = inquery.Resovled_AT,
				Status = inquery.Status
			}).ToList();
			// Map the domain model to DTO
		}

		public async Task<InqueryDTO> ViewInquery(Guid id, CancellationToken ct = default)
		{
			Dictionary<Guid,InqueryDTO> inqueries = (await _inqueryRepository.GetInquery(id, ct)).Select(inquery => new InqueryDTO
			{
				Id = inquery.Id,
				Message = inquery.Message,
				QuestionType = inquery.QuestionType,
				Resovled_AT = inquery.Resovled_AT,
				Status = inquery.Status,
				Submitted_By = inquery.SubmitterName,
				Submitted_At = inquery.Submitted_At,
				Type = inquery.Type,
				SubjectID = inquery.SubjectID,
				QuestionID = inquery.QuestionID,
			}).ToDictionary(inquery => inquery.Id, inquery => inquery);
			InqueryDTO inquery = inqueries[id];
			inqueries.Remove(id);
			inquery.Replies = inqueries.Values.ToList();
			return inquery;
		}
		
		public async Task<bool> createInquery(InqueryDTO inquery,InqueryStatus? status,CancellationToken cancellationToken)
		{
			Inquery inqueryToCreate = new Inquery
			{
				Id = inquery.Id,
				Message = inquery.Message,
				QuestionType = inquery.QuestionType,
				Resovled_AT = inquery.Resovled_AT,
				Status = inquery.Status,
				submitterId = inquery.submitterId,
				SubmitterName = inquery.Submitted_By,
				Submitted_At = inquery.Submitted_At,
				Type = inquery.Type,
				SubjectID = inquery.SubjectID,
				QuestionID = inquery.QuestionID,
				RepliedToId = inquery.RepliedToId
			};
			if(status !=  null && (inqueryToCreate.RepliedToId != null || inquery.RepliedToId != Guid.Empty))
			{
				await _inqueryRepository.updateInqueryStatusAsync(inquery.RepliedToId.Value,status.Value,cancellationToken);
			}
			int res = await _inqueryRepository.CreateInqueryAsync(inqueryToCreate, cancellationToken);
			return res > 0;
		}

		
	}
}