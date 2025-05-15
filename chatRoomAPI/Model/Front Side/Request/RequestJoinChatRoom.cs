using System.ComponentModel.DataAnnotations;

namespace ChatRoomAPI.Model.Request
{
    public class RequestJoinChatRoom
    {
        public string chatRoomCode { get; set; }

        public string chatRoomPassword { get; set; }
    }
}
