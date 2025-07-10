using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Response
{
    public class ResponseGeneric<T> : Response
    {
        public T Data { get; set; }
        public ResponseGeneric(bool isSuccess, string message, T data) : base(isSuccess, message)
        {
            Data = data;
        }
        public static ResponseGeneric<T> Success(T data, string message = "")
        {
            return new ResponseGeneric<T>(true, message, data);
        }
        public static ResponseGeneric<T> Error(string message = "")
        {
            return new ResponseGeneric<T>(false, message, default(T));
        }
    }
}
