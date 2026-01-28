namespace grad.Model
{
	public class Enrollement
	{
		public Guid Id { get; private set; } = Guid.NewGuid();

		public Guid STUFK { get; set; }

		public Guid SUBFK { get; set; }

		public Subject subject { get; set; }

		public Student Student { get; set; }

		public DateTime Enrolled_At { get; private set; } = DateTime.UtcNow;

		//public int progress { get; set; } = 0;
	}
}
