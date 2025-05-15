using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model.Back_Side.Response;
using ChatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using ChatRoomAPI.Model.Back_Side.Request;

namespace ChatRoomAPI.Controllers.Back_Side
{
    [ApiExplorerSettings(GroupName = "back")]
    [Route("api/back/[controller]")]
    [ApiController]
    public class ModifyPasswordController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ModifyPasswordController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult ModifyPassword([FromBody] RequestModifyPassword request)
        {
            string strSqlChk = @"SELECT S_ADMD_PASSWORD FROM ADMIN_DATA WHERE S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT";
            string strSqlUpd = @"UPDATE ADMIN_DATA SET S_ADMD_PASSWORD = @S_ADMD_PASSWORD WHERE S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT";

            try
            {
                if (string.IsNullOrEmpty(request.account))
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "帳號參數錯誤！"
                    };

                    return Ok(errorData);
                }

                if (string.IsNullOrEmpty(request.oldPassword))
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "舊密碼參數錯誤！"
                    };

                    return Ok(errorData);
                }

                if (string.IsNullOrEmpty(request.newPassword))
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "新密碼參數錯誤！"
                    };

                    return Ok(errorData);
                }

                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlChk, connection))
                    {
                        command.Parameters.Add("@S_ADMD_ACCOUNT", SqlDbType.VarChar).Value = request.account;

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "該帳號已不存在，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        if (request.oldPassword != dtbResult.Rows[0][0].ToString().Trim())
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "舊密碼輸入錯誤，請確認！"
                            };

                            return Ok(errorData);
                        }

                        if (request.newPassword == dtbResult.Rows[0][0].ToString().Trim())
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "新密碼與舊密碼相同，請確認！"
                            };

                            return Ok(errorData);
                        }
                    }

                    using (SqlCommand command = new SqlCommand(strSqlUpd, connection))
                    {
                        command.Parameters.Add("@S_ADMD_PASSWORD", SqlDbType.VarChar).Value = request.newPassword;
                        command.Parameters.Add("@S_ADMD_ACCOUNT", SqlDbType.VarChar).Value = request.account;

                        command.ExecuteNonQuery();
                    }
                }

                ResponseModifyPassword responseData = new ResponseModifyPassword()
                {
                    resultCode = "10",
                    message = "修改密碼成功，請重新登入！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"ModifyPassword Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
