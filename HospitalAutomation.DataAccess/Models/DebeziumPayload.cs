using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.Models
{
    public class DebeziumPayload<T>
    {
        public T before { get; set; }
        public T after { get; set; }
    }

}
