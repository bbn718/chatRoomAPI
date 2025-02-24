namespace chatRoomAPI.Model.Request
{
    public class RequestCreateChatRoom
    {
        public string account { get; set; }

        public string chatRoomPassword { get; set; }


        public string chatRoomName { get; set; }

        public string? description { get; set; }
    }
}
