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

        [HttpGet, Authorize]
        public IActionResult CreateChatRoom([FromBody] RequestCreateChatRoom requestData)
        {
            try
            {
                string account = requestData.account;
                string chatRoomName = requestData.chatRoomName;
                string description = requestData.description ?? "";

                DatabaseSettings dbs = new DatabaseSettings(_configuration);
                string connectionString = dbs.connectionString;

                string chatRoomCode = Guid.NewGuid().ToString().Replace("-", "");
                chatRoomCode = chatRoomCode[..15];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string strSqlIrt = @"INSERT CHAT_ROOM ()
                                         VALUES (@, @, @)";

                    using (SqlCommand command = new SqlCommand(strSqlIrt, connection))
                    {
                        command.Parameters.Add("@", SqlDbType.VarChar).Value = account;
                        command.Parameters.Add("@", SqlDbType.VarChar).Value = chatRoomName;
                        command.Parameters.Add("@", SqlDbType.VarChar).Value = description;

                        int result = command.ExecuteNonQuery();

                        if (result == 0)
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
                    chatRoomCode = chatRoomCode
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
