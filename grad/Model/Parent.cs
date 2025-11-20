namespace grad.Model
{
	public class Parent : User
	{
		public String FName { get; set; }

		public String LName { get; set; }

		public string phoneNumber { get; set; }

		public string Address { get; set; }

		public string Job { get; set; }

		//public string NationalID { get; set; }

		public ReletationShip reletationShip { get; set; }

		public IEnumerable<Student> students { get; set; } 

		public enum ReletationShip
		{
			Father,
			Mother,
			Guardian
		}
	}
}
