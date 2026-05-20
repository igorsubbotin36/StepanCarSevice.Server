using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StepanCarService.Common.Application.Interfaces
{
    public interface IErrorMapper
    {
        (HttpStatusCode statusCode, string message) Map(string errorCode);
    }
}
