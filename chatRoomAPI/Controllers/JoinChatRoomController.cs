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
                if (string.IsNullOrEmpty(requestData.account) || string.IsNullOrEmpty(requestData.chatRoomCode))
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "參數錯誤！"
                    };

                    return Ok(errorData);
                }

                DatabaseSettings dbs = new DatabaseSettings(_configuration);
                string connetionString = dbs.connectionString;

                using (SqlConnection connection = new SqlConnection(connetionString))
                {
                    connection.Open();

                    string strSqlChk = @"SELECT 0 FROM CHAT_ROOM WHERE ";

                    using (SqlCommand command = new SqlCommand(strSqlChk, connection))
                    {
                        command.Parameters.Add("@", SqlDbType.VarChar).Value = requestData.account;
                        command.Parameters.Add("@", SqlDbType.VarChar).Value = requestData.chatRoomCode;

                        int result = command.ExecuteNonQuery();

                        if (result == 0)
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
                    message = "登出成功！"
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
