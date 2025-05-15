namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseGetMemberList
    {
        public string resultCode { get; set; }
        public List<GetMemberList>? data { get; set; }
        public string message { get; set; }
    }

    public class GetMemberList
    {
        public string name { get; set; }
        public string account { get; set; }
        public string status { get; set; }
    }
}