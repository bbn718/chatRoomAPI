namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseGetBulletinsList
    {
        public string resultCode { get; set; }
        public List<GetBulletinsList>? data { get; set; }
        public string message { get; set; }
    }

    public class GetBulletinsList
    {
        public string title { get; set; }
        public string summary { get; set; }
        public string status { get; set; }
    }
}
