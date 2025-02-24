using System.ComponentModel.DataAnnotations;

namespace chatRoomAPI.Model.Request
{
    public class RequestJoinChatRoom
    {
        public string chatRoomCode { get; set; }

        public string chatRoomPassword { get; set; }
    }
}
