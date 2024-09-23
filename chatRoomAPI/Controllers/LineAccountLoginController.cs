using chatRoomAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace chatRoomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineAccountLoginController : ControllerBase
    {
        [HttpPost]
        public IActionResult LineAccountLogin([FromBody] RequestLineAccountLogin requestData)
        {
            try
            {
                if (requestData == null)
                {
                    return Ok(new ResponseErrorMessage
                    {
                        ResultCode = "1",
                        ErrorMessage = "傳入參數為空！"
                    });
                }

                if (string.IsNullOrEmpty(requestData.UserName) || string.IsNullOrEmpty(requestData.TokenSecret))
                {
                    return Ok(new ResponseErrorMessage
                    {
                        ResultCode = "1",
                        ErrorMessage = "傳入參數錯誤！"
                    });
                }


            }
            catch (Exception ex)
            {

            }

            return Ok();
        }
    }
}
