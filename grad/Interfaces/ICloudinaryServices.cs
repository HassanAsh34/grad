using CloudinaryDotNet.Actions;

namespace grad.Interfaces
{
	public interface ICloudinaryServices
	{
		public Task<string> UploadImageAsync(IFormFile file,string folder,string publicId, CancellationToken cancellationToken = default);

		public Task<IEnumerable<string>> UploadImagesAsync(List<IFormFile> formFiles,string folder, IEnumerable<string> publicIds, CancellationToken cancellationToken = default);

		public Task<string> UploadVideoAsync(IFormFile file,string folder,string publicId, CancellationToken cancellationToken = default);
	}
}
