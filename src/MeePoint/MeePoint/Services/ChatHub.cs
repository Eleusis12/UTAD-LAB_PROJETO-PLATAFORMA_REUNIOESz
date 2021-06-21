using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeePoint.Services
{
    public class ChatHub : Hub
    {
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
