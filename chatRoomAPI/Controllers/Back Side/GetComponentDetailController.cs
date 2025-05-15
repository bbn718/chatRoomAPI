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
    public class GetComponentDetailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetComponentDetailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetComponentDetail([FromQuery] string domId)
        {
            string strSqlComm = @"SELECT S_COMM_ID, S_COMM_NAME, S_COMM_MEMO FROM COMPONENT_MANAGEMENT WHERE S_COMM_ID = @S_COMM_ID";

            try
            {
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料
                GetComponentDetail data = new GetComponentDetail();

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlComm, connection))
                    {
                        command.Parameters.Add("@S_COMM_ID", SqlDbType.VarChar).Value = domId;

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無元件管理明細，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        data.componentName = dtbResult.Rows[0]["S_COMM_NAME"].ToString().Trim();
                        data.domId = dtbResult.Rows[0]["S_COMM_ID"].ToString().Trim();
                        data.memo = dtbResult.Rows[0]["S_COMM_MEMO"].ToString().Trim();
                    }
                }

                ResponseGetComponentDetail responseData = new ResponseGetComponentDetail()
                {
                    resultCode = "10",
                    data = data,
                    message = "取得元件管理明細成功！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetComponentDetail Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
