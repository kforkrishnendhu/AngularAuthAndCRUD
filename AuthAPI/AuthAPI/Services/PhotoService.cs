using System;
using AuthAPI.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace AuthAPI.Services
{
	public class PhotoService: IPhotoService
	{
        public readonly Cloudinary cloudinary;
        public PhotoService(IConfiguration config)
        {
            Account account = new Account(
                config.GetSection("CloudinarySettings:CloudName").Value,
                config.GetSection("CloudinarySettings:Cloudinary_ApiKey").Value,
                config.GetSection("CloudinarySettings:Cloudinary_ApiSecret").Value
                );
            cloudinary = new Cloudinary(account);
        }


        public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile photo)
        {
            var uploadResult = new ImageUploadResult();
            if (photo.Length > 0)
            {
                using var stream = photo.OpenReadStream(); //since this stream may cause memory leak ,
                                                           // so we use 'using' sothat as soon as its scope over, it automatically get dispossed of
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(photo.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(800)
                };
                uploadResult = await cloudinary.UploadAsync(uploadParams);
            }
            return uploadResult;
        }
    }
}

