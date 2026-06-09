using System.Security.Cryptography;
using Grad.Application.Auth.Interfaces;
using Grad.Application.Common.DTOs;
using Grad.Application.Common.Interfaces;
using Grad.Application.ParentFeatures.DTOs;
using Grad.Application.ParentFeatures.Interfaces;
using Grad.Application.QAFeature.DTO;
using Grad.Application.QAFeature.Interfaces;
using Grad.Application.StudentFeatures.Interfaces;
using Grad.Application.SubjectFeatures.Interfaces;
using Grad.Application.SubmissionFeatures.DTOs;
using Grad.Application.SubmissionFeatures.Interfaces;
using Grad.Application.TeacherFeatures.Interfaces;
using Grad.Application.Users.Interfaces;
using Grad.Domain.Enums;
using Grad.Domain.Model;

namespace Grad.Application.ParentFeatures.Services
{
	public class ParentServices : IParentServices
	{
		private readonly IAuthServices _authServices;
		private readonly IUserServices _userServices;
		private readonly IParentRepository _IParentRepository;
		private readonly IStudentServices _IStudentServices;
		private readonly ISubmissionServices _ISubmissionServices;
		private readonly ICummunicationServices _inqueryServices;
		private readonly IRedisServices _redisServices;
		//private readonly IUowServices  _uowServices;

		public ParentServices(IUserServices userServices,IAuthServices authServices,IParentRepository parentRepository,IStudentServices studentServices,ISubmissionServices submissionServices,IRedisServices redisServices,ICummunicationServices cummunicationServices)
		{
			_userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
			_authServices = authServices ?? throw new ArgumentNullException(nameof(authServices));
			_IParentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
			_IStudentServices = studentServices ?? throw new ArgumentNullException(nameof(studentServices));
			_ISubmissionServices = submissionServices ?? throw new ArgumentNullException(nameof(submissionServices));
			_inqueryServices = cummunicationServices ?? throw new ArgumentNullException(nameof(cummunicationServices));
			_redisServices = redisServices ?? throw new ArgumentNullException(nameof(redisServices));
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
		//	Student student = await _IParentRepository.GetEntityAsync<Student>(u => u.EmailorUserName.Equals(email)
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
		//			_IParentRepository.UpdateEntityAsync<Student>(student, cancellationToken: cancellationToken);
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
			IEnumerable<Student> children = await _IParentRepository.showChildren(pid, cancellationToken: cancellationToken);
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
						Role = UserRole.Student.ToString(),
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

		public async Task<ResultDTO> DeleteStudent(ProfileDTO profile,Guid pid,CancellationToken cancellationToken)
		{
			return await _userServices.DeleteProfile(profile, pid, false, false, cancellationToken);
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

		public async Task<ResultDTO> ViewSubjects(Guid sid, Guid pid, CancellationToken cancellationToken)
		{
			Student s = await _IParentRepository.isStudentExists(sid, pid, cancellationToken);
			if (s != null)
			{
				return await _IStudentServices.ViewSubjects(sid,s.Disability.ToString(),true, cancellationToken);
			}
			else
				return new ResultDTO
				{
					StatusCode = 404,
					Message = "We couldn't find a student with this ID."
				};
		}

		public async Task<ResultDTO> ViewSubjectStats(Guid stdid,Guid sid,Guid pid,CancellationToken cancellationToken)
		{
			if (await _IParentRepository.isStudentExists(stdid, pid, cancellationToken) != null)
			{
				List<SubmissionDTO> submissionDTOs = await _ISubmissionServices.GetSubmissions(stdid, sid, cancellationToken);
				if (submissionDTOs != null && submissionDTOs.Count > 0)
				{
					SubjectStatsDTO subjectStatsDTO = new SubjectStatsDTO
					{
						submissionDTOs = submissionDTOs
					};
					return new ResultDTO
					{
						StatusCode = 200,
						result = subjectStatsDTO

					};
				}
				else
					return new ResultDTO
					{
						StatusCode = 404,
						Message = "No submissions where found."
					};
			}
			else
				return new ResultDTO
				{
					StatusCode = 404,
					Message = "We couldn't find a student with this ID."
				};
		}

		private void GenerateUserName(RegisterStudentDTO student)// need further improvement
		{
			// Implement username generation logic here
			string shortGuid = Guid.NewGuid().ToString("N")[..8];
			student.Email = $"{student.FName.ToLower()[3]}.{shortGuid}@System.com";
		}

		public async Task<ResultDTO> listInqueries(Guid UId, CancellationToken CT)
		{
			IEnumerable<InqueryDTO> inqueries = await _inqueryServices.GetInqueries(UId,ct: CT);
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
				return new ResultDTO
				{
					StatusCode = 200,
					Message = "Inquery retrieved successfully",
					result = inquery
				};
			}
		}

		public async Task<ResultDTO> createInquery(InqueryDTO inquery, CancellationToken cancellationToken)
		{
			if (await _inqueryServices.createInquery(inquery,null, cancellationToken))
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
