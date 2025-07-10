using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Response
{
    public interface IResponse
    {
        bool IsSuccess { get; }
        string Message { get; }
    }
}
