using Grad.Application.AdminFeatures.Interfaces;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.QAFeature.DTO;
using Grad.Application.QAFeature.Interfaces;
using Grad.Application.SubjectFeatures.DTOs;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.TeacherFeatures.DTOs;
using Grad.Application.TeacherFeatures.Interfaces;
using Grad.Application.Users.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.Extensions.Logging;

namespace Grad.Application.AdminFeatures.Services
{
	public class AdminServices : IAdminServices
	{
		private readonly IAdminRepository _adminRepository;
		private readonly IUserServices _userServices;
		private readonly ISubjectServices _subjectServices;
		private readonly IUowServices _uowServices;
		private readonly ILogger<AdminServices> _logger;
		private readonly IRedisServices _redisServices;
		private readonly ICummunicationServices _inqueryServices;

		public AdminServices(
			IAdminRepository adminRepository,
			IUserServices userServices, 
			ISubjectServices subjectServices, 
			IUowServices uowServices,
			ILogger<AdminServices> logger,
			IRedisServices redisServices,ICummunicationServices cummunicationServices)
		{
			_adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_subjectServices = subjectServices ?? throw new ArgumentNullException(nameof(subjectServices));
			_uowServices = uowServices ?? throw new ArgumentNullException(nameof(uowServices));	
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
			_inqueryServices = cummunicationServices ?? throw new ArgumentNullException(nameof(cummunicationServices));
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

		public async Task<ResultDTO> RemoveSubject(Guid sid, CancellationToken cancellationToken)
		{
			return await _subjectServices.RemoveSubject(sid, cancellationToken);
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

		public async Task<ResultDTO> DeleteUser(ProfileDTO profile, bool all,CancellationToken cancellationToken)
		{
			return await _userServices.DeleteProfile(profile,null,all,true, cancellationToken);
		}


		public async Task<ResultDTO> ViewUser(ProfileDTO profileDTO, CancellationToken cancellationToken)
		{
			return await _userServices.ViewProfile(profileDTO, adminview: true, cancellationToken: cancellationToken);
		}

		public async Task<ResultDTO> listInqueries(CancellationToken CT)
		{
			IEnumerable<InqueryDTO> inqueries = await _inqueryServices.GetInqueries(null,Admin: true,ct: CT);
			if (inqueries.Count() > 0)
				return new ResultDTO
				{
					StatusCode = 200,
					Message = $"{inqueries.Count()} inqueries were found",
					result = inqueries
				};
			else
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "there are no inqueries at the moment"
				};
		}

		public async Task<ResultDTO> viewInquery(Guid Id, CancellationToken cancellationToken)
		{
			InqueryDTO inquery = await _inqueryServices.ViewInquery(Id, cancellationToken);
			if (inquery == null)
				return new ResultDTO
				{
					StatusCode = 404,
					Message = "Inquery wasnt found"
				};
			else
			{
				await _redisServices.store(Id.ToString(), BCrypt.Net.BCrypt.HashPassword("admin"), TimeSpan.FromMinutes(50));
				return new ResultDTO
				{
					StatusCode = 200,
					Message = "Inquery retrieved successfully",
					result = inquery
				};
			}
		}

		//public async Task<ResultDTO> ViewCV(Guid UID, CancellationToken cancellationToken)
		//{
		//	Teacher teacher = aw
			
		//}

		public async Task<ResultDTO> createInquery(InqueryDTO inquery, CancellationToken cancellationToken)
		{
			if (inquery.RepliedToId != Guid.Empty || inquery.RepliedToId != null)
			{
				string lockedBy = await _redisServices.get(inquery.RepliedToId.ToString());
				if (lockedBy != null && !BCrypt.Net.BCrypt.Verify("admin", lockedBy))
				{
					return new ResultDTO
					{
						StatusCode = 409,
						Message = "Inquery is currently being viewed or edited by another user"
					};
				}
			}
			//if inquery is a reply and not a new inquery then we need to look for the replied to inquery and check if it exists and if it is not solved yet
			InqueryStatus? status = inquery.RepliedToId == null || inquery.RepliedToId == Guid.Empty ? null : InqueryStatus.Solved;
			if (await _inqueryServices.createInquery(inquery, status, cancellationToken))
			{
				return new ResultDTO
				{
					StatusCode = 201,
					Message = "Inquery created successfully"
				};
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = 500,
					Message = "Failed to create inquery"
				};
			}
		}
	}
}
