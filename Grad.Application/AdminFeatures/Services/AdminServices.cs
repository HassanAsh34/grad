using Grad.Application.Users.Interfaces;
using Grad.Application.Common.Interfaces;
using Grad.Application.Common.DTOs;
using Grad.Application.TeacherFeatures.DTOs;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Domain.Model;
using Grad.Application.AdminFeatures.Interfaces;
using Grad.Domain.Enums;

namespace Grad.Application.AdminFeatures.Services
{
	public class AdminServices : IAdminServices
	{
		private readonly IAdminRepository _adminRepository;
		private readonly IUserServices _userServices;
		private readonly ISubjectServices _subjectServices;
		private readonly IUowServices _uowServices;

		public AdminServices(
			IAdminRepository adminRepository,
			IUserServices userServices, 
			ISubjectServices subjectServices, 
			IUowServices uowServices)
		{
			_adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));	
		}

		public async Task<ResultDTO> ToggleBan(Guid UID, CancellationToken cancellationToken)
		{
			User user = await _adminRepository.getUserWithRefreshToken(UID, cancellationToken);
			if (user == null)
			{
				return new ResultDTO
				{
					StatusCode = 404,
					Message = "User not found"
				};
			}

			if (user.status == Status.Pending)
			{
				return new ResultDTO
				{
					Message = "Teacher's account is still pending activation and cannot be banned",
					StatusCode = 409
				};
			}
			
			if (user.status == Status.Banned)
			{
				user.status = Status.Active;
				if (user.RefreshToken != null)
					user.RefreshToken.Revoked = true;
			}
			else
			{
				user.status = Status.Banned;
			}

			_adminRepository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
			int res = await _uowServices.SaveChangesAsync();
			
			return new ResultDTO
			{
				StatusCode = res != 0 ? 200 : 500,
			};
		}

		public async Task<ResultDTO> EndSession(Guid UID, CancellationToken cancellationToken)
		{
			User user = await _adminRepository.getUserWithRefreshToken(UID, cancellationToken);
			if (user == null)
			{
				return new ResultDTO
				{
					StatusCode = 404,
					Message = "User not found"
				};
			}

			if (user.RefreshToken != null && user.status == Status.Active)
			{
				if (user.RefreshToken.Revoked)
				{
					return new ResultDTO
					{
						StatusCode = 400,
						Message = "User session is already ended"
					};
				}

				user.RefreshToken.Revoked = true;
				_adminRepository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				int res = await _uowServices.SaveChangesAsync();
				
				return new ResultDTO
				{
					StatusCode = res != 0 ? 200 : 500,
					Message = res != 0 ? "User session ended successfully" : "Failed to end user session"
				};
			}

			return new ResultDTO
			{
				StatusCode = 400,
				Message = "User doesn't have any active sessions"
			};
		}

		public async Task<ResultDTO> GetAllUsers(string scheme, string host, CancellationToken cancellationToken)
		{
			// Fetching users who are not Admins (Role != 0)
			IEnumerable<User> users = await _adminRepository.GetEntitiesAsync<User>(u => u.Role != UserRole.Admin, cancellationToken: cancellationToken);
			
			List<ProfileDTO> userProfiles = new List<ProfileDTO>();
			foreach (var item in users)
			{
				userProfiles.Add(new ProfileDTO
				{
					Id = item.Id,
					FName = item.FName ?? string.Empty,
					LName = item.LName ?? string.Empty,
					Email = item.EmailorUserName,
					Status = item.status,
					pfpURL = item.ProfilePicture,
					Role = item.Role.ToString(),
				});
			}

			return new ResultDTO
			{
				StatusCode = userProfiles.Count > 0 ? 200 : 204,
				Message = userProfiles.Count > 0 ? "All users returned" : "No Results",
				result = userProfiles
			};
		}

		public async Task<ResultDTO> ViewSubject(Guid sid, CancellationToken cancellation)
		{
			return await _subjectServices.ViewSubjectAsync(sid, cancellationToken: cancellation);
		}

		public async Task<ResultDTO> AssignTeacherToSubject(TeacherSubjectDTO assignTeacherDTO, CancellationToken cancellationToken)
		{
			Teacher teacher = await _adminRepository.GetEntityAsync<Teacher>(t => t.Id == assignTeacherDTO.TeacherId, cancellationToken: cancellationToken);
			
			if (!await _subjectServices.IsSubjectExist(subjectId: assignTeacherDTO.SubjectId, cancellationToken: cancellationToken))
			{
				return new ResultDTO
				{
					Message = "invalid subject",
					StatusCode = 404
				};
			}

			if (teacher == null)
			{
				return new ResultDTO
				{
					Message = "Teacher not found",
					StatusCode = 404
				};
			}

			if (teacher.status != Status.Active)
			{
				teacher.status = Status.Active;
			}

			AssignedSubject assignedSubject = new AssignedSubject
			{
				SubjectId = assignTeacherDTO.SubjectId,
				TeacherId = assignTeacherDTO.TeacherId
			};

			AssignedSubject assigned = await _adminRepository.GetEntityAsync<AssignedSubject>(
				a => a.SubjectId == assignedSubject.SubjectId && a.TeacherId == assignedSubject.TeacherId, 
				cancellationToken: cancellationToken);

			if (assigned != null)
			{
				return new ResultDTO
				{
					Message = "Teacher is already assigned to that subject",
					StatusCode = 409
				};
			}

			_adminRepository.CreateEntityAsync<AssignedSubject>(assignedSubject, cancellationToken: cancellationToken);
			_adminRepository.UpdateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
			int res = await _uowServices.SaveChangesAsync();

			return new ResultDTO
			{
				Message = res != 0 ? "Teacher assigned to subject successfully" : "Failed to assign teacher to subject",
				StatusCode = res != 0 ? 200 : 500
			};
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
			return await _subjectServices.ViewSubjectsAsync(cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> AddSubject(SubjectDTO subject, CancellationToken cancellationToken)
		{
			if (subject != null)
			{
				return await _subjectServices.AddSubject(subject, cancellationToken);
			}

			return new ResultDTO
			{
				Message = "invalid",
				StatusCode = 400
			};
		}

		public async Task<ResultDTO> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			return await _userServices.ViewProfile(profileDTO, adminview: true, cancellationToken: cancellationToken);
		}
	}
}
