namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseGetBulletinsDetail
    {
        public string resultCode { get; set; }
        public GetBulletinsDetail data { get; set; }
        public string message { get; set; }
    }

    public class GetBulletinsDetail
    {
        public string title { get; set; }
        public string summary { get; set; }
        public string content { get; set; }
        public string status { get; set; }
    }
}
