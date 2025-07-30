using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.Models
{
    public class ResetPassword
    {
            public int Id { get; set; }
            public string Email { get; set; }
            public string Code { get; set; }
            public DateTime Expiration { get; set; }
            public bool Used { get; set; }
        

    }
}
