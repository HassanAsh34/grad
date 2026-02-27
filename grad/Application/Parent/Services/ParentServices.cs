using grad.Application.Auth.Interfaces;
using grad.Application.Common.Interfaces;
using grad.Application.parent.Interfaces;
using grad.Application.Users.Interfaces;
using grad.Application.Common.DTOs;
using grad.Domain.Model;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using grad.Application.parent.DTOs;

namespace grad.Application.parent.Services
{
	public class ParentServices : IParentServices
	{
		private readonly IAuthServices _authServices;
		private readonly IUserServices _userServices;
		private readonly IRepository _repository;
		//private readonly IUowServices  _uowServices;

		public ParentServices(IUserServices userServices,IAuthServices authServices,IRepository repository)
		{
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_authServices = authServices ?? throw new ArgumentNullException(nameof(authServices));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			//_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));
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
				if(studentDTO.Email.IsNullOrEmpty())
					GenerateUserName(studentDTO);
				else
					if(await _authServices.UserExists(studentDTO.Email,cancellationToken: cancellationToken))
					{
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status400BadRequest,
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
		//			StatusCode = StatusCodes.Status400BadRequest,
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
		//					StatusCode = StatusCodes.Status400BadRequest,
		//					Message = "Failed to activate account"
		//				};
		//			}
		//		}
		//	}
		//}
					
				

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
					StatusCode = profileDTOs.Count() > 0 ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
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
			//Student? child = await _repository.GetEntityAsync<Student>(s => s.PID == pid && s.Id == Sid, cancellationToken: cancellationToken);
			//if (child == null)
			//{
			//	return new ResultDTO
			//	{
			//		StatusCode = StatusCodes.Status404NotFound,
			//		Message = "Child not found"
			//	};
			//}
			//else
			//{
			//	ProfileDTO profileDTO = new ProfileDTO
			//	{
			//		Id = child.Id,
			//		Name = $"{child.FName} {child.LName}",
			//		Email = child.EmailorUserName,
			//		BirthDate = child.BirthDate,
			//		Disability = child.Disability.ToString()
			//	};
			//	return new ResultDTO
			//	{
			//		StatusCode = StatusCodes.Status200OK,
			//		Message = "Child profile retrieved successfully",
			//		result = profileDTO
			//	};
			//}
		}


		private void GenerateUserName(RegisterStudentDTO student)// need further improvement
		{
			// Implement username generation logic here
			string shortGuid = Guid.NewGuid().ToString("N")[..8];
			student.Email = $"{student.FName.ToLower()[3]}.{shortGuid}@System.com";
		}

	}
}
