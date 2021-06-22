using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeePoint.Services
{
    public class ChatHub : Hub
    {

        static HashSet<string> CurrentConnections = new HashSet<string>();

        public override Task OnConnectedAsync()
        {
            var user = Context.User;
            CurrentConnections.Add(user.Identity.Name);

            UpdateOnline();

            return base.OnConnectedAsync();
        }

        async public void UpdateOnline()
        {
            await Clients.All.SendAsync("UpdateOnline", CurrentConnections.ToList());
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connection = CurrentConnections.FirstOrDefault(x => x == Context.ConnectionId);

            if (connection != null)
            {
                CurrentConnections.Remove(connection);
                UpdateOnline();
            }

            return base.OnDisconnectedAsync(exception);
        }

        public List<string> GetAllActiveConnections()
        {
            return CurrentConnections.ToList();
        }

        public Task JoinRoom(string meetingId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, meetingId);
        }

        public Task LeaveRoom(string meetingId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId);
        }
    }
}
