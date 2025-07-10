using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Response
{
   public class NoContentResponse:Response
    {
        public NoContentResponse() : base(true, "No Content")
        {
        }
        public static NoContentResponse Success()
        {
            return new NoContentResponse();
        }
    }
}
