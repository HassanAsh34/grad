using Google.Cloud.Firestore;
using Microsoft.VisualBasic;

namespace grad.Data
{
	public class FireStoreContext
	{
		public readonly FirestoreDb _Db;
		public FireStoreContext(IConfiguration config)
		{
			var projectId = config["Google:ProjectId"];
			var credentialsRelativePath = config["Google:CredentialsFile"];

			if (!string.IsNullOrEmpty(credentialsRelativePath))
			{
				// Combine with current directory to get full path
				var credentialsFullPath = Path.Combine(Directory.GetCurrentDirectory(), credentialsRelativePath);

				if (!File.Exists(credentialsFullPath))
				{
					throw new FileNotFoundException($"Firestore credentials not found at: {credentialsFullPath}");
				}

				// Set environment variable for Firestore SDK
				Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsFullPath);
			}

			_Db = FirestoreDb.Create(projectId);
		}

	}
}
