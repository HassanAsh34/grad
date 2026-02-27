//using grad.Repositories;
using grad.Application.Users.DTOs;
using grad.Application.Users.Interfaces;
using grad.Domain.Model;
using grad.Application.Common.DTOs;
using grad.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using grad.Application.Common.Interfaces;
//remove comment from email sending parts after testing

namespace grad.Application.Users.Services
{
	public class UserServices : IUserServices
	{
		private readonly IRepository _repository;
		private readonly IUowServices _uow;
		private readonly ICloudinaryServices _cloudinaryServices;

		public UserServices(IRepository repository,IUowServices uow, ICloudinaryServices cloudinaryServices)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_cloudinaryServices = cloudinaryServices ?? throw new ArgumentNullException(nameof(cloudinaryServices));
		}


		public async Task<ResultDTO> EditProfile(EditProfileDTO editProfile,CancellationToken cancellationToken)
		{
			if(editProfile != null)
			{
				if (Enum.TryParse<UserRole>(editProfile.Role, out var role))
				{
					User user = null;	
					switch (role)
					{
						//case UserRole.Admin:
						case UserRole.Parent:
							Parent parent = await _repository.GetEntityAsync<Parent>(p => p.Id == editProfile.Id, cancellationToken: cancellationToken);
							if (parent != null)
							{
								parent.FName = editProfile.FName ?? parent.FName;
								parent.LName = editProfile.lName ?? parent.LName;
								parent.Address = editProfile.Address ?? parent.Address;
								parent.phoneNumber = editProfile.phone ?? parent.phoneNumber;
								parent.Job = editProfile.Job ?? parent.Job;
								//if (editProfile.image != null)
								//	await storeImage(parent, editProfile.image, cancellationToken);
								_repository.UpdateEntityAsync<Parent>(parent, cancellationToken: cancellationToken);
								user = parent;
							}
							else
							{
								return new ResultDTO
								{
									StatusCode = StatusCodes.Status500InternalServerError,
									Message = "Something went wrong"
								};
							}
							break;
						case UserRole.Teacher:
							Teacher teacher = await _repository.GetEntityAsync<Teacher>(t => t.Id == editProfile.Id, cancellationToken: cancellationToken);
							if (teacher != null)
							{
								teacher.FName = editProfile.FName ?? teacher.FName;
								teacher.LName = editProfile.lName ?? teacher.LName;
								teacher.Address = editProfile.Address ?? teacher.Address;
								teacher.phoneNumber = editProfile.phone ?? teacher.phoneNumber;
								_repository.UpdateEntityAsync<Teacher>(teacher, cancellationToken: cancellationToken);
								user = teacher;
							}
							else
							{
								return new ResultDTO
								{
									StatusCode = StatusCodes.Status500InternalServerError,
									Message = "Something went wrong"
								};
							}
							break;
						case UserRole.Student:
							Student student = await _repository.GetEntityAsync<Student>(s => s.Id == editProfile.Id, cancellationToken: cancellationToken);
							if (student != null)
							{
								student.FName = editProfile.FName ?? student.FName;
								student.LName = editProfile.lName ?? student.LName;
								_repository.UpdateEntityAsync<Student>(student, cancellationToken: cancellationToken);
								user = student;
							}
							else
							{
								return new ResultDTO
								{
									StatusCode = StatusCodes.Status500InternalServerError,
									Message = "Something went wrong"
								};
							}
							break;
						default:
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status403Forbidden,
								Message = "invalid action" // unknown role
							};
					}
					int res = await _uow.SaveChangesAsync();
					if (editProfile.image != null)
						await storeImage(user, editProfile.image, cancellationToken);
					if (res != 0)
					{
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status200OK,
							Message = "Profile was updated successfully"
						};
					}
					else
						return new ResultDTO
						{
							StatusCode = StatusCodes.Status200OK,
							Message = "No changes were made"
						};
				}
				else
				{
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status403Forbidden,
						Message = "invalid action" // unknown role
					};
				}
			}
			else
			{
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Invalid profile data"
				};
			}
		}

		public async Task<ResultDTO> ViewProfile(ProfileDTO user,Guid ?pid,bool adminview,CancellationToken cancellationToken)//need to be fixed to show user pfp and add teacher 
		{
			bool found = false;
			if (user != null)
			{
				if (Enum.TryParse<UserRole>(user.Role, out var role))
				{
					switch (role)
					{
						case UserRole.Admin:
							Admin admin = await _repository.GetEntityAsync<Admin>(u => u.Id == user.Id,cancellationToken: cancellationToken);
							if(admin != null) 
							{
								user.Email = admin.EmailorUserName;
								user.FName = "Admin";
								user.pfpURL = admin.ProfilePicture == null ? string.Empty : admin.ProfilePicture;
								user.Status = adminview ? admin.status : null;
								user.phone = admin.phoneNumber;
								found = true;
							}
							break;
						case UserRole.Parent:
							Parent parent = await _repository.GetEntityAsync<Parent>(u => u.Id == user.Id, cancellationToken: cancellationToken);
							if (parent != null)
							{
								user.Id = parent.Id;
								user.Email = parent.EmailorUserName;
								user.Role = parent.Role.ToString();
								user.FName = parent.FName;
								user.LName = parent.LName;
								user.BirthDate = parent.BirthDate;
								user.Address = parent.Address;
								user.phone = parent.phoneNumber;
								user.pfpURL = parent.ProfilePicture;
								user.Job = parent.Job;
								user.Status = adminview ? parent.status : null;
								found = true;
							}
							break;
						case UserRole.Student:
							Student student = null;
							if (pid != null)
								student = await _repository.GetEntityAsync<Student>(u => u.Id == user.Id && u.PID == pid, q => q.Include(u => u.parent), cancellationToken: cancellationToken);
							else
								student = await _repository.GetEntityAsync<Student>(u => u.Id == user.Id,q=>q.Include(u=>u.parent).Include(u=>u.EnrolledSubjects),cancellationToken: cancellationToken);
							//Parent p = await _repository.GetEntityAsync<Parent>(u => u.Id.Equals(student.PID));
							if(student != null)
							{
								user.Id = student.Id;
								user.Email = student.EmailorUserName;
								user.Role = student.Role.ToString();
								user.FName = student.FName;
								user.Job = UserRole.Student.ToString();
								
								user.phone = student.phoneNumber;
								if (student.parent != null)
								{
									user.LName = student.parent.FName;
									user.Address = student.parent.Address;
								}
								else
								{
									user.Address = student.Address;
									user.LName = student.LName;
								}
								//else
								//	user.Address = student.;
								user.subjectsCount = student.EnrolledSubjects != null ? student.EnrolledSubjects.Count() : 0;
								user.BirthDate = student.BirthDate;
								user.Disability = student.Disability.ToString();
								//user.Job = string.Empty;
								user.pfpURL = student.ProfilePicture;
								user.Status = adminview ? student.status : null;
								if (student.parent != null)
								{
									user.setParent(new Parent
									{
										EmailorUserName = student.parent.EmailorUserName,
										FName = student.parent.FName,
										phoneNumber = student.parent.phoneNumber,
										reletationShip = student.parent.reletationShip,
										ProfilePicture = student.parent.ProfilePicture,
									});
								}
								found = true;
							}
							break;
						case UserRole.Teacher:
							//Teacher teacher = await _repository.GetEntityAsync<Teacher>(t => t.Id == user.Id,include: q=>q.Include(t=>t.AssignedSubjects), cancellationToken: cancellationToken);
							Teacher teacher = await _repository.GetEntityAsync<Teacher>(t => t.Id == user.Id, cancellationToken: cancellationToken);
							if (teacher != null)
							{
								user.Id = teacher.Id;
								user.Email = teacher.EmailorUserName;
								user.Role = teacher.Role.ToString();
								user.FName = teacher.FName;
								user.BirthDate = teacher.BirthDate;
								user.LName = teacher.LName;
								user.Address = teacher.Address;
								user.pfpURL = teacher.ProfilePicture;
								user.phone = teacher.phoneNumber;
								user.Status = adminview ? teacher.status : null;
								user.Job = "Teacher";
								found = true;
							}
							break;
						default:
							return new ResultDTO
							{
								StatusCode = StatusCodes.Status403Forbidden,
								Message = "invalid action" // unknown role
							};
					}
				}
				if (!found)
					return new ResultDTO
					{
						StatusCode = StatusCodes.Status404NotFound,
						Message = pid != null ? "Child's id is invalid" : "User isn't found"
					};
				else
				{
					return new ResultDTO
					{
						Message = "Found",
						StatusCode = StatusCodes.Status200OK,
						result = user
					};
				}
			}
			else
				return new ResultDTO
				{
					StatusCode = StatusCodes.Status500InternalServerError,
					Message = "Something Went Wrong"
				};
		}

		private async Task storeImage(User user, IFormFile file, CancellationToken cancellationToken = default) // implement cloudinary
		{
			if (file != null && file.Length > 0)
			{
				//string userFolder = Path.Combine("uploads", "users");
				//if (!System.IO.File.Exists(userFolder))
				//{
				//	Directory.CreateDirectory(userFolder);
				//}

				//user.ProfilePicture = Path.Combine(userFolder, $"{user.Id}.jpg");

				//using var stream = new FileStream(user.ProfilePicture, FileMode.Create);
				//await file.CopyToAsync(stream, cancellationToken);
				string publicId = $"{user.Id}";
				user.ProfilePicture = await _cloudinaryServices.UploadImageAsync(file, "user", publicId, cancellationToken);
				_repository.UpdateEntityAsync<User>(user, cancellationToken: cancellationToken);
				await _uow.SaveChangesAsync();
			}
		}

	} 
}

