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

            // Client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get container list
            CloudBlobContainer container = blobClient.GetContainerReference("yam");

            // Create container if does not exist
            container.CreateIfNotExists();

            // Get blob
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("70-532-questions.pdf");

            // Open file and upload it
            using (var fileStream = System.IO.File.OpenRead(@"D:\Development\70-532-questions.pdf"))
            {
                // Upload filestream
                blockBlob.UploadFromStream(fileStream);
            }

            Console.ReadKey();
        }
    }
}
