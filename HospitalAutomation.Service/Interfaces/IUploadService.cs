using Azure;
using HospitalAutomation.Service.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ResponseBase = HospitalAutomation.Service.Response.Response;
namespace HospitalAutomation.Service.Interfaces
{
    public interface IUploadService
    {

        Task<ResponseGeneric<string>> UploadUserImageAsync(IFormFile file, string fileKeyFromClient);
        string GetUserImageUrl(string fileKey);

    }
}
