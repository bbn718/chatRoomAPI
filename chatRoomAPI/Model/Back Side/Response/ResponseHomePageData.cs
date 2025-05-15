using ChatRoomAPI.Model.Response;

namespace ChatRoomAPI.Model.Back_Side.Response
{
    public class ResponseHomePageData
    {
        public string resultCode { get; set; }
        public List<HomePageData>? data { get; set; }
        public string message { get; set; }
    }

    public class HomePageData
    {
        public string name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? linkedInURL { get; set; }
        public string? gitHubURL { get; set; }
        public string? picture { get; set; }
    }
}
