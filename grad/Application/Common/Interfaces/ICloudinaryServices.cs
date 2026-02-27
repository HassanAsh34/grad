using CloudinaryDotNet.Actions;

namespace grad.Application.Common.Interfaces
{
	public interface ICloudinaryServices
	{
		public Task<string> UploadImageAsync(IFormFile file,string folder,string publicId, CancellationToken cancellationToken = default);

		public Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> formFiles,string folder, IEnumerable<string> publicIds, CancellationToken cancellationToken = default);

		public Task<string> UploadVideoAsync(IFormFile file,string folder,string publicId, CancellationToken cancellationToken = default);

		//public Task<bool> DeleteAsync(string directory, bool video = false);

		public Task<bool> DeleteAsync(string directory, bool video = false, bool folder = false);
	}
}
