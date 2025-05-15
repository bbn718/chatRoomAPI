using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model.Back_Side.Request;
using ChatRoomAPI.Model.Back_Side.Response;
using ChatRoomAPI.Model.Response;
using ChatRoomAPI.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ChatRoomAPI.Controllers.Back_Side
{
    [ApiExplorerSettings(GroupName = "back")]
    [Route("api/back/[controller]")]
    [ApiController]
    public class GetHomePageDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetHomePageDataController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetHomePageData()
        {
            string strSqlAdmd = @"SELECT S_ADMD_NAME,
                                                 S_ADMD_PHONE,
                                                 S_ADMD_EMAIL,
                                                 S_ADMD_LINKEDINURL,
                                                 S_ADMD_GITHUBURL,
                                                 S_ADMD_PICTURE
                                          FROM ADMIN_DATA
                                          WHERE  S_ADMD_ID != 'Administrator'";
            List<HomePageData> ltData = new List<HomePageData>();

            try
            {
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料

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
                                errorMessage = "查無管理成員資料，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        foreach (DataRow dr in dtbResult.Rows)
                        {
                            HomePageData data = new HomePageData()
                            {
                                name = dr["S_ADMD_NAME"].ToString().Trim(),
                                phone = dr["S_ADMD_PHONE"]?.ToString().Trim(),
                                email = dr["S_ADMD_EMAIL"]?.ToString().Trim(),
                                linkedInURL = dr["S_ADMD_LINKEDINURL"]?.ToString().Trim(),
                                gitHubURL = dr["S_ADMD_GITHUBURL"]?.ToString().Trim(),
                                picture = dr["S_ADMD_PICTURE"]?.ToString().Trim()
                            };

                            ltData.Add(data);
                        }
                    }
                }

                ResponseHomePageData responseData = new ResponseHomePageData()
                {
                    resultCode = "10",
                    data = ltData,
                    message = "取得管理成員資料成功！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetHomePageData Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
