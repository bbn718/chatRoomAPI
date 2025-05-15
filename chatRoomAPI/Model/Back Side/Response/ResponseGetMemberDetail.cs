namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseGetMemberDetail
    {
        public string resultCode { get; set; }
        public GetMemberDetail data { get; set; }
        public string message { get; set; }
    }

    public class GetMemberDetail
    {
        public string picture { get; set; }
        public string name { get; set; }
        public string account { get; set; }
        public string loginType { get; set; }
        public string role { get; set; }
        public string status { get; set; }
    }
}
