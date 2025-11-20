using grad.DTO;
using grad.Interfaces;
using grad.Model;
using Microsoft.EntityFrameworkCore;

namespace grad.Services
{
	public class AdminServices : IAdminServices
	{
		private readonly IRepository _repository;
		private readonly IUserServices _userServices;

		public AdminServices(IRepository repository,IUserServices userServices)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
		}

		public async Task<ResultDTO> ActivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			Teacher teacher = await _repository.GetEntityAsync<Teacher>(t => t.Id.ToLower().Equals(profileDTO.Id.ToLower()), cancellationToken: cancellationToken);
			if (teacher == null)
				return new ResultDTO
				{
					Message = "Teacher not found",
					StatusCode = StatusCodes.Status404NotFound
				};
			else
			{
				if(teacher.IsActive)
					return new ResultDTO
					{
						Message = "Teacher is already activated",
						StatusCode = StatusCodes.Status400BadRequest
					};
				teacher.IsVerified = true;
				teacher.IsActive = true;
				bool res = await _repository.UpdateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
				return new ResultDTO
				{
					Message = res == true ? "Teacher activated successfully" : "Failed to activate teacher",
					StatusCode = res == true ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError
				};
			}
		}


		
		public async Task<ResultDTO> BlockUser(ProfileDTO? profileDTO, CancellationToken cancellationToken)
		{
			User user = await _repository.GetEntityAsync<User>(u=>u.Id == profileDTO.Id,q=>q.Include(u=>u.RefreshToken) ,cancellationToken: cancellationToken);
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
				user.IsActive = false;
				if (user.RefreshToken != null)
					user.RefreshToken.Revoked = true;
				user.IsVerified = false;
				bool res = await _repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				return new ResultDTO
				{
					StatusCode = res == true ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError,
					Message = res == true ? "User was blocked successfully" : "Failed to block user"
				};
			}
		}

		public Task<ResultDTO> DeactivateTeacher(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<ResultDTO> EndSession(ProfileDTO? profileDTO, CancellationToken cancellationToken)
		{
			User user = await _repository.GetEntityAsync<User>(u => u.Id == profileDTO.Id, q => q.Include(u => u.RefreshToken), cancellationToken: cancellationToken);
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
				bool res = await _repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				return new ResultDTO
				{
					StatusCode = res == true ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError,
					Message = res == true ? "User session ended successfully" : "Failed to end user session"
				};
			}
		}

		public async Task<ResultDTO> GetAllUsers(CancellationToken cancellationToken)
		{
			IEnumerable<User> users = await _repository.GetEntitiesAsync<User>((u => u.Role != 0), q => q.Include(u => u.RefreshToken), cancellationToken: cancellationToken);
			List<ProfileDTO> userProfiles = new List<ProfileDTO>();
			foreach (var item in users)
			{
				userProfiles.Add(new ProfileDTO
				{
					Id = item.Id,
					Email = item.EmailorUserName,
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

		public Task<ResultDTO> RemoveSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<ResultDTO> UpdateSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<ResultDTO> ViewSubjects(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
			//IEnumerable<Subject> subjects = await _repository.GetEntitiesAsync<Subject>(,q=>q.Include(s=> s.Students).ThenInclude(s=>s.);	
		}

		public async Task<ResultDTO> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			return await _userServices.ViewProfile(profileDTO, cancellationToken);
		}
	}
}
