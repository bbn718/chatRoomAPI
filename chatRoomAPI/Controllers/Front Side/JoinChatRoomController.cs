using chatRoomAPI.Configuration;
using chatRoomAPI.Model.Request;
using chatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace chatRoomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JoinChatRoomController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public JoinChatRoomController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost, Authorize]
        public IActionResult JoinChatRoom (RequestJoinChatRoom requestData)
        {
            try
            {
                if (string.IsNullOrEmpty(requestData.chatRoomCode) || string.IsNullOrEmpty(requestData.chatRoomPassword))
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

                    string strSqlChk = @"SELECT 0 FROM CHAT_ROOM
                                         WHERE S_CHAR_CODE = @S_CHAR_CODE 
                                           AND S_CHAR_PASSWORD = @S_CHAR_PASSWORD
                                           AND I_CHAR_STATUS = @I_CHAR_STATUS";

                    using (SqlCommand command = new SqlCommand(strSqlChk, connection))
                    {
                        command.Parameters.Add("@S_CHAR_CODE", SqlDbType.VarChar).Value = requestData.chatRoomCode;
                        command.Parameters.Add("@S_CHAR_PASSWORD", SqlDbType.VarChar).Value = requestData.chatRoomPassword;
                        command.Parameters.Add("@I_CHAR_STATUS", SqlDbType.TinyInt).Value = 1;

                        //int result = Convert.ToInt32(command.ExecuteScalar());

                        if (command.ExecuteScalar() is null)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = $"加入失敗，查無該聊天室: {requestData.chatRoomCode}！"
                            };

                            return Ok(errorData);
                        }
                    }
                }

                ResponseJoinChatRoom responseData = new ResponseJoinChatRoom()
                {
                    resultCode = "10",
                    message = $"成功加入聊天室[{requestData.chatRoomCode}]！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"JoinChatRoom Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
