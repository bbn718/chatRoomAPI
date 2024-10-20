namespace chatRoomAPI.Model
{
    public class ResponseLogin
    {
        public string resultCode { get; set; }

        public string token { get; set; }

        public string refreshToken { get; set; }
    }
}
