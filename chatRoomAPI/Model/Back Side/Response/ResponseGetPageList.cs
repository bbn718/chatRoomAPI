namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseGetPageList
    {
        public string resultCode { get; set; }
        public List<GetPageList>? data { get; set; }
        public string message { get; set; }
    }

    public class GetPageList
    {
        public string pageName { get; set; }
        public string path { get; set; }
        public string[] authentication { get; set; }
    }
}
