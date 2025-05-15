using System.ComponentModel.DataAnnotations;

namespace ChatRoomAPI.Model
{
    public class RequestRefreshToken
    {
        [Required]
        public string account { get; set; }

        [Required]
        public string token { get; set; }

        [Required]
        public string refreshToken { get; set; }    
    }
}
