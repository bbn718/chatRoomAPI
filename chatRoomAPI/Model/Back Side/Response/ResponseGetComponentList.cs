namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseGetComponentList
    {
        public string resultCode { get; set; }
        public List<GetComponentList>? data { get; set; }
        public string message { get; set; }
    }

    public class GetComponentList
    {
        public string domId { get; set; }
        public string componentName { get; set; }
    }
}
