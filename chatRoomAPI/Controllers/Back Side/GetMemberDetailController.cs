using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model.Back_Side.Response;
using ChatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace ChatRoomAPI.Controllers.Back_Side
{
    [ApiExplorerSettings(GroupName = "back")]
    [Route("api/back/[controller]")]
    [ApiController]
    public class GetMemberDetailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetMemberDetailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetMemberDetail([FromQuery] string account)
        {
            string strSqlAdmd = @"SELECT S_ADMD_PICTURE, S_ADMD_NAME, S_ADMD_ACCOUNT, '1' 'loginType', I_ADMD_AUTGID, I_ADMD_STATUS FROM ADMIN_DATA WHERE S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT";

            try
            {
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料
                GetMemberDetail data = new GetMemberDetail();

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlAdmd, connection))
                    {
                        command.Parameters.Add("@S_ADMD_ACCOUNT", SqlDbType.VarChar).Value = account;

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無會員管理明細，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        DataRow dr = dtbResult.Rows[0];

                        data.picture = dr["S_ADMD_PICTURE"].ToString().Trim();
                        data.name = dr["S_ADMD_NAME"].ToString().Trim();
                        data.account = dr["S_ADMD_ACCOUNT"].ToString().Trim();
                        data.loginType = dr["loginType"].ToString().Trim() == "1" ? "一般登入" : "第三方登入";
                        data.role = dr["I_ADMD_AUTGID"].ToString().Trim();
                        data.status = dr["I_ADMD_STATUS"].ToString().Trim() == "1" ? "已啟用" : "已停用";
                    }
                }

                ResponseGetMemberDetail responseData = new ResponseGetMemberDetail()
                {
                    resultCode = "10",
                    data = data,
                    message = "取得會員管理明細成功！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetMemberDetail Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
