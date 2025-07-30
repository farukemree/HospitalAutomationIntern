﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.DataAccess.DTOs
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string Code { get; set; }
    }
    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; }
    }

    public class VerifyResetCodeDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }


}

