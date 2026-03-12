using Grad.Application.Auth.Interfaces;
using Grad.Application.Common.DTOs;
using Grad.Application.ParentFeatures.DTOs;
using Grad.Application.ParentFeatures.Interfaces;
using Grad.Application.Users.Interfaces;
using Grad.Domain.Model;

namespace Grad.Application.ParentFeatures.Services
{
	public class ParentServices : IParentServices
	{
		private readonly IAuthServices _authServices;
		private readonly IUserServices _userServices;
		private readonly IParentRepository _repository;
		//private readonly IUowServices  _uowServices;

		public ParentServices(IUserServices userServices,IAuthServices authServices,IParentRepository parentRepository)
		{
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_authServices = authServices ?? throw new ArgumentNullException(nameof(authServices));
			_repository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
		}
		public async Task<ResultDTO> registerStudent(RegisterStudentDTO studentDTO, CancellationToken cancellationToken)//need to be tested
		{
			if (studentDTO == null)
			{
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Invalid student data"
				};
			}
			else
			{
				if(string.IsNullOrEmpty(studentDTO.Email))
					GenerateUserName(studentDTO);
				else
					if(await _authServices.UserExists(studentDTO.Email,cancellationToken: cancellationToken))
					{
						return new ResultDTO
						{
							StatusCode = 400,
							Message = "Email is already in use"
						};
					}
				studentDTO.password = BCrypt.Net.BCrypt.HashPassword(studentDTO.password);
				return await _authServices.register(student: studentDTO, cancellationToken: cancellationToken, Reg: true);
			}
		}

		//public async Task<ResultDTO> activateAccount(LoginDTO login, Guid parentId, CancellationToken cancellationToken)
		//{
		//	//var res = await _authServices.login(login, cancellationToken);
		//	string email = login.UsernameorEmail.ToLower().Trim();
		//	Student student = await _repository.GetEntityAsync<Student>(u => u.EmailorUserName.Equals(email)
		//		&& u.PID == parentId, cancellationToken: cancellationToken);
		//	if (student == null)
		//	{
		//		return new ResultDTO
		//		{
		//			Message = "Failed to activate account.",
		//			StatusCode = StatusCodes.Status500InternalServerError
		//		};
		//	}
		//	else
		//	{
		//		if(student.IsActive)
		//			return new ResultDTO
		//			{
		//			StatusCode = 400,
		//			Message = "Student account is already active"
		//			};
		//		else
		//		{
		//			student.Password = BCrypt.Net.BCrypt.HashPassword(login.password);
		//			_repository.UpdateEntityAsync<Student>(student, cancellationToken: cancellationToken);
		//			int res = await _uowServices.SaveChangesAsync();
		//			if (res != 0)
		//			{
		//				return new ResultDTO
		//				{
		//					StatusCode = StatusCodes.Status200OK,
		//					Message = "Account activated successfully"
		//				};
		//			}
		//			else
		//			{
		//				return new ResultDTO
		//				{
		//					StatusCode = 400,
		//					Message = "Failed to activate account"
		//				};
		//			}
		//		}
		//	}
		//}



		public async Task<ResultDTO> ShowChildren(Guid pid, CancellationToken cancellationToken)
		{
			IEnumerable<Student> children = await _repository.showChildren(pid, cancellationToken: cancellationToken);
			IEnumerable<ProfileDTO> profileDTOs = new List<ProfileDTO>();
			if (children == null || !children.Any())
			{
				return new ResultDTO
				{
					StatusCode = 404,
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
						FName = child.FName,
						LName = child.LName,
						Email = child.EmailorUserName,
						pfpURL = child.ProfilePicture,
						BirthDate = child.BirthDate,
						Disability = child.Disability.ToString()
					});
				}
				return new ResultDTO
				{
					StatusCode = profileDTOs.Count() > 0 ? 200 : 400,
					Message = profileDTOs.Count() > 0 ? "Children retrieved successfully" : "No children were found",
					result = profileDTOs
				};
			}
		}

		public async Task<ResultDTO> viewProfile(Guid pid,Guid Sid, CancellationToken cancellationToken)
		{
			ProfileDTO profileDTO = new ProfileDTO
			{
				Id = Sid,
				Role = "Student"
			};
			return await _userServices.ViewProfile(profileDTO, pid,cancellationToken: cancellationToken);
		}


		private void GenerateUserName(RegisterStudentDTO student)// need further improvement
		{
			// Implement username generation logic here
			string shortGuid = Guid.NewGuid().ToString("N")[..8];
			student.Email = $"{student.FName.ToLower()[3]}.{shortGuid}@System.com";
		}

	}
}
