using chatRoomAPI.Model.Response;

namespace chatRoomAPI.Model.Back_Side.Response
{
    public class ResponseAdminGroupData
    {
        public string resultCode { get; set; }

        public List<ResponseAdminGroupDataItem>? data { get; set; }
    }

    public class ResponseAdminGroupDataItem
    {
        public string name { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public string? linkedinURL { get; set; }
        public string? githubURL { get; set; }
    }
}
