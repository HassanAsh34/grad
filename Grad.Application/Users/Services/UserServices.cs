using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.Users.DTOs;
using Grad.Application.Users.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Grad.Application.Users.Services
{
	public class UserServices : IUserServices
	{
		private readonly IUserRepository _userRepository;
		private readonly IUowServices _uow;
		private readonly ICloudinaryServices _cloudinaryServices;
		private readonly ILogger<UserServices> _logger;

		public UserServices(IUserRepository userRepository, IUowServices uow, ICloudinaryServices cloudinaryServices, ILogger<UserServices> logger)
		{
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
					await storeImage(user, editProfile.Image, $"{user.Id}.png", cancellationToken);

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
							profile.cvPath = teacher.cvPath;
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

		public async Task<ResultDTO> DeleteProfile(ProfileDTO profile, Guid? parentAccss, bool deleteAll,bool admin, CancellationToken cancellationToken)
		{
			if (profile == null)
			{
				return new ResultDTO
				{
					StatusCode = 500,
					Message = "Something Went Wrong"
				};
			}
			else
			{
				User user = await _userRepository.GetEntityAsync<User>(u => u.Id == profile.Id, cancellationToken: cancellationToken);
				int res = 0;
				if (user == null)
				{
					return new ResultDTO
					{
						StatusCode = 404,
						Message = "User isn't found"
					};
				}
				else
				{
					switch (user.Role)
					{
						case UserRole.Admin:
							return new ResultDTO
							{
								StatusCode = 403,
								Message = "You can't delete an admin account"
							};
						case UserRole.Parent:
							Parent parent = await _userRepository.GetParentWithStudentsAsync(profile.Id, ct: cancellationToken);
							_userRepository.DeleteEntityAsync<Parent>(parent, cancellationToken: cancellationToken);
							if (deleteAll)
							{
								if (parent.students != null && parent.students.Count > 0)
								{
									foreach (Student std in parent.students)
									{
										_userRepository.DeleteEntityAsync<Student>(std, cancellationToken: cancellationToken);
									}
									await Task.WhenAll(parent.students.Select(s => deleteImage($"{s.Id}", cancellationToken)));
								}
							}
							else
							{
								if (parent.students != null && parent.students.Count > 0)
								{
									foreach (Student std in parent.students)
									{
										std.PID = null;
										_userRepository.UpdateEntityAsync<Student>(std, cancellationToken);
									}	
								}
							}
							if (parent.ProfilePicture != null)
							{
								if (await deleteImage(parent.ProfilePicture, cancellationToken))
								{
									res = await _uow.SaveChangesAsync();
								}
							}
							else
							{
								res = await _uow.SaveChangesAsync();
							}
							return new ResultDTO
							{
								StatusCode = 200,
								Message = res != 0 ? "Profile was deleted successfully" : "No changes were made"
							};
						case UserRole.Student:
							Student student = await _userRepository.GetEntityAsync<Student>(s => s.Id == profile.Id, cancellationToken: cancellationToken);
							if (student.PID != null || admin)
							{
								if ((parentAccss != null && student.PID == parentAccss) || admin)
								{ 
									_userRepository.DeleteEntityAsync<Student>(student, cancellationToken: cancellationToken);
									if (student.ProfilePicture != null)
									{
										if (await deleteImage(student.ProfilePicture, cancellationToken))
										{
											res = await _uow.SaveChangesAsync();
										}
									}
									else
										res = await _uow.SaveChangesAsync();
									return new ResultDTO
									{
										StatusCode = 200,
										Message = res != 0 ? "Profile was deleted successfully" : "No changes were made"
									};
								}
								else
								{
									return new ResultDTO
									{
										StatusCode = 403,
										Message = "You don't have access to delete this account"
									};
								}
							}
							else
							{
								_userRepository.DeleteEntityAsync<Student>(student, cancellationToken: cancellationToken);
								if (student.ProfilePicture != null)
								{
									if (await deleteImage(student.ProfilePicture, cancellationToken))
									{
										res = await _uow.SaveChangesAsync();
									}
								}
								else
									res = await _uow.SaveChangesAsync();
								return new ResultDTO
								{
									StatusCode = 200,
									Message = res != 0 ? "Profile was deleted successfully" : "No changes were made"
								};
							}
						case UserRole.Teacher:
							Teacher teacher = await _userRepository.GetEntityAsync<Teacher>(t => t.Id == profile.Id, cancellationToken: cancellationToken);
							_userRepository.DeleteEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
							if (teacher.ProfilePicture != null)
							{
								if (await deleteImage(teacher.ProfilePicture, cancellationToken))
								{
									res = await _uow.SaveChangesAsync();
								}
							}
							if(!string.IsNullOrEmpty(teacher.cvPath))
							{
								if (await _cloudinaryServices.DeleteAsync($"teacher/{teacher.EmailorUserName}/", false, true, cancellationToken))
									res = await _uow.SaveChangesAsync();
							}
							else
								res = await _uow.SaveChangesAsync();
			
							return new ResultDTO
							{
								StatusCode = 200,
								Message = res != 0 ? "Profile was deleted successfully" : "No changes were made"
							};
						default:
							return new ResultDTO
							{
								StatusCode = 403,
								Message = "Invalid action"
							};
					}
				}
			}
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

		private async Task<bool> deleteImage(string publicId, CancellationToken cancellationToken = default)
		{
			string directory = $"user/{publicId}";
			if (!string.IsNullOrEmpty(publicId))
			{
				return await _cloudinaryServices.DeleteAsync(directory, cancellationToken: cancellationToken);
			}
			return false;
		}
	}
}

