using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grad.Domain.Enums;

namespace Grad.Application.QAFeature.DTO
{
	public class InqueryDTO
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid? RepliedToId { get; set; }

		public string Submitted_By { get; set; }

		public InqueryType Type { get; set; }

		public Guid? SubjectID { get; set; } = Guid.Empty;

		public string Message { get; set; }

		public QuestionType QuestionType { get; set; } = QuestionType.other;

		public Guid? QuestionID { get; set; } = Guid.Empty;

		//public Guid? RecipientID { get; set; } = Guid.Empty;

		public DateTime Submitted_At { get; set; } = DateTime.UtcNow;

		public InqueryStatus? Status { get; set; } 
	}
}
