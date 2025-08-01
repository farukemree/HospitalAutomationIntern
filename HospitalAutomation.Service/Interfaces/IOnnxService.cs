using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Interfaces
{
    public interface IOnnxService
    {
        string Predict(string inputText);
    }

}
