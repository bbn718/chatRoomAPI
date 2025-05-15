namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseGetPageDetail
    {
        public string resultCode { get; set; }
        public GetPageDetail data { get; set; }
        public string message { get; set; }
    }
    public class GetPageDetail
    {
        public string pageName { get; set; }
        public string path { get; set; }
        public string parent { get; set; }
        public string order { get; set; }
        public List<GetPageComponent> component { get; set; }
    }

    public class GetPageComponent
    {
        public string componentName { get; set; }
        public string domId { get; set; }
        public string position { get; set; }
    }
}
