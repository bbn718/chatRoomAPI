using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model.Back_Side.Response;
using ChatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChatRoomAPI.Controllers.Back_Side
{
    [ApiExplorerSettings(GroupName = "back")]
    [Route("api/back/[controller]")]
    [ApiController]
    public class GetBulletinsDetailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetBulletinsDetailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetBulletinsDetail([FromQuery] string title)
        {
            string strSql = @"SELECT S_BULD_TITLE, S_BULD_SUMMARY, S_BULD_CONTENT, I_BULD_STATUS FROM BULLETIN_DATA WHERE S_BULD_TITLE = @S_BULD_TITLE";
            GetBulletinsDetail data = new GetBulletinsDetail();

            try
            {
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSql, connection))
                    {
                        command.Parameters.Add("@S_BULD_TITLE", SqlDbType.VarChar).Value = title;

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無最新消息明細，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        data.title = dtbResult.Rows[0]["S_BULD_TITLE"].ToString().Trim();
                        data.summary = dtbResult.Rows[0]["S_BULD_SUMMARY"].ToString().Trim();
                        data.content = dtbResult.Rows[0]["S_BULD_CONTENT"].ToString().Trim();
                        data.status = dtbResult.Rows[0]["I_BULD_STATUS"].ToString().Trim();
                    }
                }

                ResponseGetBulletinsDetail responseData = new ResponseGetBulletinsDetail()
                {
                    resultCode = "10",
                    data = data,
                    message = "取得最新消息資料成功！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetBulletinsDetail Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
