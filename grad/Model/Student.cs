namespace grad.Model
{
	public class Student : User
	{
		public String FName { get; set; }

		public String LName { get; set; }

		public DateOnly BirthDate { get; set; }
		public int age { get; set; }

		public DisablityType Disability { get; set; }

		public enum DisablityType
		{
			None,
			Hearing,
			Speech
		}

		//public string Address { get; set; }

		//public string NationalID { get; set; }

		public string PID { get; set; } //pfk

		//public string classroomId { get; set; } //cfk

		public Parent parent { get; set; }
	}
}
