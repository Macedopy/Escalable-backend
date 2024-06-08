using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace DeliveryDrivers.Infrastructure
{
    public interface IDriverimageS3
    {
        Task<byte[]> DownloadFile(string file);
        Task SendFileToS3(string bucket, string key, IFormFile file);
        Task DeleteFileS3(string filename, string versionId = "");
    };
    public sealed class DriverimageS3 : IDriverimageS3
    {
        public string AwsKeyID { get; private set; }
        public string AwsSecretKey { get; private set; }
        public BasicAWSCredentials AwsCredentials { get; private set; }
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;

        public DriverimageS3()
        {
            AwsKeyID = "AKIAW3MEFVAPWS57A6MQ";
            AwsSecretKey = "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";
            AwsCredentials = new BasicAWSCredentials(AwsKeyID, AwsSecretKey);
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast2
            };
            _s3Client = new AmazonS3Client(AwsCredentials, config);
        }

        public async Task DeleteFileS3(string filename, string versionId = "")
        {
            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = "mottu-driver-image",
                Key = filename
            };

            if (!string.IsNullOrEmpty(versionId))
            {
                request.VersionId = versionId;
            }

            await _s3Client.DeleteObjectAsync(request);
        }

        public async Task<byte[]> DownloadFile(string file)
        {

            try
            {
                var TransferUtility = new TransferUtility(_s3Client);

                var request = await TransferUtility.S3Client.GetObjectAsync(new GetObjectRequest()
                {
                    BucketName = "mottu-driver-image",
                    Key = file
                });

                using (var memoryStream = new MemoryStream())
                {
                    await request.ResponseStream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
                // using (GetObjectResponse response = await _s3Client.GetObjectAsync(request))
                // {
                //     using (StreamReader reader = new StreamReader(response.ResponseStream))
                //     {
                //         string contents = reader.ReadToEnd();
                //         Console.WriteLine("Object - " + response.Key);
                //         Console.WriteLine(" Version Id - " + response.VersionId);
                //         Console.WriteLine(" Contents - " + contents);
                //     }
                // }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SendFileToS3(string bucket, string key, IFormFile file)
        {
            using var newMemoryStream = new MemoryStream();
            file.CopyTo(newMemoryStream);

            var fileTransferUtility = new TransferUtility(_s3Client);

            await fileTransferUtility.UploadAsync(new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = key,
                BucketName = bucket,
                ContentType = file.ContentType
            });
        }
    }
}