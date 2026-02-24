using System.Text.Json.Serialization;

namespace Grad_Structured.Application.teacher.DTOs
{
	public class LISTStudentDTO
	{
		public Guid Id { get; set; }
		public string name { get; set; }
		public string email { get; set; }
		public parentInfo parentInfo { get; set; }
	}
	public class parentInfo
	{
		public string phoneNumber { get; set; }
		public string Email { get; set; }
	}
}
