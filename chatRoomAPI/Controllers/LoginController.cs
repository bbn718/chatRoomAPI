﻿using chatRoomAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace chatRoomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        DateTime createDate = DateTime.Now;
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult login([FromBody] RequestLogin requestData)
        {
            try
            {
                if (requestData == null)
                {
                    ResponseErrorMessage errorMessage = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "傳入參數為空！"
                    };

                    return Ok(errorMessage);
                }

                if (requestData.loginType != 1 && requestData.loginType != 2)
                {
                    ResponseErrorMessage errorMessage = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "登入類型參數錯誤！"
                    };

                    return Ok(errorMessage);
                }

                if (requestData.loginType == 1) //使用一般登入
                {
                    if (string.IsNullOrEmpty(requestData.account))
                    {
                        ResponseErrorMessage errorMessage = new ResponseErrorMessage()
                        {
                            resultCode = "01",
                            errorMessage = "帳號參數錯誤！"
                        };

                        return Ok(errorMessage);
                    }

                    if (string.IsNullOrEmpty(requestData.password))
                    {
                        ResponseErrorMessage errorMessage = new ResponseErrorMessage()
                        {
                            resultCode = "01",
                            errorMessage = "密碼參數錯誤！"
                        };

                        return Ok(errorMessage);
                    }
                }
                else //使用第三方登入
                {
                    if (string.IsNullOrEmpty(requestData.account))
                    {
                        ResponseErrorMessage errorMessage = new ResponseErrorMessage()
                        {
                            resultCode = "01",
                            errorMessage = "ID 參數錯誤！"
                        };

                        return Ok(errorMessage);
                    }
                }

                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 資料
                string connectionString = dbs.connectionString; //取得連線字串

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (requestData.loginType == 1) //使用一般登入
                    {
                        string strSql = @"SELECT 0
                                          FROM USER_DATA
                                          WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT
                                            AND S_USED_PASSWORD = @S_USED_PASSWORD";

                        using (SqlCommand command = new SqlCommand(strSql, connection))
                        {
                            command.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;
                            command.Parameters.Add("@S_USED_PASSWORD", SqlDbType.VarChar).Value = requestData.password;

                            int result = Convert.ToInt32(command.ExecuteScalar());

                            if (result == 0)
                            {
                                ResponseErrorMessage errorMessage = new ResponseErrorMessage()
                                {
                                    resultCode = "01",
                                    errorMessage = "帳號或密碼輸入錯誤！"
                                };

                                return Ok(errorMessage);
                            }
                        }
                    }
                    else if (requestData.loginType == 2) //使用第三方登入
                    {
                        string strSql = @"SELECT 0
                                          FROM USER_DATA
                                          WHERE S_USED_ACCOUNT = @S_USED_ACCOUNT";

                        using (SqlCommand command = new SqlCommand(strSql, connection))
                        {
                            command.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;

                            object result = command.ExecuteScalar();

                            if (result == null)
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
                                    commandIrt.Parameters.Clear();
                                    commandIrt.Parameters.Add("@S_USED_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;
                                    commandIrt.Parameters.Add("@S_USED_PASSWORD", SqlDbType.VarChar).Value = string.Empty;
                                    commandIrt.Parameters.Add("@S_USED_NICKNAME", SqlDbType.VarChar).Value = requestData.name;
                                    commandIrt.Parameters.Add("@S_USED_PICTURE", SqlDbType.VarChar).Value = DBNull.Value;
                                    commandIrt.Parameters.Add("@D_USED_BIRTHDATE", SqlDbType.Date).Value = DBNull.Value;
                                    commandIrt.Parameters.Add("@I_USED_SIGNUPTYPE", SqlDbType.Int).Value = 2;
                                    commandIrt.Parameters.Add("@I_USED_STATUS", SqlDbType.Int).Value = 1;
                                    commandIrt.Parameters.Add("@D_USED_CREATEDATE", SqlDbType.DateTime).Value = createDate;

                                    int irtResult = commandIrt.ExecuteNonQuery();

                                    if (irtResult == 0)
                                    {
                                        ResponseErrorMessage errorMessage = new ResponseErrorMessage()
                                        {
                                            resultCode = "01",
                                            errorMessage = "寫入資料庫錯誤，請聯繫開發人員！"
                                        };

                                        return Ok(errorMessage);
                                    }
                                }
                            }
                        }
                    }
                }

                ResponseSuccessMessage successMessage = new ResponseSuccessMessage()
                {
                    resultCode = "10",
                    message = $"登入成功！ 歡迎使用者 {requestData.name}"
                };

                return Ok(successMessage);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorMessage = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"login API 發生錯誤： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorMessage);
            }
        }
    }
}
