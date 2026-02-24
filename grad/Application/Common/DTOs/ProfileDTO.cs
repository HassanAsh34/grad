using System.Text.Json.Serialization;
using Grad_Structured.Domain.Enums;
using Grad_Structured.Domain.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Grad_Structured.Application.Common.DTOs
{
	public class ProfileDTO
	{
		public Guid Id { get; set; }

		public string ?FName { get; set; }

		public string? LName { get; set; }

		public string ?Email { get; set; }

		public Status ?Status { get; set; }

		public string ?Address { get; set; }

		public string ?phone { get; set; }

		//public bool banned { get; set; } =====

		public string Role { get; set; }

		public DateOnly ?BirthDate { get; set; }

		public string ?Disability { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		public Parent? Parent { get; private set; }

		//[JsonIgnore]
		//public string ?pfpPath { get; set; }
		
		public string ?pfpURL { get; set; }

		public string ?Teaches { get; set; }

		public string ?Job { get; set; }

		public int? subjectsCount { get; set; } = 0;

		public void setParent(Parent p)
		{
			Parent = p;
		}

		//public Student Student { get; set; }


	}
}
