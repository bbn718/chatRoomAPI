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
    public class GetBulletinsListController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetBulletinsListController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetBulletinsList()
        {
            string strSql = @"SELECT S_BULD_TITLE, S_BULD_SUMMARY, I_BULD_STATUS FROM BULLETIN_DATA";

            try
            {
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料
                List<GetBulletinsList> ltData = new List<GetBulletinsList>();

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSql, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無最新消息資料，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        foreach (DataRow row in dtbResult.Rows)
                        {
                            GetBulletinsList data = new GetBulletinsList()
                            {
                                title = row["S_BULD_TITLE"].ToString().Trim(),
                                summary = row["S_BULD_SUMMARY"].ToString().Trim(),
                                status = row["I_BULD_STATUS"].ToString().Trim() == "1" ? "已啟用" : "已停用"
                            };

                            ltData.Add(data);
                        }
                    }
                }

                ResponseGetBulletinsList responseData = new ResponseGetBulletinsList()
                {
                    resultCode = "10",
                    data = ltData,
                    message = "取得最新消息資料成功！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetBulletinsList Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
