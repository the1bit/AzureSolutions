using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;


namespace _01Blobs
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get connection string from Config file
            string storageconnection = 
                System.Configuration.ConfigurationManager.AppSettings.Get("StorageConnectionString");

            // Parse connection string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageconnection);

            // Client for blob
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get container list
            CloudBlobContainer container = blobClient.GetContainerReference("yam");

            // Create container if does not exist
            container.CreateIfNotExists();


            // List container attributes
            //ListAttributes(container);

            // Set new attributes - metadata
            //SetMetadata(container);

            //List Metadata of container
            //ListMetadata(container);

            //Copy file
            //CopyBlob(container, "lights.jpg");

            Console.ReadKey();
        }

        static void UploadBlob(CloudBlobContainer container, string fileName, string fileWithPath)
        {
            // Get blob
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Open file and upload it
            using (var fileStream = System.IO.File.OpenRead(fileWithPath))
            {
                // Upload filestream
                blockBlob.UploadFromStream(fileStream);
            }
        }

        static void ListAttributes(CloudBlobContainer container)
        {
            // Get container existing attrubutes
            container.FetchAttributes();

            // Write attributes out to screen
            Console.WriteLine("Container Name: " + container.Name);
            Console.WriteLine("Container Primary URL: " + container.StorageUri.PrimaryUri.ToString());
            Console.WriteLine("Last modified: " + container.Properties.LastModified.ToString());
        }

        static void SetMetadata(CloudBlobContainer container)
        {
            // Clear all metadata
            container.Metadata.Clear();
            // Add metadata on different ways
            container.Metadata.Add("author", "the1bit");
            container.Metadata["authoredOn"] = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();



            // Apply changes
            container.SetMetadata();
        }

        static void ListMetadata(CloudBlobContainer container)
        {
            // Get existing metadata
            container.FetchAttributes();

            Console.WriteLine("-=== METADATA ===-\n");

            // Fetch on metadata list
            foreach (var item in container.Metadata)
            {
                Console.WriteLine("Key: " + item.Key);
                Console.WriteLine("Value: " + item.Value + "\n\n");
            }
        }

        static void CopyBlob(CloudBlobContainer container, string fileName)
        {
            // Get source
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Set target
            CloudBlockBlob copyTobBockBlob = container.GetBlockBlobReference("copy_" + fileName);

            // Start copy
            copyTobBockBlob.StartCopyAsync(new Uri(blockBlob.Uri.AbsoluteUri));
        }

        static void CreateSAS(CloudBlobContainer container)
        {
            // Create a new access policy
            SharedAccessBlobPolicy sharedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List
            };

            // Get container's existing permissions
            BlobContainerPermissions permissions = new BlobContainerPermissions();

            // Add the new policy to the container's permissions
            permissions.SharedAccessPolicies.Clear();
            permissions.SharedAccessPolicies.Add("PolicyName", sharedPolicy);
            container.SetPermissions(permissions);
        }
    }

}
