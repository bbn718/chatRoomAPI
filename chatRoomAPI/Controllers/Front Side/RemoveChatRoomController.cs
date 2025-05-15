using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model.Request;
using ChatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ChatRoomAPI.Controllers
{
    [ApiExplorerSettings(GroupName = "front")]
    [Route("api/front/[controller]")]
    [ApiController]
    public class RemoveChatRoomController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RemoveChatRoomController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpDelete("{chatRoomCode}"), Authorize]
        public IActionResult RemoveChatRoom(string chatRoomCode)
        {
            try
            {
                if (string.IsNullOrEmpty(chatRoomCode))
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "參數錯誤！"
                    };

                    return Ok(errorData);
                }

                DatabaseSettings dbs = new DatabaseSettings(_configuration);
                string connectionString = dbs.connectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string strSqlDlt = "UPDATE CHAT_ROOM SET I_CHAR_STATUS = @I_CHAR_STATUS WHERE S_CHAR_CODE = @S_CHAR_CODE";

                    using (SqlCommand command = new SqlCommand(strSqlDlt, connection))
                    {
                        command.Parameters.Add("@I_CHAR_STATUS", SqlDbType.TinyInt).Value = 0;
                        command.Parameters.Add("@S_CHAR_CODE", SqlDbType.VarChar).Value = chatRoomCode;

                        int result = command.ExecuteNonQuery();

                        if (result <= 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無可刪除的聊天室！"
                            };

                            return Ok(errorData);
                        }
                    }
                }

                ResponseRemoveChatRoom responseData = new ResponseRemoveChatRoom()
                {
                    resultCode = "10",
                    message = $"成功刪除聊天室[{chatRoomCode}]！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"RemoveChatRoom Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
