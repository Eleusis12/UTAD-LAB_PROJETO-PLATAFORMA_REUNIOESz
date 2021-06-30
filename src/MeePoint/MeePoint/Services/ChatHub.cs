using MeePoint.Data;
using MeePoint.Models;
using Microsoft.AspNetCore.Mvc;
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
        bool quorumAlert = false;

        async public void UpdateOnline()
        {
            await Clients.All.SendAsync("UpdateOnline", CurrentConnections.ToList());
        }

        public List<string> GetAllActiveConnections()
        {
            return CurrentConnections.ToList();
        }

        public Task JoinRoom(string meetingId)
        {
            var user = Context.User;
            CurrentConnections.Add(user.Identity.Name);

            UpdateOnline();

            return Groups.AddToGroupAsync(Context.ConnectionId, meetingId);
        }

        public Task LeaveRoom(string meetingId)
        {
            var user = Context.User;

                CurrentConnections.Remove(user.Identity.Name);
                UpdateOnline();

            return Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId);
        }
    }
}
