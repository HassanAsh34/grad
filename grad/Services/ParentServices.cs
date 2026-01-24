using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.IdentityModel.Tokens;

namespace grad.Services
{
	public class ParentServices : IParentServices
	{
		private readonly IUserServices _userServices;
		private readonly IRepository _repository;
		private readonly IUowServices  _uowServices;

		public ParentServices(IUserServices userServices,IRepository repository,IUowServices uowServices)
		{
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));
		}
		public async Task<ResultDTO> registerStudent(RegisterStudentDTO studentDTO, CancellationToken cancellationToken)//need to fix pfp
		{
			if (studentDTO == null)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Invalid student data"
				};
			}
			else
			{
				Student student = new Student
				{
					FName = studentDTO.FName,
					LName = studentDTO.Pname.Split(' ')[0],
					age = DateOnly.FromDateTime(DateTime.UtcNow).Year - studentDTO.BirthDate.Year,
					PID = studentDTO.p_Id,
					BirthDate = studentDTO.BirthDate,
					Disability = (Student.DisablityType)studentDTO.Disability,
					Role = User.UserRole.Student,
					IsActive = false
				};
				GenerateUserName(student);
				return await _userServices.register(student: student, cancellationToken: cancellationToken, Reg: true);
			}
		}

		public async Task<ResultDTO> activateAccount(LoginDTO login, CancellationToken cancellationToken)
		{
			//var res = await _userServices.login(login, cancellationToken);
			User user = await _repository.GetEntityAsync<User>((u => u.EmailorUserName.ToLower().Equals(login.UsernameorEmail.ToLower())), cancellationToken: cancellationToken);
			if (user == null)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "User not found"
				};
			}
			else if (user.IsActive || !user.Password.IsNullOrEmpty())
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Account is already active"
				};
			}
			else
			{
				user.Password = BCrypt.Net.BCrypt.HashPassword(login.password);
				_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				int res = await _uowServices.SaveChangesAsync();
				if (res!=0)
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status200OK,
						Message = "Account activated successfully"
					};
				}
				else
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status400BadRequest,
						Message = "Failed to activate account"
					};
				}
			}
		}

		public async Task<ResultDTO> ShowChildren(string? pid, CancellationToken cancellationToken)
		{
			IEnumerable<Student> children = await _repository.GetEntitiesAsync<Student>((s => s.PID == pid),cancellationToken: cancellationToken);
			IEnumerable<ProfileDTO> profileDTOs = new List<ProfileDTO>();
			if (children == null || !children.Any())
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "No children were found"
				};
			}
			else
			{
                foreach (var child in children)
                {
					profileDTOs = profileDTOs.Append(new ProfileDTO
					{
						Id = child.Id,
						Name = $"{child.FName} {child.LName}",
						BirthDate = child.BirthDate,
						Disability = child.Disability.ToString()
					});
				}
				return new ResultDTO
				{
					StatusCode = profileDTOs.Count() > 0 ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
					Message = profileDTOs.Count() > 0 ? "Children retrieved successfully" : "No children were found",
					result = profileDTOs
				};
			}
		}


		private void GenerateUserName(Student student)// need further improvement
		{
			// Implement username generation logic here
			student.EmailorUserName = $"{student.FName.ToLower()[3]}-{student.Id.ToLower()}";
		}

	}
}
