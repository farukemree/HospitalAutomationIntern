using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;


namespace HospitalAutomation.API.Hubs

{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task JoinDoctorGroup(string doctorId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, doctorId);
        }
    }
}
