namespace chatRoomAPI.Model
{
    public class RequestLogin
    {
        public int? loginType { get; set; }

        public string account { get; set; }

        public string password { get; set; }

        public string? name { get; set; }

        public string? pic { get; set; }
    }
}
