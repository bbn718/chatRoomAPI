using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model;
using ChatRoomAPI.Model.Back_Side.Request;
using ChatRoomAPI.Model.Response;
using ChatRoomAPI.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChatRoomAPI.Controllers.Back_Side
{
    [ApiExplorerSettings(GroupName = "back")]
    [Route("api/back/[controller]")]
    [ApiController]
    public class AdminLoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AdminLoginController(IConfiguration configuration, ITokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost, AllowAnonymous]
        public IActionResult AdminLogin([FromBody] RequestAdminLogin requestData)
        {
            string strSqlChkAdminAccount = @"SELECT I_ADMD_ERRNUM FROM ADMIN_DATA WHERE S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT";
            string strSqlGetAdminData = @"SELECT S_ADMD_ID, S_ADMD_NAME, S_ADMD_ACCOUNT FROM ADMIN_DATA
                                          WHERE S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT AND S_ADMD_PASSWORD = @S_ADMD_PASSWORD";
            string strSqlUpdAdmin = @"UPDATE ADMIN_DATA SET I_ADMD_ERRNUM = @I_ADMD_ERRNUM
                                      WHERE S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT";
            short errnum = 0;
            string token = "";

            try
            {
                if (requestData == null)
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "傳入參數為空！"
                    };

                    return Ok(errorData);
                }

                if (string.IsNullOrEmpty(requestData.account))
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "帳號參數錯誤！"
                    };

                    return Ok(errorData);
                }

                if (string.IsNullOrEmpty(requestData.password))
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "密碼參數錯誤！"
                    };

                    return Ok(errorData);
                }

                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlChkAdminAccount, connection))
                    {
                        command.Parameters.Add("@S_ADMD_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);
                        
                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "帳號或密碼輸入錯誤！"
                            };

                            return Ok(errorData);
                        }

                        errnum = Convert.ToInt16(dtbResult.Rows[0]["I_ADMD_ERRNUM"]);

                        if (errnum >= 3)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "密碼錯誤三次！您的帳號已被系統鎖定，請聯繫最高權限管理人 Neil 解鎖！"
                            };

                            return Ok(errorData);
                        }
                    }

                    using (SqlCommand command = new SqlCommand(strSqlGetAdminData, connection))
                    {
                        command.Parameters.Add("@S_ADMD_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;
                        command.Parameters.Add("@S_ADMD_PASSWORD", SqlDbType.VarChar).Value = requestData.password;

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        using (SqlCommand commandUpd = new SqlCommand(strSqlUpdAdmin, connection))
                        {
                            commandUpd.Parameters.Add("@S_ADMD_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;

                            if (dtbResult.Rows.Count == 0)
                            {
                                commandUpd.Parameters.Add("@I_ADMD_ERRNUM", SqlDbType.SmallInt).Value = errnum + 1; //錯誤次數+1

                                commandUpd.ExecuteNonQuery();

                                ResponseErrorMessage errorData = new ResponseErrorMessage()
                                {
                                    resultCode = "01",
                                    errorMessage = "密碼輸入錯誤！"
                                };

                                return Ok(errorData);
                            }

                            commandUpd.Parameters.Add("@I_ADMD_ERRNUM", SqlDbType.SmallInt).Value = 0 ; //錯誤次數歸零

                            commandUpd.ExecuteNonQuery();
                        }

                        Claim[] claims =
                        {
                            new Claim("id", dtbResult.Rows[0]["S_ADMD_ID"]?.ToString() ?? ""),
                            new Claim("name", dtbResult.Rows[0]["S_ADMD_NAME"]?.ToString()  ?? ""),
                            new Claim("account", dtbResult.Rows[0]["S_ADMD_ACCOUNT"]?.ToString() ?? "")
                        };

                        token = _tokenService.GenerateJwt(claims);

                        ResponseLoginData data = new ResponseLoginData() { token = token };

                        ResponseLogin responseData = new ResponseLogin()
                        {
                            resultCode = "10",
                            data = data,
                            message = $"登入成功，歡迎使用者 [{requestData.account}]！"
                        };

                        return Ok(responseData);
                    }
                }
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"AdminLogin Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
