using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ProyectoIndividualMvcNet.Helpers
{
    public class ImageHelper
    {
        private readonly IWebHostEnvironment _environment;

        public ImageHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string folderPath = "images/juegos")
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;
            if (!IsValidImageFile(imageFile))
                throw new InvalidOperationException("El archivo debe ser una imagen válida (jpg, jpeg, png, gif)");

            string fileName = GenerateUniqueFileName(imageFile.FileName);
            string uploadsFolder = Path.Combine(_environment.WebRootPath, folderPath);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/{folderPath}/{fileName}";
        }
            public void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            // Convertir ruta relativa a absoluta
            string fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception)
                {
                    
                }
            }
        }
        public async Task<string> UpdateImageAsync(IFormFile newImageFile, string oldImagePath, string folderPath = "images/juegos")
        {
            if (newImageFile == null || newImageFile.Length == 0)
                return oldImagePath;
            if (!string.IsNullOrEmpty(oldImagePath))
                DeleteImage(oldImagePath);
            return await SaveImageAsync(newImageFile, folderPath);
        }
        public async Task<string> ConvertToBase64Async(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            if (!IsValidImageFile(imageFile))
                throw new InvalidOperationException("El archivo debe ser una imagen válida (jpg, jpeg, png, gif, webp)");

            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                string mimeType = GetMimeType(imageFile.FileName);
                return $"data:{mimeType};base64,{base64String}";
            }
        }

        private bool IsValidImageFile(IFormFile file)
        {
            if (file == null)
                return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }
        private string GenerateUniqueFileName(string originalFileName)
        {
            string extension = Path.GetExtension(originalFileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
           
            fileNameWithoutExtension = string.Join("_", fileNameWithoutExtension.Split(Path.GetInvalidFileNameChars()));
            
            string uniqueFileName = $"{fileNameWithoutExtension}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            
            return uniqueFileName;
        }

        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
        }
    }
}
