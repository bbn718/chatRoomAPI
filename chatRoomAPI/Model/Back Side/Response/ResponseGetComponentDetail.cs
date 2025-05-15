namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseGetComponentDetail
    {
        public string resultCode { get; set; }
        public GetComponentDetail data { get; set; }
        public string message { get; set; }
    }

    public class GetComponentDetail
    {
        public string componentName { get; set; }
        public string domId { get; set; }
        public string memo { get; set; }
    }
}
