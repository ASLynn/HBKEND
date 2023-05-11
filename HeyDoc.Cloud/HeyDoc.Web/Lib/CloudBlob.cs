using HeyDoc.Web.Helpers;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace HeyDoc.Web.Lib
{
    public class CloudBlob
    {
        #region Singleton
        public string BaseUri { get; set; }
        private static volatile CloudBlob instance;
        private static object syncRoot = new Object();

        public static CloudBlob Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new CloudBlob();
                        }
                    }
                }

                return instance;
            }
        }


        private CloudStorageAccount storageAccount;
        private CloudBlob()
        {
            storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString);

            BaseUri = BlobClient.BaseUri.ToString();
        }

        #endregion Singleton

        private CloudBlobClient BlobClient
        {
            get
            {
                // always create new client to handle new request so that if any crashes happen to this instance it will affect other request
                return storageAccount.CreateCloudBlobClient();
            }
        }

        #region Blob
        public string CreateBlob(string containerName, string blobName, Stream dataStream, string contentType, bool isPublic = true)
        {
            var container = BlobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            // configure container for public access
            var permissions = container.GetPermissions();
            permissions.PublicAccess = isPublic ? BlobContainerPublicAccessType.Container : BlobContainerPublicAccessType.Off;
            container.SetPermissions(permissions);

            var blob = container.GetBlockBlobReference(blobName);
            if (!string.IsNullOrEmpty(contentType))
            {
                blob.Properties.ContentType = contentType;
            }
            else
            {
                blob.Properties.ContentType = MimeMapping.GetMimeMapping(blobName);
            }
            if (dataStream.CanSeek && dataStream.Position != 0)
            {
                dataStream.Position = 0;
            }
            blob.UploadFromStream(dataStream);
            string blobUri = blob.Uri.ToString();

            return blobUri;
        }

        public string CreateBlobForUploadUrl(string url, string containerName, string fileName, bool overwriteBlob)
        {
            var container = BlobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();
            var permissions = container.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            container.SetPermissions(permissions);


            var newBlockBlob = container.GetBlockBlobReference(fileName);
            if (overwriteBlob)
            {
                newBlockBlob.StartCopy(new Uri(url));
            }
            else
            {
                newBlockBlob.StartCopy(new Uri(url), destAccessCondition: AccessCondition.GenerateIfNotExistsCondition());
            }

            string blobUri = newBlockBlob.Uri.ToString();

            return blobUri;
        }

        public static string UploadFileFromUrl(string url, string containerName, string fileName, bool overwriteBlob = true)
        {
            return CloudBlob.Instance.CreateBlobForUploadUrl(url, containerName, fileName, overwriteBlob);
        }

        public void DeleteBlobIfExist(string blobUri)
        {
            if (blobUri.Contains(BaseUri))
            {
                var uri = new Uri(blobUri);
                string containerName = uri.Segments[1].Replace("/", "");// remove last '/'
                string blobName = uri.AbsolutePath.Replace(uri.Segments[0] + uri.Segments[1], "");

                var blobCointainer = BlobClient.GetContainerReference(containerName);
                var blob = blobCointainer.GetBlockBlobReference(blobName);
                //blob.Delete();
                blob.DeleteIfExists();
            }
        }

        public byte[] DownloadFile(string blobAddress)
        {
            return DownloadFileStream(blobAddress).ToArray();
        }

        public MemoryStream DownloadFileStream(string blobAddress)
        {
            using (var memoryStream = new MemoryStream())
            {
                var blob = BlobClient.GetBlobReferenceFromServer(new Uri(blobAddress));
                blob.DownloadToStream(memoryStream);
                return memoryStream;
            }
        }

        public long GetContainerSize(string containerName)
        {
            var container = BlobClient.GetContainerReference(containerName);

            long size = 0;
            if (container.Exists())
            {
                var list = container.ListBlobs(null, true);
                foreach (CloudBlockBlob blob in list)
                {
                    size += blob.Properties.Length;
                }
                return size;
            }
            return size;
        }
        #endregion Blob

        public static string UploadImage(string containerName, string fileName, System.Drawing.Imaging.ImageFormat imageFormat, Stream photoStream)
        {
            Image originalImage = null;
            try
            {
                originalImage = Image.FromStream(photoStream);
                return UploadImage(containerName, fileName, imageFormat, originalImage);
            }
            finally
            {
                if (originalImage != null)
                {
                    originalImage.Dispose();
                    originalImage = null;
                }
            }
        }

        public static string UploadImage(string containerName, string fileName, System.Drawing.Imaging.ImageFormat imageFormat, Image photo)
        {
            string directoryName = Path.GetDirectoryName(fileName).Replace("\\", "/");
            if (!string.IsNullOrEmpty(directoryName))
            {
                directoryName = directoryName + "/";
            }
            fileName = directoryName + Path.GetFileNameWithoutExtension(fileName);
            string fileNameWithExtension = fileName + "." + imageFormat.ToString();

            containerName = HttpUtility.UrlEncode(containerName.ToLower().Replace(" ", ""));
            fileName = HttpUtility.UrlEncode(fileName);

            using (MemoryStream imageStream = new MemoryStream())
            {
                photo.Save(imageStream, imageFormat);
                imageStream.Seek(0, SeekOrigin.Begin);

                return CloudBlob.Instance.CreateBlob(containerName, fileNameWithExtension, imageStream, "Image/jpeg");
            }
        }

        public static string UploadFile(string containerName, string fileNameWithExtension, Stream file, bool isPublic = true)
        {
            return CloudBlob.Instance.CreateBlob(containerName, fileNameWithExtension, file, null, isPublic);
        }

        public static void DeleteImage(string blobUrl)
        {
            CloudBlob.Instance.DeleteBlobIfExist(blobUrl);
        }

        public static long ContainerSize(string containerName)
        {
            return CloudBlob.Instance.GetContainerSize(containerName);
        }

        public static string GetThumbnailUrl(string blobImageUrl)
        {
            if (string.IsNullOrEmpty(blobImageUrl) || !blobImageUrl.Contains("blob.core.windows.net"))
            {
                return blobImageUrl;
            }
            try
            {
                string imageFileName = Path.GetFileName(blobImageUrl);
                string imageFileExtension = Path.GetExtension(imageFileName);
                string imageFileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFileName);

                string thumbnailFileNameWithExtension = imageFileNameWithoutExtension + "_tb" + imageFileExtension;

                return blobImageUrl.Replace(imageFileName, thumbnailFileNameWithExtension);
            }
            catch
            {
                // not in out blob image format
                return blobImageUrl;
            }
        }

        public CloudBlobContainer GetContainer(string containerName)
        {
            return BlobClient.GetContainerReference(containerName);
        }
    }
}