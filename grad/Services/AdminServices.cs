using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace grad.Services
{
	public class AdminServices : IAdminServices
	{
		private readonly IRepository _repository;
		private readonly IUserServices _userServices;
		private readonly ISubjectServices _subjectServices;
		private readonly IUowServices _uowServices;

		public AdminServices(IRepository repository,IUserServices userServices, ISubjectServices subjectServices, IUowServices uowServices)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));	
		}

		//public async Task<ResultDTO> ActivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken)
		//{
		//	Teacher teacher = await _repository.GetEntityAsync<Teacher>(t => t.Id.ToLower().Equals(profileDTO.Id.ToLower()), cancellationToken: cancellationToken);
		//	if (teacher == null)
		//		return new ResultDTO
		//		{
		//			Message = "Teacher not found",
		//			StatusCode = StatusCodes.Status404NotFound
		//		};
		//	else
		//	{
		//		if(teacher.IsActive)
		//			return new ResultDTO
		//			{
		//				Message = "Teacher is already activated",
		//				StatusCode = StatusCodes.Status400BadRequest
		//			};
		//		teacher.IsVerified = true;
		//		teacher.IsActive = true;
		//		_repository.UpdateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
		//		int res = await _uowServices.SaveChangesAsync();
		//		return new ResultDTO
		//		{
		//			Message = res != 0 ? "Teacher activated successfully" : "Failed to activate teacher",
		//			StatusCode = res != 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError
		//		};
		//	}
		//}


		
		public async Task<ResultDTO> ToggleBan(Guid UID, CancellationToken cancellationToken) // need to be fixed
		{
			User user = await _repository.GetEntityAsync<User>(u=>u.Id == UID,q=>q.Include(u=>u.RefreshToken) ,cancellationToken: cancellationToken);
			if (user == null)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "User not found"
				};
			}
			else
			{
				if(user.status == User.Status.Banned)
				{
					user.status = User.Status.Active;
					if (user.RefreshToken != null)
						user.RefreshToken.Revoked = true;
				}
				else
				{
					user.status = User.Status.Banned;
				}
				//user.IsVerified = false;
				_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				int res = await _uowServices.SaveChangesAsync();
				return new ResultDTO
				{
					StatusCode = res != 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError,
					//Message = res != 0 ? "" : "Failed to block user"
				};
			}
		}

		
		
		//public async Task<ResultDTO> DeactivateTeacher(string UID, CancellationToken cancellationToken) //not implemented yet
		//{
		//	Teacher user = await _repository.GetEntityAsync<Teacher>(u => u.Id == UID, q => q.Include(u => u.RefreshToken), cancellationToken: cancellationToken);
		//	if (user == null)
		//	{
		//		return new ResultDTO
		//		{
		//			StatusCode = StatusCodes.Status404NotFound,
		//			Message = "User not found"
		//		};
		//	}
		//	else
		//	{
		//		user.IsActive = false;
		//		if (user.RefreshToken != null)
		//			user.RefreshToken.Revoked = true;
		//		user.IsVerified = false;
		//		_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
		//		int res = await _uowServices.SaveChangesAsync();
		//		return new ResultDTO
		//		{
		//			StatusCode = res != 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError,
		//			Message = res != 0 ? "User was blocked successfully" : "Failed to block user"
		//		};
		//	}
		//}

		public async Task<ResultDTO> EndSession(Guid UID, CancellationToken cancellationToken)
		{
			User user = await _repository.GetEntityAsync<User>(u => u.Id == UID, q => q.Include(u => u.RefreshToken), cancellationToken: cancellationToken);
			if (user == null)
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "User not found"
				};
			}
			else
			{
				if(user.RefreshToken != null)
					user.RefreshToken.Revoked = true;
				_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				int res = await _uowServices.SaveChangesAsync();
				return new ResultDTO
				{
					StatusCode = res != 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError,
					Message = res != 0 ? "User session ended successfully" : "Failed to end user session"
				};
			}
		}

		public async Task<ResultDTO> GetAllUsers(string scheme,string host,CancellationToken cancellationToken)
		{
			IEnumerable<User> users = await _repository.GetEntitiesAsync<User>((u => u.Role != 0), q => q.Include(u => u.RefreshToken), cancellationToken: cancellationToken);
			List<ProfileDTO> userProfiles = new List<ProfileDTO>();
			foreach (var item in users)
			{
				//string Url = string.Empty;
				//if(item.ProfilePicture != null)
				//	Url = $"{scheme}://{host}/{item.ProfilePicture}";
				userProfiles.Add(new ProfileDTO
				{
					Id = item.Id,
					Name = item.FName != string.Empty && item.LName != string.Empty ? $"{item.FName} {item.LName}" : string.Empty,
					Email = item.EmailorUserName,
					Status = item.status,
					pfpURL = item.ProfilePicture,
					Role = item.Role.ToString(),
				});
			}
			return new ResultDTO
			{
				StatusCode = userProfiles.Count > 0 ? StatusCodes.Status200OK : StatusCodes.Status204NoContent,
				Message = userProfiles.Count >0 ? "All users returned" : "No Results",
				result = userProfiles
			};
		}

		public async Task<ResultDTO> ViewSubject(Guid sid, CancellationToken cancellation)
		{
			//if(subject != null && !subject.SubjectId.IsNullOrEmpty())
			//{
			//	return await _subjectServices.ViewSubjectAsync(subject, cancellation);
			//}
			//else
			//{
			//	return new ResultDTO
			//	{
			//		Message = "Subject not found",
			//		StatusCode = StatusCodes.Status404NotFound
			//	};
			//}
			return await _subjectServices.ViewSubjectAsync(sid,cancellationToken: cancellation);
		}

		public async Task<ResultDTO> AssignTeacherToSubject(AssignTeacherDTO assignTeacherDTO, CancellationToken cancellationToken)
		{
			Teacher teacher = await _repository.GetEntityAsync<Teacher>(t => t.Id == assignTeacherDTO.TeacherId, cancellationToken: cancellationToken);
			if(!await _subjectServices.IsSubjectExist(subjectId: assignTeacherDTO.SubjectId, cancellationToken: cancellationToken))
			{
				return new ResultDTO
				{
					Message = "invalid subject",
					StatusCode = StatusCodes.Status404NotFound
				};
			}
			if(teacher == null)
			{
				return new ResultDTO
				{
					Message = "Teacher not found",
					StatusCode = StatusCodes.Status404NotFound
				};
			}
			else
			{
				if(teacher.status != User.Status.Active)
				{
					teacher.status = User.Status.Active;
				}
				teacher.SubjectFK = assignTeacherDTO.SubjectId;
				_repository.UpdateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
				int res = await _uowServices.SaveChangesAsync();
				return new ResultDTO
				{
					Message = res != 0 ? "Teacher assigned to subject successfully" : "Failed to assign teacher to subject",
					StatusCode = res != 0 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError
				};
			}
		}

		public Task<ResultDTO> RemoveSubject(Guid sid, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<ResultDTO> UpdateSubject(Guid sid, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<ResultDTO> ViewSubjects(CancellationToken cancellationToken)
		{
			ResultDTO res = await _subjectServices.ViewSubjectsAsync(cancellationToken: cancellationToken);
			return res;
		}

		public async Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			if (subject != null)
			{
				return await _subjectServices.AddSubject(subject, cancellationToken);
			}
			else
			{
				return new ResultDTO
				{
					Message = "invalid",
					StatusCode = StatusCodes.Status400BadRequest
				};
			}
		}

		public async Task<ResultDTO> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			return await _userServices.ViewProfile(profileDTO,adminview: true,cancellationToken: cancellationToken);
		}
	}
}
