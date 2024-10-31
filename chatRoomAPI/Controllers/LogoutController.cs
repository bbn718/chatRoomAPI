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
    public class LogoutController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LogoutController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost, Authorize]
        public IActionResult Logout([FromBody] RequestLogout requestData)
        {
            try
            {
                string account = requestData.account;
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 資料
                string connectionString = dbs.connectionString; //取得連線字串

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string strSqlChk = @"SELECT 0 FROM USER_DATA WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT";

                    using (SqlCommand command = new SqlCommand(strSqlChk, connection))
                    {
                        command.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = account;

                        if (command.ExecuteScalar() is null)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無使用者，無法執行登出，請聯繫開發人員！"
                            };

                            return Ok(errorData);
                        }
                    }

                    #region 重設 refreshToken
                    string strSqlUpd = @"UPDATE USER_DATA SET S_USED_REFRESHTOKEN = @S_USED_REFRESHTOKEN,
                                                              D_USED_REFRESHTOKENEXPIRYDATE = @D_USED_REFRESHTOKENEXPIRYDATE
                                         WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT";

                    using (SqlCommand command = new SqlCommand(strSqlUpd, connection))
                    {
                        command.Parameters.Add("@S_USED_REFRESHTOKEN", SqlDbType.VarChar).Value = DBNull.Value;
                        command.Parameters.Add("@D_USED_REFRESHTOKENEXPIRYDATE", SqlDbType.DateTime).Value = DBNull.Value;
                        command.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = account;

                        int result = command.ExecuteNonQuery();

                        if (result <= 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "更新 refreshToken 發生錯誤，請聯繫開發人員！"
                            };

                            return Ok(errorData);
                        }
                    }
                    #endregion
                }

                ResponseLogout responseData = new ResponseLogout()
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
                    errorMessage = $"Logout Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
