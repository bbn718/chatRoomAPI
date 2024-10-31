using chatRoomAPI.Configuration;
using chatRoomAPI.Model.Request;
using chatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace chatRoomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateChatRoomController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CreateChatRoomController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost, Authorize]
        public IActionResult CreateChatRoom([FromBody] RequestCreateChatRoom requestData)
        {
            try
            {
                string account = requestData.account;
                string chatRoomPassword = requestData.chatRoomPassword;
                string chatRoomName = requestData.chatRoomName;
                string description = requestData.description ?? "";
                DateTime dtCurrent = DateTime.Now;

                DatabaseSettings dbs = new DatabaseSettings(_configuration);
                string connectionString = dbs.connectionString;

                string chatRoomCode = Guid.NewGuid().ToString().Replace("-", "");
                chatRoomCode = chatRoomCode[..15];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string strSqlIrt = @"INSERT CHAT_ROOM
                                            (S_CHAR_CODE,
                                             S_CHAR_PASSWORD,
                                             S_CHAR_NAME,
                                             S_CHAR_CREATOR,
                                             S_CHAR_DESCRIPTION,
                                             D_CHAR_CREATEDATE,
                                             I_CHAR_STATUS)
                                         VALUES
                                            (@S_CHAR_CODE,
                                             @S_CHAR_PASSWORD,
                                             @S_CHAR_NAME,
                                             @S_CHAR_CREATOR,
                                             @S_CHAR_DESCRIPTION,
                                             @D_CHAR_CREATEDATE,
                                             @I_CHAR_STATUS)";

                    using (SqlCommand command = new SqlCommand(strSqlIrt, connection))
                    {
                        command.Parameters.Add("@S_CHAR_CODE", SqlDbType.VarChar).Value = chatRoomCode;
                        command.Parameters.Add("@S_CHAR_PASSWORD", SqlDbType.VarChar).Value = chatRoomPassword;
                        command.Parameters.Add("@S_CHAR_NAME", SqlDbType.VarChar).Value = chatRoomName;
                        command.Parameters.Add("@S_CHAR_CREATOR", SqlDbType.VarChar).Value = account;
                        command.Parameters.Add("@S_CHAR_DESCRIPTION", SqlDbType.VarChar).Value = description;
                        command.Parameters.Add("@D_CHAR_CREATEDATE", SqlDbType.DateTime).Value = dtCurrent;
                        command.Parameters.Add("@I_CHAR_STATUS", SqlDbType.TinyInt).Value = 1;

                        int result = command.ExecuteNonQuery();

                        if (result <= 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "寫入 chatRoomCode 失敗，請聯繫開發人員！"
                            };

                            return Ok(errorData);
                        }
                    }
                }

                ResponseCreateChatRoom responseData = new ResponseCreateChatRoom()
                {
                    resultCode = "10",
                    chatRoomCode = chatRoomCode,
                    message = $"成功創建聊天室，代碼為[{chatRoomCode}]"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"CreateChatRoom Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
