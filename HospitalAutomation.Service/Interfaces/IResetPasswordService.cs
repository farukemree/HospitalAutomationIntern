using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Interfaces
{
    public interface IResetPasswordService
    {
        Task<ResponseGeneric<bool>> ResetPasswordAsync(ResetPasswordDto dto);
        Task<ResponseGeneric<bool>> SendResetCodeAsync(ForgotPasswordRequestDto dto);
        Task<ResponseGeneric<bool>> VerifyResetCodeAsync(VerifyResetCodeDto dto);
    }
}
