namespace ChatRoomAPI.Model.Back_Side.Request
{
    public class RequestModifyPassword
    {
        public string account { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
    }
}
