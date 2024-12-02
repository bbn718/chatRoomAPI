namespace chatRoomAPI.Model.Response
{
    public class ResponseCreateChatRoom
    {
        public string resultCode { get; set; }

        public ResponseCreateChatRoomData data { get; set; }

        public string message { get; set; }
    }

    public class ResponseCreateChatRoomData
    {
        public string chatRoomCode { get; set; }
    }
}
