using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model.Back_Side.Response;
using ChatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace ChatRoomAPI.Controllers.Back_Side
{
    [ApiExplorerSettings(GroupName = "back")]
    [Route("api/back/[controller]")]
    [ApiController]
    public class GetComponentListController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetComponentListController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetComponentList()
        {
            string strSqlComm = @"SELECT S_COMM_ID, S_COMM_NAME FROM COMPONENT_MANAGEMENT";

            try
            {
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料
                List<GetComponentList> ltData = new List<GetComponentList>();

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlComm, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無元件管理資料，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        foreach (DataRow dr in dtbResult.Rows)
                        {
                            GetComponentList data = new GetComponentList()
                            {
                                domId = dr["S_COMM_ID"].ToString().Trim(),
                                componentName = dr["S_COMM_NAME"].ToString().Trim()
                            };

                            ltData.Add(data);
                        }
                    }
                }

                ResponseGetComponentList responseData = new ResponseGetComponentList()
                {
                    resultCode = "10",
                    data = ltData,
                    message = "取得元件管理資料成功！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetComponentList Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
