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
    public class GetMemberListController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetMemberListController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetMemberList()
        {
            string strSqlAdmd = @"SELECT S_ADMD_NAME, S_ADMD_ACCOUNT, I_ADMD_STATUS FROM ADMIN_DATA";

            try
            {
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料
                List<GetMemberList> ltData = new List<GetMemberList>();

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlAdmd, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無會員管理資料，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        foreach (DataRow dr in dtbResult.Rows)
                        {
                            GetMemberList data = new GetMemberList()
                            {
                                name = dr["S_ADMD_NAME"].ToString().Trim(),
                                account = dr["S_ADMD_ACCOUNT"].ToString().Trim(),
                                status = dr["I_ADMD_STATUS"].ToString().Trim() == "1" ? "已啟用" : "已停用"
                            };

                            ltData.Add(data);
                        }
                    }
                }

                ResponseGetMemberList responseData = new ResponseGetMemberList()
                {
                    resultCode = "10",
                    data = ltData,
                    message = "取得會員管理資料成功！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetAdminMemberData Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
