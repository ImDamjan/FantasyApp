namespace FantasyApp.BusinessLogic
{
    public class ServiceResult<T>
    {
        public bool Succeeded { get; private init; }
        public T? Data { get; private init; }
        public string? ErrorMessage { get; private init; }

        public static ServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };
        public static ServiceResult<T> Failure(string errorMessage) => new() { Succeeded = false, ErrorMessage = errorMessage };
    }
}
