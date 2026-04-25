
namespace ProgressApp.Core.Exceptions
{
    public class AppException : Exception
    {
        public string ResourceKey { get; }
        public object[]? Args { get; }

        public bool IsCritical { get; } = false;
        public AppException(string errorKey, bool isCritical = false, params object[] args) : base(errorKey)
        {
            ResourceKey = errorKey;
            Args = args;
            IsCritical = isCritical;
        }

        public AppException(string errorKey, bool isCritical, Exception inner, params object[] args)
            : base(errorKey, inner)
        {
            ResourceKey = errorKey;
            Args = args;
            IsCritical = isCritical;
        }
    }
}