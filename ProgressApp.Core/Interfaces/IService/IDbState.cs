namespace ProgressApp.Core.Interfaces.IService
{
    public interface IDbState
    {
        string DbPath { get; } 
        bool IsAuthenticated { get; }
        void SetPassword(string password);
        string GetConnectionString();
    }
}
