namespace chatRoomAPI.Model.Response
{
    public class ResponseRefreshToken
    {
        public string resultCode { get; set; }

        public ResponseRefreshTokenData data { get; set; }
    }

    public class ResponseRefreshTokenData
    {
        public string newJwt { get; set; }

        public string newRefreshToken { get; set; }
    }
}
