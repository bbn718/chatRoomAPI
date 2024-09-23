using chatRoomAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace chatRoomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineAccountLoginController : ControllerBase
    {
                public IConfiguration Configuration { get; }
        [HttpPost]
        public IActionResult LineAccountLogin([FromBody] RequestLineAccountLogin requestData)
        {
            try
            {
                DatabaseSettings dbs = new DatabaseSettings(Configuration);
                string connectionString;

                using (SqlConnection connection = new SqlConnection())
                {
                    if (requestData == null)
                    {
                        return Ok(new ResponseErrorMessage
                        {
                            ResultCode = "1",
                            ErrorMessage = "傳入參數為空！"
                        });
                    }

                    if (string.IsNullOrEmpty(requestData.UserId) || string.IsNullOrEmpty(requestData.UserName))
                    {
                        return Ok(new ResponseErrorMessage
                        {
                            ResultCode = "1",
                            ErrorMessage = "傳入參數錯誤！"
                        });
                    }





                }

                return Ok();
            }
            catch (Exception ex)
            {
                return Conflict();
            }

        }
    }
}
