using chatRoomAPI.Configuration;
using chatRoomAPI.Model.Back_Side.Request;
using chatRoomAPI.Model.Back_Side.Response;
using chatRoomAPI.Model.Response;
using chatRoomAPI.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace chatRoomAPI.Controllers.Back_Side
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetAuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public GetAuthenticationController(IConfiguration configuration, ITokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost, AllowAnonymous]
        public IActionResult GetAuthentication([FromBody] RequestGetAuthentication requestData)
        {
            string strSqlGetAutg = @"SELECT AD.S_ADMD_NAME, PM.S_PAGM_NAME, CM.S_COMM_NAME
                                     FROM ADMIN_DATA AD
                                     JOIN AUTH_GROUP AG ON I_ADMD_AUTGID = S_AUTG_ID
                                     JOIN PAGE_MANAGEMENT PM ON I_AUTG_PAGMSYSNO = I_PAGM_SYSNO
                                     JOIN COMPONENT_MANAGEMENT CM ON I_AUTG_COMMSYSNO = I_COMM_SYSNO
                                     WHERE S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT
                                       AND S_PAGM_NAME = @S_PAGM_NAME";
            List<string> authList = new List<string>();
            ResponseGetAuthentication responseData = new ResponseGetAuthentication();

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

                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlGetAutg, connection))
                    {
                        command.Parameters.Add("@S_ADMD_ACCOUNT", SqlDbType.VarChar).Value = requestData.account;
                        command.Parameters.Add("@S_PAGM_NAME", SqlDbType.VarChar).Value = requestData.pageName;

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無權限資料，請聯繫最高管理員 Neil ！"
                            };

                            return Ok(errorData);
                        }

                        foreach (DataRow row in dtbResult.Rows)
                        {
                            string auth = row["S_COMM_NAME"].ToString().Trim();
                            authList.Add(auth);
                        }

                        responseData.pageName = requestData.pageName;
                        responseData.authentication = authList.ToArray();
                    }
                }

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetAuthentication Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
