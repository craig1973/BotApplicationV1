using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.IO;

namespace BotApplication.Models
{
	public class Storage
	{
		public static string SaveFile(Stream stream, string containerName, string uniqueName, string contentType)
		{
			var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["Storage"].ConnectionString);
			var blobStorage = storageAccount.CreateCloudBlobClient();

			var container = blobStorage.GetContainerReference(containerName);

			if (container.CreateIfNotExists())
			{
				var permissions = container.GetPermissions();
				permissions.PublicAccess = BlobContainerPublicAccessType.Container;
				container.SetPermissions(permissions);
			}

			var blob = container.GetBlockBlobReference(uniqueName);
			blob.Properties.ContentType = contentType;

			blob.UploadFromStream(stream);

			return blob.Uri.ToString().Replace("http:", "https:");
		}
	}
}