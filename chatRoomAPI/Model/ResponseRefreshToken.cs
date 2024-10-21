namespace chatRoomAPI.Model
{
    public class ResponseRefreshToken
    {
        public string resultCode { get; set; }

        public string newJwt { get; set; }

        public string newRefreshToken { get; set; }
    }
}
