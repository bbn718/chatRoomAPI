namespace chatRoomAPI.Model.Response
{
    public class ResponseLogin
    {
        public string resultCode { get; set; }

        public ResponseLoginData data { get; set; }

        public string message { get; set; }
    }

    public class ResponseLoginData
    {
        public string token { get; set; }

        public string refreshToken { get; set; }
    }
}
