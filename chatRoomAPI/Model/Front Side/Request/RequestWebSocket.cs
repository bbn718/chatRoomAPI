﻿namespace ChatRoomAPI.Model.Request
{
    public class RequestWebSocket
    {
        public string chatRoomCode { get; set; }

        public string account { get; set; }

        public string message { get; set; }

        public DateTime timestamp { get; set; }
    }
}
