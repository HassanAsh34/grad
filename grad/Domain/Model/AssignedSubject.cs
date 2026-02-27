namespace grad.Domain.Model
{
	public class AssignedSubject
	{
		public int Id { get; set; }
		public Guid TeacherId { get; set; }
		public Guid SubjectId { get; set; }

		public DateOnly Assigned_At { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);
		public Teacher Teacher { get; set; }
		public Subject Subject { get; set; }
	}
}
