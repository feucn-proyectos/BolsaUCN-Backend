using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Domain.Models.Options;
using backend.src.Infrastructure.Repositories.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Serilog;
using SkiaSharp;

namespace backend.src.Application.Services.Implements
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly Cloudinary _cloudinary;
        private readonly string[] _allowedExtensions;
        private readonly int _maxFileSizeInBytes;
        private readonly int _maxBuySellImages;
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly string _cloudName;
        private readonly string _cloudApiKey;
        private readonly string _cloudApiSecret;
        private readonly int _transformationWidth;
        private readonly string _transformationCrop;
        private readonly string _transformationQuality;
        private readonly string _transformationFetchFormat;

        public FileService(
            IConfiguration configuration,
            IFileRepository fileRepository,
            IUserRepository userRepository
        )
        {
            _configuration = configuration;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _cloudName =
                _configuration["Cloudinary:CloudName"]
                ?? throw new InvalidOperationException(
                    "La configuración de CloudName es obligatoria"
                );
            _cloudApiKey =
                _configuration["Cloudinary:ApiKey"]
                ?? throw new InvalidOperationException("La configuración de ApiKey es obligatoria");
            _cloudApiSecret =
                _configuration["Cloudinary:ApiSecret"]
                ?? throw new InvalidOperationException(
                    "La configuración de ApiSecret es obligatoria"
                );
            Account account = new Account(_cloudName, _cloudApiKey, _cloudApiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // Aseguramos  que las URLs sean seguras con HTTPS
            _allowedExtensions =
                _configuration.GetSection("Images:ImageAllowedExtensions").Get<string[]>()
                ?? throw new InvalidOperationException(
                    "La configuración de las extensiones de las imágenes es obligatoria"
                );
            _transformationQuality =
                _configuration["Images:TransformationQuality"]
                ?? throw new InvalidOperationException(
                    "La configuración de la calidad de la transformación es obligatoria"
                );
            _transformationCrop =
                _configuration["Images:TransformationCrop"]
                ?? throw new InvalidOperationException(
                    "La configuración del recorte de la transformación es obligatoria"
                );
            _transformationFetchFormat =
                _configuration["Images:TransformationFetchFormat"]
                ?? throw new InvalidOperationException(
                    "La configuración del formato de la transformación es obligatoria"
                );
            if (!int.TryParse(_configuration["Images:MaxBuySellImages"], out _maxBuySellImages))
            {
                throw new InvalidOperationException(
                    "La configuración del número máximo de imágenes por publicación de compra/venta es obligatoria"
                );
            }
            if (
                !int.TryParse(_configuration["Images:ImageMaxSizeInBytes"], out _maxFileSizeInBytes)
            )
            {
                throw new InvalidOperationException(
                    "La configuración del tamaño de la imagen es obligatoria"
                );
            }
            if (
                !int.TryParse(
                    _configuration["Images:TransformationWidth"],
                    out _transformationWidth
                )
            )
            {
                throw new InvalidOperationException(
                    "La configuración del ancho de la transformación es obligatoria"
                );
            }
        }

        /// <summary>
        /// Sube un archivo a Cloudinary.
        /// </summary>
        /// <param name="file">El archivo a subir.</param>
        /// <param name="buySellId">El ID de la publicación de compra/venta al que pertenece la imagen.</param>
        /// <returns>True si la carga fue exitosa, de lo contrario False.</returns>
        public async Task<bool> UploadAsync(IFormFile file, int buySellId)
        {
            if (buySellId <= 0)
            {
                Log.Error($"BuySellId inválido: {buySellId}");
                throw new ArgumentException("BuySellId debe ser mayor a 0");
            }

            if (file == null || file.Length == 0)
            {
                Log.Error("Intento de subir un archivo nulo o vacío");
                throw new ArgumentException("Archivo inválido");
            }

            if (file.Length > _maxFileSizeInBytes)
            {
                Log.Error(
                    $"El archivo {file.FileName} excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB"
                );
                throw new ArgumentException(
                    $"El archivo excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB"
                );
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(fileExtension))
            {
                Log.Error($"Extensión de archivo no permitida: {fileExtension}");
                throw new ArgumentException(
                    $"Extensión de archivo no permitida. Permitir: {string.Join(", ", _allowedExtensions)}"
                );
            }

            if (!IsValidImageFile(file))
            {
                Log.Error($"El archivo {file.FileName} no es una imagen válida");
                throw new ArgumentException("El archivo no es una imagen válida");
            }
            var folder = $"product/{buySellId}/images";
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                Folder = folder,
                File = new FileDescription(file.FileName, stream),
                UseFilename = true,
                UniqueFilename = true,
            };

            Log.Information($"Optimizando imagen: {file.FileName} antes de subir a la nube");
            uploadParams.Transformation = new Transformation()
                .Width(_transformationWidth)
                .Crop(_transformationCrop)
                .Chain()
                .Quality(_transformationQuality)
                .Chain()
                .FetchFormat(_transformationFetchFormat);

            Log.Information($"Subiendo imagen: {file.FileName} a Cloudinary");
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                Log.Error($"Hubo un error al subir la imagen: {uploadResult.Error.Message}");
                throw new Exception($"Error al subir la imagen: {uploadResult.Error.Message}");
            }

            var image = new Image()
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.ToString(),
                BuySellId = buySellId,
            };

            var result = await _fileRepository.CreateAsync(image);
            if (result is bool && !result.Value!)
            {
                Log.Error($"Error al guardar la imagen en la base de datos: {file.FileName}");
                var deleteResult = await DeleteInCloudinaryAsync(uploadResult.PublicId); // Eliminamos la imagen de Cloudinary si falla la creación de la imagen en la bdd
                if (!deleteResult)
                {
                    Log.Error(
                        $"Error al eliminar la imagen de Cloudinary después de fallar la creación en la base de datos: {uploadResult.PublicId}"
                    );
                    throw new Exception(
                        "Error al eliminar la imagen de Cloudinary después de fallar la creación en la base de datos"
                    );
                }
                throw new Exception("Error al guardar la imagen en la base de datos");
            }
            else if (result is null)
            {
                Log.Warning($"La imagen ya existe en la base de datos: {file.FileName}");
                return false;
            }

            Log.Information($"Imagen subida exitosamente: {uploadResult.SecureUrl}");
            return true;
        }

        public async Task<bool> UploadBatchAsync(List<IFormFile> images, BuySell buySell)
        {
            if (buySell.Id <= 0)
            {
                Log.Error($"BuySellId inválido: {buySell.Id}");
                throw new ArgumentException("BuySellId debe ser mayor a 0");
            }
            if (images == null || images.Count == 0)
            {
                Log.Error("Intento de subir una lista de imágenes nula o vacía");
                throw new ArgumentException("Lista de imágenes inválida");
            }
            if (images.Count > 3)
            {
                Log.Error($"Número de imágenes excede el máximo permitido: {images.Count}");
                throw new ArgumentException("No se pueden subir más de 3 imágenes");
            }
            foreach (var imageFile in images)
            {
                ValidateImageFile(imageFile);
            }

            var uploadedImages = new List<Image>();
            var uploadedPublicIds = new List<string>();
            try
            {
                var folder = $"product/{buySell.Id}/images";
                var uploadTasks = images.Select(async imageFile =>
                {
                    using var stream = imageFile.OpenReadStream();

                    var uploadParams = new ImageUploadParams()
                    {
                        Folder = folder,
                        File = new FileDescription(imageFile.FileName, stream),
                        UseFilename = true,
                        UniqueFilename = true,
                    };

                    Log.Information(
                        $"Optimizando imagen: {imageFile.FileName} antes de subir a la nube"
                    );
                    uploadParams.Transformation = new Transformation()
                        .Width(_transformationWidth)
                        .Crop(_transformationCrop)
                        .Chain()
                        .Quality(_transformationQuality)
                        .Chain()
                        .FetchFormat(_transformationFetchFormat);
                    Log.Information($"Subiendo imagen: {imageFile.FileName} a Cloudinary");
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    if (uploadResult.Error != null)
                    {
                        Log.Error(
                            $"Hubo un error al subir la imagen: {uploadResult.Error.Message}"
                        );
                        throw new Exception(
                            $"Error al subir la imagen: {uploadResult.Error.Message}"
                        );
                    }
                    return new Image
                    {
                        PublicId = uploadResult.PublicId,
                        Url = uploadResult.SecureUrl.ToString(),
                        BuySellId = buySell.Id,
                    };
                });
                var cloudinaryResults = await Task.WhenAll(uploadTasks);
                uploadedImages.AddRange(cloudinaryResults);
                uploadedPublicIds.AddRange(cloudinaryResults.Select(r => r.PublicId));

                var saveResult = await _fileRepository.CreateBatchAsync(uploadedImages);
                if (!saveResult)
                {
                    Log.Error("Error al guardar las imágenes en la base de datos");
                    await RollbackCloudinaryUploads(uploadedPublicIds);
                    throw new Exception("Error al guardar las imágenes en la base de datos");
                }
                Log.Information(
                    $"Todas las imágenes subidas y guardadas exitosamente para BuySellId: {buySell.Id}"
                );
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(
                    $"Error durante la carga de imágenes para BuySellId: {buySell.Id}: {ex.Message}"
                );
                if (uploadedPublicIds.Count > 0)
                {
                    await RollbackCloudinaryUploads(uploadedPublicIds);
                }
                throw;
            }
        }

        public async Task<bool> UploadUserImageAsync(IFormFile file, User user)
        {
            if (user.Id <= 0)
            {
                Log.Error($"Usuario inválido: {user.Id}");
                throw new ArgumentException("UserId debe ser mayor a 0");
            }

            if (file == null || file.Length == 0)
            {
                Log.Error("Intento de subir un archivo nulo o vacío");
                throw new ArgumentException("Archivo inválido");
            }

            if (file.Length > _maxFileSizeInBytes)
            {
                Log.Error(
                    $"El archivo {file.FileName} excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB"
                );
                throw new ArgumentException(
                    $"El archivo excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB"
                );
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(fileExtension))
            {
                Log.Error($"Extensión de archivo no permitida: {fileExtension}");
                throw new ArgumentException(
                    $"Extensión de archivo no permitida. Permitir: {string.Join(", ", _allowedExtensions)}"
                );
            }

            if (!IsValidImageFile(file))
            {
                Log.Error($"El archivo {file.FileName} no es una imagen válida");
                throw new ArgumentException("El archivo no es una imagen válida");
            }
            var folder = $"user/{user.Id}/images";
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                Folder = folder,
                File = new FileDescription(file.FileName, stream),
                UseFilename = true,
                UniqueFilename = true,
            };

            Log.Information($"Optimizando imagen: {file.FileName} antes de subir a la nube");
            uploadParams.Transformation = new Transformation()
                .Width(_transformationWidth)
                .Crop(_transformationCrop)
                .Chain()
                .Quality(_transformationQuality)
                .Chain()
                .FetchFormat(_transformationFetchFormat);

            Log.Information($"Subiendo imagen: {file.FileName} a Cloudinary");
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                Log.Error($"Hubo un error al subir la imagen: {uploadResult.Error.Message}");
                throw new Exception($"Error al subir la imagen: {uploadResult.Error.Message}");
            }

            var image = new UserImage()
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.ToString(),
            };

            var result = await _fileRepository.CreateUserImageAsync(image);
            if (result is bool && !result.Value!)
            {
                Log.Error($"Error al guardar la imagen en la base de datos: {file.FileName}");
                var deleteResult = await DeleteInCloudinaryAsync(uploadResult.PublicId); // Eliminamos la imagen de Cloudinary si falla la creación de la imagen en la bdd
                if (!deleteResult)
                {
                    Log.Error(
                        $"Error al eliminar la imagen de Cloudinary después de fallar la creación en la base de datos: {uploadResult.PublicId}"
                    );
                    throw new Exception(
                        "Error al eliminar la imagen de Cloudinary después de fallar la creación en la base de datos"
                    );
                }
                throw new Exception("Error al guardar la imagen en la base de datos");
            }
            else if (result is null)
            {
                Log.Warning($"La imagen ya existe en la base de datos: {file.FileName}");
                return false;
            }

            Log.Information($"Imagen subida exitosamente: {uploadResult.SecureUrl}");

            // Actualizar la imagen de perfil del usuario
            string publicId;
            if (user.ProfilePhoto != null)
            {
                publicId = user.ProfilePhoto.PublicId;
                await DeleteInCloudinaryAsync(publicId);
                await _fileRepository.DeleteUserImageAsync(publicId);
            }
            user.ProfilePhoto = image;
            user.ProfilePhotoId = image.Id;
            return true;
        }

        public async Task<bool> UploadPDFAsync(IFormFile file, User user)
        {
            // Validar archivo
            if (file == null || file.Length == 0)
            {
                Log.Error("Intento de subir un archivo nulo o vacío");
                throw new ArgumentException("Archivo inválido");
            }
            if (file.Length > _maxFileSizeInBytes)
            {
                Log.Error(
                    "El archivo {FileName} excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB",
                    file.FileName
                );
                throw new ArgumentException(
                    $"El archivo excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB"
                );
            }
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (fileExtension != ".pdf")
            {
                Log.Error("Extensión de archivo no permitida: {FileExtension}", fileExtension);
                throw new ArgumentException("Solo se permiten archivos PDF");
            }
            var folder = $"user/{user.Id}/documents";
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams()
            {
                Folder = folder,
                File = new FileDescription(file.FileName, stream),
                Type = "authenticated",
            };
            Log.Information("Subiendo PDF: {FileName} a Cloudinary", file.FileName);
            uploadParams.Transformation = new Transformation()
                .Width(_transformationWidth)
                .Crop(_transformationCrop)
                .Chain()
                .Quality(_transformationQuality)
                .Page(1)
                .Chain()
                .FetchFormat("pdf");
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                Log.Error(
                    "Hubo un error al subir el PDF: {ErrorMessage}",
                    uploadResult.Error.Message
                );
                throw new Exception($"Error al subir el PDF: {uploadResult.Error.Message}");
            }
            Curriculum cv = new Curriculum()
            {
                PublicId = uploadResult.PublicId,
                FileSizeBytes = file.Length,
            };
            bool result = await _fileRepository.CreateCVAsync(cv);
            if (!result)
            {
                Log.Error("Error al guardar el PDF en la base de datos: {FileName}", file.FileName);
                var deleteResult = await DeleteInCloudinaryAsync(uploadResult.PublicId);
                if (!deleteResult)
                {
                    Log.Error(
                        "Error al eliminar el PDF de Cloudinary después de fallar la creación en la base de datos: {PublicId}",
                        uploadResult.PublicId
                    );
                    throw new Exception(
                        "Error al eliminar el PDF de Cloudinary después de fallar la creación en la base de datos"
                    );
                }
                throw new Exception("Error al guardar el PDF en la base de datos");
            }

            // Asignar el CV al usuario
            if (user.CV != null)
            {
                // Eliminar el CV anterior si existe
                await DeleteInCloudinaryAsync(user.CV.PublicId);
                await _fileRepository.DeleteCVAsync(user.CV.PublicId);
            }
            user.CVId = cv.Id;
            bool saveUserResult = await _userRepository.UpdateAsync(user);
            if (!saveUserResult)
            {
                Log.Error("Error al actualizar el usuario con el nuevo CV: {UserId}", user.Id);
                throw new Exception("Error al actualizar el usuario con el nuevo CV");
            }

            Log.Information("PDF subido exitosamente: {SecureUrl}", uploadResult.SecureUrl);
            return true;
        }

        public async Task<(Stream fileStream, string fileName, string contentType)> DownloadCVAsync(
            int userId
        )
        {
            User? user = await _userRepository.GetByIdAsync(
                userId,
                new UserQueryOptions { IncludeCV = true }
            );
            if (user == null || user.CV == null)
            {
                Log.Warning("Usuario o CV no encontrado para el ID: {UserId}", userId);
                throw new KeyNotFoundException("Usuario o CV no encontrado");
            }

            string signedUrl = _cloudinary
                .Api.UrlImgUp.Signed(true)
                .Action("authenticated")
                .Secure(true)
                .ResourceType("image")
                .BuildUrl($"{user.CV.PublicId}.pdf");
            Log.Information(
                "URL firmada generada para el CV del usuario {UserId}: {SignedUrl}",
                userId,
                signedUrl
            );
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(signedUrl);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error(
                    "Error al descargar el CV desde Cloudinary para el usuario {UserId}: {StatusCode}",
                    userId,
                    response.StatusCode
                );
                throw new Exception("Error al descargar el CV desde Cloudinary");
            }
            var memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reiniciar la posición del stream para su lectura posterior

            string fileName = $"CV_{user.FirstName}_{user.LastName}.pdf"
                .Replace(" ", "_")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");
            Log.Information(
                "CV descargado exitosamente para el usuario {UserId}: {FileName}",
                userId,
                fileName
            );
            return (memoryStream, fileName, "application/pdf");
        }

        public async Task<string> BuildSignedUrlForCVAsync(string publicId)
        {
            string signedUrl = _cloudinary
                .Api.UrlImgUp.Signed(true)
                .Action("authenticated")
                .Secure(true)
                .ResourceType("image")
                .BuildUrl($"{publicId}.pdf");
            Log.Information(
                "URL firmada generada para el CV con PublicId {PublicId}: {SignedUrl}",
                publicId,
                signedUrl
            );
            return signedUrl;
        }

        public async Task<bool> DeleteAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            Log.Information($"Eliminando imagen con PublicId: {publicId} de Cloudinary");
            var deleteResult = await _cloudinary.DestroyAsync(deletionParams);
            if (deleteResult.Error != null)
            {
                Log.Error(
                    $"Error al eliminar la imagen con PublicId: {publicId} de Cloudinary: {deleteResult.Error.Message}"
                );
                throw new Exception($"Error al eliminar la imagen: {deleteResult.Error.Message}");
            }
            Log.Information(
                $"Imagen con PublicId: {publicId} eliminada exitosamente de Cloudinary"
            );
            var result = await _fileRepository.DeleteAsync(publicId);
            if (result is bool && !result.Value!)
            {
                Log.Error(
                    $"Error al eliminar la imagen de la base de datos con PublicId: {publicId}"
                );
                throw new Exception("Error al eliminar la imagen de la base de datos");
            }
            else if (result is null)
            {
                Log.Warning($"La imagen no existe en la base de datos con PublicId: {publicId}");
                return false;
            }
            Log.Information(
                $"Imagen con PublicId: {publicId} eliminada exitosamente de la base de datos"
            );
            return true;
        }

        /// <summary>
        /// Elimina una imagen de Cloudinary de forma asíncrona.
        /// </summary>
        /// <param name="publicId">El ID público de la imagen a eliminar.</param>
        /// <returns>True si la eliminación fue exitosa, de lo contrario false.</returns>
        private async Task<bool> DeleteInCloudinaryAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            Log.Information($"Eliminando imagen con PublicId: {publicId} de Cloudinary");
            try
            {
                var deleteResult = await _cloudinary.DestroyAsync(deletionParams);
                if (deleteResult.Error != null)
                {
                    Log.Error(
                        $"Error al eliminar la imagen con PublicId: {publicId} de Cloudinary: {deleteResult.Error.Message}"
                    );
                    return false;
                }
                Log.Information(
                    $"Imagen con PublicId: {publicId} eliminada exitosamente de Cloudinary"
                );
                return true;
            }
            catch
            {
                Log.Error(
                    $"Error al eliminar la imagen con PublicId: {publicId} de Cloudinary: No es una publicId de cloudinary."
                );
                return false;
            }
        }

        /// <summary>
        /// Valida si el archivo es una imagen válida.
        /// </summary>
        /// <param name="file">El archivo a validar.</param>
        /// <returns>True si el archivo es una imagen válida, de lo contrario false.</returns>
        private static bool IsValidImageFile(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var skiaStream = new SKManagedStream(stream); // SkiaSharp es una librería de procesamiento de imágenes (dotnet add package SkiaSharp)
                using var codec = SKCodec.Create(skiaStream); // Crea un codec para leer la imagen (codificador/decodificador)

                return codec != null && codec.Info.Width > 0 && codec.Info.Height > 0;
            }
            catch (Exception ex)
            {
                Log.Warning($"Error validando imagen {file.FileName}: {ex.Message}");
                return false;
            }
        }

        private void ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                Log.Error("Intento de subir un archivo nulo o vacío");
                throw new ArgumentException("Archivo inválido");
            }

            if (file.Length > _maxFileSizeInBytes)
            {
                Log.Error(
                    $"El archivo {file.FileName} excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB"
                );
                throw new ArgumentException(
                    $"El archivo excede el tamaño máximo permitido de {_maxFileSizeInBytes / 1024 / 1024} MB"
                );
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(fileExtension))
            {
                Log.Error($"Extensión de archivo no permitida: {fileExtension}");
                throw new ArgumentException(
                    $"Extensión de archivo no permitida. Permitir: {string.Join(", ", _allowedExtensions)}"
                );
            }

            if (!IsValidImageFile(file))
            {
                Log.Error($"El archivo {file.FileName} no es una imagen válida");
                throw new ArgumentException("El archivo no es una imagen válida");
            }
        }

        private async Task RollbackCloudinaryUploads(List<string> publicIds)
        {
            var deletionTasks = publicIds.Select(publicId => DeleteInCloudinaryAsync(publicId));
            await Task.WhenAll(deletionTasks);
            Log.Information(
                $"Rollback de uploads a Cloudinary completado para PublicIds: {string.Join(", ", publicIds)}"
            );
        }
    }
}
