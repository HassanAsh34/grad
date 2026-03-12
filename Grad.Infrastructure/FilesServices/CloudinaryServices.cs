using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Grad.Application.Common.Interfaces;
using Grad.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Grad.Infrastructure.FilesServices
{
	public class CloudinaryServices : ICloudinaryServices
	{
		private readonly Cloudinary _cloudinary;

		public CloudinaryServices(IOptions<CloudinarySettings> config)
		{
			var account = new Account(
				config.Value.CloudName,
				config.Value.ApiKey,
				config.Value.ApiSecret
			);

			_cloudinary = new Cloudinary(account);
		}

		public async Task<string> UploadImageAsync(IFormFile file, string folder, string publicId, CancellationToken cancellationToken)
		{
			if (file.Length > 0)
			{
				using var stream = file.OpenReadStream();
				var uploadParams = new ImageUploadParams()
				{
					File = new FileDescription(file.FileName, stream),
					Folder = $"uploads/{folder}",
					PublicId = publicId,
					Overwrite = true
				};

				var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken: cancellationToken);
				return result.SecureUrl.ToString();
			}

			return string.Empty;
		}

		public async Task<string> UploadVideoAsync(IFormFile file, string folder, string publicId, CancellationToken cancellationToken)
		{
			if (file.Length > 0)
			{
				using var stream = file.OpenReadStream();
				var uploadParams = new VideoUploadParams()
				{
					File = new FileDescription(file.FileName, stream),
					Folder = $"uploads/{folder}",
					PublicId = publicId,
					Overwrite = true
				};

				var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken: cancellationToken);
				return result.SecureUrl.ToString();
			}

			return string.Empty;
		}

		public async Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> formFiles, string folder, IEnumerable<string> publicIds, CancellationToken cancellationToken)
		{
			List<string> urls = new List<string>();
			if (formFiles == null || formFiles.Count() == 0)
				return urls;
			else
			{
				int index = 0;
				foreach (var file in formFiles)
				{
					if (file.Length > 0)
					{
						using var stream = file.OpenReadStream();
						var uploadParams = new ImageUploadParams()
						{
							File = new FileDescription(file.FileName, stream),
							Folder = $"uploads/{folder}", // ✅ Cloudinary folder
							PublicId = publicIds.ElementAt(index),
							Overwrite = true
						};

						var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken: cancellationToken);
						urls.Add(result.SecureUrl.ToString());
					}
					else
					{
						urls.Add(string.Empty);
					}
					index++;
				}
				return urls;
			}
		}

		public async Task<bool> DeleteAsync(string directory, bool video = false, bool folder = false)
		{
			//DeletionResult res = null;
			//DeletionResult res = null;
			string res = string.Empty;
			if (folder)
			{
				string folderPrefix = directory.EndsWith('/') ? directory : directory + "/";
				await _cloudinary.DeleteResourcesAsync(new DelResParams
				{
					Prefix = folderPrefix,
					ResourceType = video ? ResourceType.Video : ResourceType.Image
				});
				var folderResult = await _cloudinary.DeleteFolderAsync(directory);
				res = "ok";
			}
			else
			{
				var deletionResult = await _cloudinary.DestroyAsync(new DeletionParams(directory) { ResourceType = video ? ResourceType.Video : ResourceType.Image });
				res = deletionResult.Result;
			}
			if (res == "ok")
				return true;
			else
				return false;
		}
	}
}
