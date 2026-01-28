using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Grpc.Core;
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
		public async Task<ResultDTO> registerStudent(RegisterStudentDTO studentDTO, CancellationToken cancellationToken)//need to be tested
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
				GenerateUserName(studentDTO);
				return await _userServices.register(student: studentDTO, cancellationToken: cancellationToken, Reg: true);
			}
		}

		public async Task<ResultDTO> activateAccount(LoginDTO login, Guid parentId, CancellationToken cancellationToken)
		{
			//var res = await _userServices.login(login, cancellationToken);
			string email = login.UsernameorEmail.ToLower().Trim();
			Student student = await _repository.GetEntityAsync<Student>(u => u.EmailorUserName.Equals(email)
				&& u.PID == parentId, cancellationToken: cancellationToken);
			if (student == null)
			{
				return new ResultDTO
				{
					Message = "Failed to activate account.",
					StatusCode = StatusCodes.Status500InternalServerError
				};
			}
			else
			{
				if(student.IsActive)
					return new ResultDTO
					{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Student account is already active"
					};
				else
				{
					student.Password = BCrypt.Net.BCrypt.HashPassword(login.password);
					_repository.UpdateEntityAsync<Student>(student, cancellationToken: cancellationToken);
					int res = await _uowServices.SaveChangesAsync();
					if (res != 0)
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
		}
					
				

		public async Task<ResultDTO> ShowChildren(Guid pid, CancellationToken cancellationToken)
		{
			IEnumerable<Student> children = await _repository.GetEntitiesAsync<Student>((s => s.PID == pid), cancellationToken: cancellationToken);
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
						Email = child.EmailorUserName,
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


		private void GenerateUserName(RegisterStudentDTO student)// need further improvement
		{
			// Implement username generation logic here
			string shortGuid = Guid.NewGuid().ToString("N")[..8];
			student.Username = $"{student.FName.ToLower()[3]}.{shortGuid}@System.com";
		}

	}
}
