namespace backend.src.Infrastructure.Exceptions
{
    [Serializable]
    public class EmailNotVerifiedException : Exception
    {
        public string Email { get; }

        public EmailNotVerifiedException(string email)
            : base($"El email '{email}' no ha sido verificado.")
        {
            Email = email;
        }

        public EmailNotVerifiedException(string email, string message)
            : base(message)
        {
            Email = email;
        }

        public EmailNotVerifiedException(string email, string message, Exception inner)
            : base(message, inner)
        {
            Email = email;
        }
    }
}
