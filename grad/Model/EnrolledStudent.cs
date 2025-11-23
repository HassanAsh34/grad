namespace grad.Model
{
	public class EnrolledStudent
	{
		public string Id { get; private set; } = Guid.NewGuid().ToString();

		public string STUFK { get; set; }

		public string SUBFK { get; set; }

		public Subject subject { get; set; }

		public Student Student { get; set; }
	}
}
