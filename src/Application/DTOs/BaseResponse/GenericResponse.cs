namespace backend.src.Application.DTOs.BaseResponse;

/// <summary>
/// Clase que representa una respuesta genérica de la aplicación.
/// </summary>
/// <typeparam name="T">Tipo de datos que contiene la respuesta.</typeparam>
/// <param name="message">Mensaje de la respuesta.</param>
/// <param name="data">Datos de la respuesta (opcional).</param>
public class GenericResponse<T>(string message, T? data = default)
{
    /// <summary>
    /// Mensaje principal de la respuesta.
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// Datos de la respuesta (pueden ser nulos).
    /// </summary>
    public T? Data { get; set; } = data;
}
