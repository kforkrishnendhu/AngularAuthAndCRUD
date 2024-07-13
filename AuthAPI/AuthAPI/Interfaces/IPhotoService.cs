using System;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
namespace AuthAPI.Interfaces
{
	public interface IPhotoService
	{
		Task<ImageUploadResult> UploadPhotoAsync(IFormFile photo);
	}
}

