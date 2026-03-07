namespace ProyectoIndividualMvcNet.Helpers
{
    public class ImageHelper
    {
        private readonly IWebHostEnvironment _environment;

        public ImageHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public async Task<ImageResult> SaveImageAndGetBase64Async(IFormFile imageFile, string folderPath = "images/juegos")
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            if (!IsValidImageFile(imageFile))
                throw new InvalidOperationException("El archivo debe ser una imagen válida (jpg, jpeg, png, gif, webp)");

            // 1. Generar nombre único para el archivo
            string fileName = GenerateUniqueFileName(imageFile.FileName);
            string uploadsFolder = Path.Combine(_environment.WebRootPath, folderPath);

            // 2. Crear directorio si no existe
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string filePath = Path.Combine(uploadsFolder, fileName);

            // 3. Convertir a Base64 Y guardar en disco simultáneamente
            string base64String;
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                // Convertir a Base64
                base64String = Convert.ToBase64String(imageBytes);

                // Guardar archivo en disco
                await File.WriteAllBytesAsync(filePath, imageBytes);
            }

            string mimeType = GetMimeType(imageFile.FileName);
            string base64WithPrefix = $"data:{mimeType};base64,{base64String}";
            string relativePath = $"/{folderPath}/{fileName}";

            return new ImageResult
            {
                Base64 = base64WithPrefix,
                FilePath = relativePath
            };
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

        public void DeleteImageFile(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            // Si es Base64, no hay archivo que eliminar
            if (imagePath.StartsWith("data:"))
                return;

            string fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    
                }
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

        private string GenerateUniqueFileName(string originalFileName)
        {
            string extension = Path.GetExtension(originalFileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            fileNameWithoutExtension = string.Join("_", fileNameWithoutExtension.Split(Path.GetInvalidFileNameChars()));
            string uniqueFileName = $"{fileNameWithoutExtension}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            return uniqueFileName;
        }
    }


    public class ImageResult
    {
        public string Base64 { get; set; }
        public string FilePath { get; set; }
    }
}