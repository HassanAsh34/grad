namespace grad.Model
{
	public class Subject
	{

		public string Id { get; private set; } = Guid.NewGuid().ToString();

		public string Name { get; set; }

		public bool deaf_mute { get; set; } 

		public DateOnly CreatedAt { get; private set; } = DateOnly.FromDateTime(DateTime.Now);

		public DateOnly UpdatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

		//public IEnumerable<Lesson> Lessons { get; set; } //lessons related to the subject

		public IEnumerable<Teacher> Teachers { get; set; } //resposible teachers

		public IEnumerable<EnrolledStudent> Students { get; set; } //students enrolled in the class

		//private enum Subject
		//{
		//	arabic,
		//	english,
		//	mathematics,
		//	science,
		//}
	}

}
