using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.DTOs
{
   
        public class PatientDto
        {

            public string FullName { get; set; }
            public int Id { get; set; }
            public DateTime BirthDate { get; set; }
            public string Gender { get; set; }

        }
    }


