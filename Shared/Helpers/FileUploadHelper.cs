using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;

namespace Shared.Helpers
{
    public class FileUploadHelper
    {

        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;
        public FileUploadHelper(IConfiguration configuration)
        {
            //_bucketName = configuration["AWS:BucketName"];
            //var region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);

            //_s3Client = new AmazonS3Client(
            //    configuration["AWS:AccessKey"],
            //    configuration["AWS:SecretKey"],
            //    region
            //);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string companyName, string documentType)
        {
            try
            {
                if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(documentType))
                    return null;

                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                fileName = fileName+"-" + Guid.NewGuid() + fileExtension;
                //company name with dash if space found
                //var key = $"{companyName.Replace(" ","-").Trim()}/{documentType.Trim()}/{fileName.Trim()}";

                var key = $"{companyName.Trim()}/{documentType.Trim()}/{fileName.Trim()}";

                using (var stream = file.OpenReadStream())
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = key,
                        InputStream = stream,
                        ContentType = file.ContentType,
                        AutoCloseStream = true,
                    };

                    await _s3Client.PutObjectAsync(putRequest);
                }

                //return $"https://{_bucketName}.s3.amazonaws.com/{key}";
                //for manage space found in any key
                return $"https://{_bucketName}.s3.amazonaws.com/{Uri.EscapeDataString(key)}";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteFileAsync(string companyName, string documentType, string fileName)
        {
            if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(documentType) || string.IsNullOrWhiteSpace(fileName))
                return false;

            var key = $"{companyName.Trim()}/{documentType.Trim()}/{fileName.Trim()}";

            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.DeleteObjectAsync(deleteRequest);

                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> UploadFileToLocalAsync(IFormFile file, string folderName = "Uploads")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadPath = Path.Combine(wwwRootPath, folderName);

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                fileName = $"{fileName}-{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/{folderName}/{fileName}";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



    }
}
