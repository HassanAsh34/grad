////using Google.Apis.Auth.OAuth2;
////using Google.Cloud.Firestore;

////namespace grad.Data
////{
////	public sealed class FireStoreContext //firestore issues need to be fixed
////	{
////		public FirestoreDb Db { get; }

////		public FireStoreContext(IConfiguration configuration)
////		{
////			// 1️⃣ Read credentials path from config
////			var credentialsRelativePath = configuration["Google:CredentialsFile"];

////			if (string.IsNullOrWhiteSpace(credentialsRelativePath))
////				throw new InvalidOperationException(
////					"Missing configuration value: Google:CredentialsFile");

////			// 2️⃣ Resolve absolute path
////			var credentialsFullPath =
////				Path.Combine(Directory.GetCurrentDirectory(), credentialsRelativePath);

////			if (!File.Exists(credentialsFullPath))
////				throw new FileNotFoundException(
////					"Firestore service account file not found.",
////					credentialsFullPath);

////			// 3️⃣ Load service account credentials explicitly
////			GoogleCredential credential =
////				GoogleCredential.FromFile(credentialsFullPath);

////			if (credential.UnderlyingCredential is not ServiceAccountCredential serviceAccount)
////				throw new InvalidOperationException(
////					"The provided JSON is not a valid Firestore service account key.");

////			// 4️⃣ Build Firestore client with EXPLICIT auth + project
////			Db = new FirestoreDbBuilder
////			{
////				ProjectId = serviceAccount.ProjectId,
////				Credential = credential
////			}.Build();
////		}
////	}
////}



//using Google.Cloud.Firestore;
//using Microsoft.VisualBasic;

//namespace grad.Data
//{
//	public class FireStoreContext
//	{
//		public readonly FirestoreDb _Db;
//		public FireStoreContext(IConfiguration config)
//		{
//			var projectId = config["Google:ProjectId"];
//			var credentialsRelativePath = config["Google:CredentialsFile"];

//			if (!string.IsNullOrEmpty(credentialsRelativePath))
//			{
//				// Combine with current directory to get full path
//				var credentialsFullPath = Path.Combine(Directory.GetCurrentDirectory(), credentialsRelativePath);

//				if (!File.Exists(credentialsFullPath))
//				{
//					throw new FileNotFoundException($"Firestore credentials not found at: {credentialsFullPath}");
//				}

//				// Set environment variable for Firestore SDK
//				Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsFullPath);
//			}

//			_Db = FirestoreDb.Create(projectId);
//		}

//	}
//}
