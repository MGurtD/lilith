namespace Application.Contracts
{
    public class GenericResponse
    {
        public bool Result { get; }
        public IList<string> Errors { get; }
        public string? ErrorCode { get; }
        public object? Content { get; set; }

        public GenericResponse(bool result, object? content = null)
        {
            Result = result;
            Errors = new List<string>();
            Content = content;
        }

        public GenericResponse(bool result, string error, object? content = null, string? errorCode = null)
        {
            Result = result;
            Errors = new List<string> { error };
            ErrorCode = errorCode;
            Content = content;
        }

        public GenericResponse(bool result, IList<string> errors, object? content = null, string? errorCode = null)
        {
            Result = result;
            Errors = errors;
            ErrorCode = errorCode;
            Content = content;
        }
    }
}
