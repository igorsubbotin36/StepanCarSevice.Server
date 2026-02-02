using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarSevice.AuthService.Application.Interfaces.Services
{
    public interface IErrorMapper
    {
        (HttpStatusCode statusCode, string message) Map(string errorCode);
    }
}
