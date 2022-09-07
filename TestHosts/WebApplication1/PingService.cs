using System.ServiceModel;

namespace WebApplication1
{
    [ServiceContract]
    public interface IPingService
    {
        [OperationContract]
        string Ping(String message);
    }

    public class PingService : IPingService
    {
        public string Ping(string msg)
        {
            return string.Join(string.Empty, msg.Reverse());
        }
    }
}
