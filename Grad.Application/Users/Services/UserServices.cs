using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.Users.DTOs;
using Grad.Application.Users.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.AspNetCore.Http;

namespace Grad.Application.Users.Services
{
	public class UserServices : IUserServices
	{
		private readonly IUserRepository _userRepository;
		private readonly IUowServices _uow;
		private readonly ICloudinaryServices _cloudinaryServices;

		public UserServices(IUserRepository userRepository, IUowServices uow, ICloudinaryServices cloudinaryServices)
		{
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
		}

		public async Task<ResultDTO> EditProfile(EditProfileDTO editProfile, CancellationToken cancellationToken)
		{
			if (editProfile == null)
			{
				return new ResultDTO
				{
					StatusCode = 400,
					Message = "Invalid profile data"
				};
			}

			if (Enum.TryParse<UserRole>(editProfile.Role, out var role))
			{
				User? user = null;
				switch (role)
				{
					case UserRole.Parent:
						Parent parent = await _userRepository.GetParentWithStudentsAsync(editProfile.Id ?? Guid.Empty, ct: cancellationToken);
						if (parent != null)
						{
							parent.FName = editProfile.FName ?? parent.FName;
							parent.LName = editProfile.lName ?? parent.LName;
							parent.Address = editProfile.Address ?? parent.Address;
							parent.phoneNumber = editProfile.phone ?? parent.phoneNumber;
							parent.Job = editProfile.Job ?? parent.Job;
							_userRepository.UpdateEntityAsync<Parent>(parent, cancellationToken: cancellationToken);
							user = parent;
						}
						break;

					case UserRole.Teacher:
						Teacher? teacher = await _userRepository.GetTeacherWithAssignedSubjectsAsync(editProfile.Id ?? Guid.Empty, ct: cancellationToken);
						if (teacher != null)
						{
							teacher.FName = editProfile.FName ?? teacher.FName;
							teacher.LName = editProfile.lName ?? teacher.LName;
							teacher.Address = editProfile.Address ?? teacher.Address;
							teacher.phoneNumber = editProfile.phone ?? teacher.phoneNumber;
							_userRepository.UpdateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
							user = teacher;
						}
						break;

					case UserRole.Student:
						Student? student = await _userRepository.GetStudentWithParentAndSubjectsAsync(editProfile.Id ?? Guid.Empty, ct: cancellationToken);
						if (student != null)
						{
							student.FName = editProfile.FName ?? student.FName;
							student.LName = editProfile.lName ?? student.LName;
							_userRepository.UpdateEntityAsync<Student>(student, cancellationToken: cancellationToken);
							user = student;
						}
						break;

					default:
						return new ResultDTO
						{
							StatusCode = 403,
							Message = "Invalid action"
						};
				}

				if (user == null)
				{
					return new ResultDTO
					{
						StatusCode = 500,
						Message = "Something went wrong"
					};
				}

				int res = await _uow.SaveChangesAsync();
				
				if (editProfile.Image != null)
					await storeImage(user,editProfile.Image,$"{user.Id}.png", cancellationToken);

				return new ResultDTO
				{
					StatusCode = 200,
					Message = res != 0 ? "Profile was updated successfully" : "No changes were made"
				};
			}

			return new ResultDTO
			{
				StatusCode = 403,
				Message = "Invalid action"
			};
		}

		public async Task<ResultDTO> ViewProfile(ProfileDTO profile, Guid? pid, bool adminview, CancellationToken cancellationToken)
		{
			if (profile == null)
			{
				return new ResultDTO
				{
					StatusCode = 500,
					Message = "Something Went Wrong"
				};
			}

			if (Enum.TryParse<UserRole>(profile.Role, out var role))
			{
				switch (role)
				{
					case UserRole.Admin:
						Admin? admin = await _userRepository.GetAdminByIdAsync(profile.Id, ct: cancellationToken);
						if (admin != null)
						{
							profile.Email = admin.EmailorUserName;
							profile.FName = "Admin";
							profile.pfpURL = admin.ProfilePicture ?? string.Empty;
							profile.Status = adminview ? admin.status : null;
							profile.phone = admin.phoneNumber;
							return new ResultDTO { Message = "Found", StatusCode = 200, result = profile };
						}
						break;

					case UserRole.Parent:
						Parent? parent = await _userRepository.GetParentWithStudentsAsync(profile.Id, ct: cancellationToken);
						if (parent != null)
						{
							profile.Email = parent.EmailorUserName;
							profile.Role = parent.Role.ToString();
							profile.FName = parent.FName;
							profile.LName = parent.LName;
							profile.BirthDate = parent.BirthDate;
							profile.Address = parent.Address;
							profile.phone = parent.phoneNumber;
							profile.pfpURL = parent.ProfilePicture;
							profile.Job = parent.Job;
							profile.Status = adminview ? parent.status : null;
							return new ResultDTO { Message = "Found", StatusCode = 200, result = profile };
						}
						break;

					case UserRole.Student:
						Student? student = await _userRepository.GetStudentWithParentAndSubjectsAsync(profile.Id, pid, ct: cancellationToken);
						if (student != null)
						{
							profile.Email = student.EmailorUserName;
							profile.Role = student.Role.ToString();
							profile.FName = student.FName;
							profile.Job = UserRole.Student.ToString();
							profile.phone = student.phoneNumber;
							
							if (student.parent != null)
							{
								profile.LName = student.parent.FName;
								profile.Address = student.parent.Address;
							}
							else
							{
								profile.Address = student.Address;
								profile.LName = student.LName;
							}
							
							profile.subjectsCount = student.EnrolledSubjects?.Count() ?? 0;
							profile.BirthDate = student.BirthDate;
							profile.Disability = student.Disability.ToString();
							profile.pfpURL = student.ProfilePicture;
							profile.Status = adminview ? student.status : null;
							
							if (student.parent != null)
							{
								profile.ParentContactInfo = new ParentContactInfo
								{
									ParentEmail = student.parent.EmailorUserName,
									ParentFName = student.parent.FName,
									phoneNumber = student.parent.phoneNumber,
									reletationShip = student.parent.reletationShip.ToString(),
									PProfilePicture = student.parent.ProfilePicture,
								};
							}
							return new ResultDTO { Message = "Found", StatusCode = 200, result = profile };
						}
						break;

					case UserRole.Teacher:
						Teacher? teacher = await _userRepository.GetTeacherWithAssignedSubjectsAsync(profile.Id, ct: cancellationToken);
						if (teacher != null)
						{
							profile.Email = teacher.EmailorUserName;
							profile.Role = teacher.Role.ToString();
							profile.FName = teacher.FName;
							profile.BirthDate = teacher.BirthDate;
							profile.LName = teacher.LName;
							profile.Address = teacher.Address;
							profile.pfpURL = teacher.ProfilePicture;
							profile.phone = teacher.phoneNumber;
							profile.Status = adminview ? teacher.status : null;
							profile.Job = "Teacher";
							return new ResultDTO { Message = "Found", StatusCode = 200, result = profile };
						}
						break;
				}
			}

			return new ResultDTO
			{
				StatusCode = 404,
				Message = pid != null ? "Child's id is invalid" : "User isn't found"
			};
		}

		private async Task storeImage(User user, IFormFile file, string fileName, CancellationToken cancellationToken = default)
		{
			if (file != null && file.Length > 0)
			{
				string publicId = $"{user.Id}";
				user.ProfilePicture = await _cloudinaryServices.UploadImageAsync(file, "user", publicId, cancellationToken);
				_userRepository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				await _uow.SaveChangesAsync();
			}
		}
	}
}

