using System.Net;

namespace StepanCarService.Core.Interfaces
{
    public interface IErrorMapper
    {
        (HttpStatusCode statusCode, string message) Map(string errorCode);
    }
}
