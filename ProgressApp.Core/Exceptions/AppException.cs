
namespace ProgressApp.Core.Exceptions
{
    public class AppException : Exception
    {
        public string ResourceKey { get; }
        public object[]? Args { get; }

        public AppException(string errorKey, params object[] args) : base(errorKey)
        {
            ResourceKey = errorKey;
            Args = args;
        }

        public AppException(string errorKey, Exception inner, params object[] args)
            : base(errorKey, inner)
        {
            ResourceKey = errorKey;
            Args = args;
        }
    }
}