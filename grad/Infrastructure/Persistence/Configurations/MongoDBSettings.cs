namespace Grad_Structured.Infrastructure.Persistence.Configurations
{
	public class MongoDBSettings
	{
		public string Local_Host { get; set; }
		public string Hosted { get; set; }

		public bool Hosted_Enabled {get; set;}
        
		public 	string DB_Name { get; set; }

		public string ConnectionString =>
		Hosted_Enabled ? Hosted : Local_Host;


		//public string ConnectionURI { get; set; }

		//public string DBName { get; set; }

		//public string CollectionName { get; set; }
	}
}


