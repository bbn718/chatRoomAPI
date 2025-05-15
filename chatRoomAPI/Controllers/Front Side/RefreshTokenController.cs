using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model;
using ChatRoomAPI.Model.Response;
using ChatRoomAPI.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatRoomAPI.Controllers
{
    [ApiExplorerSettings(GroupName = "front")]
    [Route("api/front/[controller]")]
    [ApiController]
    public class RefreshTokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public RefreshTokenController(ITokenService tokenService, IConfiguration configuration)
        {
            _tokenService = tokenService;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult RefreshToken([FromBody] RequestRefreshToken requestData)
        {
            try
            {
                string account = requestData.account;
                string ExpiredToken = requestData.token;
                string refreshToken = requestData.refreshToken;
                var principal = _tokenService.GetPrincipalFromExpiredToken(ExpiredToken);
                string username = principal.Identity?.Name ?? "";
                string newJwt = _tokenService.GenerateJwt(principal.Claims);
                string newRefeshToken = _tokenService.GenerateRefreshToken();
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 資料
                string connectionString = dbs.connectionString; //取得連線字串

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    #region 檢查使用者 refreshToken 是否匹配、refreshTokenExpiryDate 是否過期
                    string strSqlChk = @"SELECT S_used_refreshToken, D_used_refreshTokenExpiryDate FROM user_data WHERE S_used_account = @S_used_account";

                    using (SqlCommand command = new SqlCommand(strSqlChk, connection))
                    {
                        command.Parameters.Add("S_used_account", SqlDbType.VarChar).Value = account;

                        SqlDataAdapter sdaResult = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();

                        sdaResult.Fill(dtbResult);

                        if (dtbResult.Rows.Count <= 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無使用者資料，不允許重新獲取 jwt！"
                            };

                            return Ok(errorData);
                        }

                        string DbRefreshToken = dtbResult.Rows[0]["S_used_refreshToken"].ToString();

                        if (!DateTime.TryParse(dtbResult.Rows[0]["D_used_refreshTokenExpiryDate"].ToString(), out DateTime DbRefreshTokenExpiryDate))
                            throw new Exception("DateTime 轉換錯誤，請聯繫開發人員！");

                        if (refreshToken != DbRefreshToken)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "RefreshToken 不匹配，不允許重新獲取 jwt！"
                            };

                            return Ok(errorData);
                        }

                        if (DateTime.Now > DbRefreshTokenExpiryDate)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "RefreshTokenExpiryDate 已過期，不允許重新獲取 jwt，請重新登入！"
                            };

                            return Ok(errorData);
                        }
                    }
                    #endregion

                    #region 更新使用者 refreshToken
                    string strSqlUpd = @"UPDATE user_data SET S_used_refreshToken = @S_used_refreshToken WHERE S_used_account = @S_used_account";

                    using (SqlCommand command = new SqlCommand(strSqlUpd, connection))
                    {
                        command.Parameters.Add("@S_used_refreshToken", SqlDbType.VarChar).Value = newRefeshToken;
                        command.Parameters.Add("@S_used_account", SqlDbType.VarChar).Value = account;

                        int result = Convert.ToInt32(command.ExecuteNonQuery());

                        if (result <= 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "更新使用者 refreshToken 失敗，請聯繫開發人員！"
                            };

                            return Ok(errorData);
                        }
                    }
                }
                #endregion

                ResponseRefreshTokenData data = new ResponseRefreshTokenData()
                {
                    newJwt = newJwt,
                    newRefreshToken = newRefeshToken
                };

                ResponseRefreshToken responseData = new ResponseRefreshToken
                {
                    resultCode = "10",
                    data = data
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"RefreshToken Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }

    
}
