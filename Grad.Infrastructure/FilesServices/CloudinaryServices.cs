using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Grad.Application.Common.Interfaces;
using Grad.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
				return result.SecureUrl?.ToString();
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
				return result.SecureUrl?.ToString();
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
						urls.Add(result.SecureUrl?.ToString());
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

		public async Task<bool> DeleteAsync(string directory, bool video = false, bool folder = false, CancellationToken cancellationToken = default)
		{
			if (folder)
			{
				bool success = await DeleteFolderRecursiveAsync(directory, cancellationToken);
				return success;
			}

			var deletionResult = await _cloudinary.DestroyAsync(
				new DeletionParams(directory)
				{
					ResourceType = video ? ResourceType.Video : ResourceType.Image
				});

			return deletionResult.StatusCode == System.Net.HttpStatusCode.OK ||
				   deletionResult.StatusCode == System.Net.HttpStatusCode.NotFound;
		}

		private async Task<bool> DeleteFolderRecursiveAsync(string directory, CancellationToken cancellationToken)
		{
			// Step 1: Delete all resources under this prefix (includes all subfolders recursively)
			await DeleteAllResourcesInFolderAsync(directory);

			// Step 2: Delete all subfolders via the REST API manually, since SDK method is unavailable
			await DeleteSubFoldersViaApiAsync(directory);

			// Step 3: Delete the now-empty root folder
			var folderResult = await _cloudinary.DeleteFolderAsync(directory, cancellationToken);

			return folderResult.StatusCode == System.Net.HttpStatusCode.OK ||
				   folderResult.StatusCode == System.Net.HttpStatusCode.NotFound;
		}

		private async Task DeleteAllResourcesInFolderAsync(string directory)
		{
			string folderPrefix = directory.EndsWith('/') ? directory : directory + "/";
			var resourceTypes = new[] { ResourceType.Image, ResourceType.Video, ResourceType.Raw };

			foreach (var resourceType in resourceTypes)
			{
				string? nextCursor = null;

				do
				{
					var listResult = await _cloudinary.ListResourcesAsync(
						new ListResourcesByPrefixParams
						{
							Prefix = folderPrefix,
							MaxResults = 500,
							NextCursor = nextCursor,
							ResourceType = resourceType,
							Type = "upload"
						});

					if (listResult.Resources == null || !listResult.Resources.Any())
						break;

					var publicIds = listResult.Resources
						.Select(x => x.PublicId)
						.ToList();

					await _cloudinary.DeleteResourcesAsync(new DelResParams
					{
						PublicIds = publicIds,
						ResourceType = resourceType
					});

					nextCursor = listResult.NextCursor;

				} while (!string.IsNullOrEmpty(nextCursor));
			}
		}

		private async Task DeleteSubFoldersViaApiAsync(string directory)
		{
			// The .NET SDK doesn't expose SubFoldersAsync, so we call the REST API directly
			var account = _cloudinary.Api.Account;
			var cloudName = account.Cloud;
			var apiKey = account.ApiKey;
			var apiSecret = account.ApiSecret;

			var url = $"https://api.cloudinary.com/v1_1/{cloudName}/folders/{Uri.EscapeDataString(directory)}";

			using var httpClient = new HttpClient();
			var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"));
			httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

			var response = await httpClient.GetAsync(url);
			if (!response.IsSuccessStatusCode) return;

			var json = await response.Content.ReadAsStringAsync();
			var doc = System.Text.Json.JsonDocument.Parse(json);
			var folders = doc.RootElement.GetProperty("folders");

			foreach (var folder in folders.EnumerateArray())
			{
				var path = folder.GetProperty("path").GetString();
				if (!string.IsNullOrEmpty(path))
					await DeleteSubFoldersViaApiAsync(path); // recurse into nested subfolders
			}
		}

		public async Task<string> UploadCV(IFormFile file, string folder, string publicId, CancellationToken cancellationToken)
		{
			if (file.Length > 0)
			{
				if(!file.FileName.EndsWith(".pdf"))
					return string.Empty;
				using var stream = file.OpenReadStream();
				var uploadParams = new RawUploadParams()
				{
					File = new FileDescription(file.FileName, stream),
					Folder = $"uploads/{folder}",
					PublicId = publicId,
					Overwrite = true
				};
				var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken: cancellationToken);
				return result.SecureUrl?.ToString();
			}
			return string.Empty;
		}
	}
}
