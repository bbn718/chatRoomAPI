namespace ChatRoomAPI.Model.Response
{
    public class ResponseGetChatRoomInfo
    {
        public string resultCode { get; set; }

        public ResponseGetChatRoomInfoData? data { get; set; }

        public string message { get; set; }
    }

    public class ResponseGetChatRoomInfoData
    {
        public string chatRoomCode { get; set; }

        public string chatRoomName { get; set; }

        public List<chatRoomUsersHistoryData> historyData { get; set; }
    }

    public class chatRoomUsersHistoryData
    {
        public string userNickName { get; set; }

        public string userPic { get; set; }

        public string historyMessage { get; set; }

        public DateTime timestamp { get; set; }
    }
}
