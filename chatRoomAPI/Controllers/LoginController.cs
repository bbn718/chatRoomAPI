using chatRoomAPI.Configuration;
using chatRoomAPI.Model;
using chatRoomAPI.Model.Response;
using chatRoomAPI.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace chatRoomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        DateTime createDate = DateTime.Now;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public LoginController(IConfiguration configuration, ITokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost, AllowAnonymous]
        public IActionResult Login([FromBody] RequestLogin requestData)
        {
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

                if (requestData.loginType != 1 && requestData.loginType != 2)
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "登入類型參數錯誤！"
                    };

                    return Ok(errorData);
                }

                if (requestData.loginType == 1) //使用一般登入
                {
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
                }
                else //使用第三方登入
                {
                    if (string.IsNullOrEmpty(requestData.account))
                    {
                        ResponseErrorMessage errorData = new ResponseErrorMessage()
                        {
                            resultCode = "01",
                            errorMessage = "ID 參數錯誤！"
                        };

                        return Ok(errorData);
                    }
                }

                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 資料
                string connectionString = dbs.connectionString; //取得連線字串
                string token = "";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (requestData.loginType == 1) //使用一般登入
                    {
                        string strSql = @"SELECT S_USED_ACCOUNT, S_USED_NICKNAME, S_USED_PICTURE
                                          FROM USER_DATA
                                          WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT
                                            AND S_USED_PASSWORD = @S_USED_PASSWORD";

                        using (SqlCommand command = new SqlCommand(strSql, connection))
                        {
                            command.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;
                            command.Parameters.Add("@S_USED_PASSWORD", SqlDbType.VarChar).Value = requestData.password;

                            if (command.ExecuteScalar() is null)
                            {
                                ResponseErrorMessage errorData = new ResponseErrorMessage()
                                {
                                    resultCode = "01",
                                    errorMessage = "帳號或密碼輸入錯誤！"
                                };

                                return Ok(errorData);
                            }

                            DataTable dtbResult = (DataTable)command.ExecuteScalar();

                            Claim[] claims = new[]
                            {
                                new Claim("account", dtbResult.Rows[0]["S_USED_ACCOUNT"]?.ToString() ?? ""),
                                new Claim("nickname", dtbResult.Rows[0]["S_USED_NICKNAME"]?.ToString()  ?? ""),
                                new Claim("pic", dtbResult.Rows[0]["S_USED_PICTURE"]?.ToString() ?? "")
                            };

                            token = _tokenService.GenerateJwt(claims);
                        }
                    }
                    else if (requestData.loginType == 2) //使用第三方登入
                    {
                        string strSql = @"SELECT S_USED_ACCOUNT, S_USED_NICKNAME, S_USED_PICTURE
                                          FROM USER_DATA
                                          WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT";

                        using (SqlCommand command = new SqlCommand(strSql, connection))
                        {
                            command.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;

                            SqlDataAdapter sdaResult = new SqlDataAdapter(command);
                            DataTable dtbResult = new DataTable();

                            sdaResult.Fill(dtbResult);

                            if (dtbResult.Rows.Count == 0) //資料庫無該使用者則註冊
                            {
                                string strSqlIrt = @"INSERT USER_DATA
                                                       ([S_USED_ACCOUNT]
                                                       ,[S_USED_PASSWORD]
                                                       ,[S_USED_NICKNAME]
                                                       ,[S_USED_PICTURE]
                                                       ,[D_USED_BIRTHDATE]
                                                       ,[I_USED_SIGNUPTYPE]
                                                       ,[I_USED_STATUS]
                                                       ,[D_USED_CREATEDATE])
                                                     VALUES
                                                       (@S_USED_ACCOUNT
                                                       ,@S_USED_PASSWORD
                                                       ,@S_USED_NICKNAME
                                                       ,@S_USED_PICTURE
                                                       ,@D_USED_BIRTHDATE
                                                       ,@I_USED_SIGNUPTYPE
                                                       ,@I_USED_STATUS
                                                       ,@D_USED_CREATEDATE)";

                                using (SqlCommand commandIrt = new SqlCommand(strSqlIrt, connection))
                                {
                                    commandIrt.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;
                                    commandIrt.Parameters.Add("@S_USED_PASSWORD", SqlDbType.VarChar).Value = string.Empty;
                                    commandIrt.Parameters.Add("@S_USED_NICKNAME", SqlDbType.VarChar).Value = requestData.name;
                                    commandIrt.Parameters.Add("@S_USED_PICTURE", SqlDbType.VarChar).Value = requestData.pic;
                                    commandIrt.Parameters.Add("@D_USED_BIRTHDATE", SqlDbType.Date).Value = DBNull.Value;
                                    commandIrt.Parameters.Add("@I_USED_SIGNUPTYPE", SqlDbType.Int).Value = 2;
                                    commandIrt.Parameters.Add("@I_USED_STATUS", SqlDbType.Int).Value = 1;
                                    commandIrt.Parameters.Add("@D_USED_CREATEDATE", SqlDbType.DateTime).Value = createDate;

                                    int irtResult = commandIrt.ExecuteNonQuery();

                                    if (irtResult <= 0)
                                    {
                                        ResponseErrorMessage errorData = new ResponseErrorMessage()
                                        {
                                            resultCode = "01",
                                            errorMessage = "寫入資料庫錯誤，請聯繫開發人員！"
                                        };

                                        return Ok(errorData);
                                    }
                                }
                            }
                            else if (requestData.name != dtbResult.Rows[0]["S_USED_NICKNAME"].ToString()) //暱稱不同則更新
                            {
                                string strSqlUpd = @"UPDATE USER_DATA SET S_USED_NICKNAME = @S_USED_NICKNAME WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT";

                                using (SqlCommand commandUpd = new SqlCommand(strSqlUpd, connection))
                                {
                                    commandUpd.Parameters.Add("@S_USED_NICKNAME", SqlDbType.VarChar).Value = requestData.name;
                                    commandUpd.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;

                                    int updResult = commandUpd.ExecuteNonQuery();

                                    if (updResult <= 0)
                                    {
                                        ResponseErrorMessage errorData = new ResponseErrorMessage()
                                        {
                                            resultCode = "01",
                                            errorMessage = "更新使用者名稱錯誤，請聯繫開發人員！"
                                        };

                                        return Ok(errorData);
                                    }
                                }
                            }
                            else if (requestData.pic != dtbResult.Rows[0]["S_USED_PICTURE"].ToString()) //大頭貼不同則更新
                            {
                                string strSqlUpd = @"UPDATE USER_DATA SET S_USED_PICTURE = @S_USED_PICTURE WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT";

                                using (SqlCommand commandUpd = new SqlCommand(strSqlUpd, connection))
                                {
                                    commandUpd.Parameters.Add("@S_USED_PICTURE", SqlDbType.VarChar).Value = requestData.pic;
                                    commandUpd.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;

                                    int updResult = commandUpd.ExecuteNonQuery();

                                    if (updResult <= 0)
                                    {
                                        ResponseErrorMessage errorData = new ResponseErrorMessage()
                                        {
                                            resultCode = "01",
                                            errorMessage = "更新大頭貼錯誤，請聯繫開發人員！"
                                        };

                                        return Ok(errorData);
                                    }
                                }
                            }

                            sdaResult.Fill(dtbResult); //重新獲取寫入or更新後之使用者資料

                            Claim[] claims = new[]
                            {
                                new Claim("account", dtbResult.Rows[0]["S_USED_ACCOUNT"]?.ToString() ?? ""),
                                new Claim("nickname", dtbResult.Rows[0]["S_USED_NICKNAME"]?.ToString()  ?? ""),
                                new Claim("pic", dtbResult.Rows[0]["S_USED_PICTURE"]?.ToString() ?? "")
                            };

                            token = _tokenService.GenerateJwt(claims);
                        }
                    }

                    string refreshToken = _tokenService.GenerateRefreshToken();
                    string strSqlUpdRefreshToken = @"UPDATE USER_DATA SET S_USED_REFRESHTOKEN = @S_USED_REFRESHTOKEN,
                                                                          D_USED_REFRESHTOKENEXPIRYDATE = @D_USED_REFRESHTOKENEXPIRYDATE
                                                     WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT";

                    using (SqlCommand commandUpd = new SqlCommand(strSqlUpdRefreshToken, connection)) //更新使用者 refreshToken
                    {
                        commandUpd.Parameters.Add("@S_USED_REFRESHTOKEN", SqlDbType.VarChar).Value = refreshToken;
                        commandUpd.Parameters.Add("@D_USED_REFRESHTOKENEXPIRYDATE", SqlDbType.DateTime).Value = DateTime.Now.AddDays(Convert.ToDouble(_configuration.GetSection("JwtConfig")["RefreshTokenExpiryDay"]));
                        commandUpd.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;

                        int updResult = commandUpd.ExecuteNonQuery();

                        if (updResult <= 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "更新 RefreshToken 錯誤，請聯繫開發人員！"
                            };

                            return Ok(errorData);
                        }
                    }

                    ResponseLoginData data = new ResponseLoginData()
                    {
                        token = token,
                        refreshToken = refreshToken
                    };

                    ResponseLogin responseData = new ResponseLogin()
                    {
                        resultCode = "10",
                        data = data,
                        message = $"登入成功，歡迎使用者[{requestData.name}]！"
                    };

                    return Ok(responseData);
                }
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"Login Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
