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
    public class GetAdminGroupDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public GetAdminGroupDataController(IConfiguration configuration, ITokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult GetAdminGroupData()
        {
            string strSqlGetAdminData = @"SELECT S_ADMD_NAME,
                                                 S_ADMD_PHONE,
                                                 S_ADMD_EMAIL,
                                                 S_ADMD_LINKEDINURL,
                                                 S_ADMD_GITHUBURL
                                          FROM ADMIN_DATA
                                          WHERE  S_ADMD_ID != 'Administrator'";
            List<ResponseAdminGroupDataItem> ltData = new List<ResponseAdminGroupDataItem>();

            try
            {
                DatabaseSettings dbs = new DatabaseSettings(_configuration); //取得 appsetting.json 連線資料

                using (SqlConnection connection = new SqlConnection(dbs.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlGetAdminData, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dtbResult = new DataTable();
                        adapter.Fill(dtbResult);

                        if (dtbResult.Rows.Count == 0)
                        {
                            ResponseErrorMessage errorData = new ResponseErrorMessage()
                            {
                                resultCode = "01",
                                errorMessage = "查無開發團隊資料，請回報最高管理員 Neil ！"
                            };

                            return Ok(errorData);
                        }

                        foreach (DataRow dr in dtbResult.Rows)
                        {
                            ResponseAdminGroupDataItem data = new ResponseAdminGroupDataItem()
                            {
                                name = dr["S_ADMD_NAME"].ToString().Trim(),
                                phone = dr["S_ADMD_PHONE"]?.ToString().Trim(),
                                email = dr["S_ADMD_EMAIL"]?.ToString().Trim(),
                                linkedinURL = dr["S_ADMD_LINKEDINURL"]?.ToString().Trim(),
                                githubURL = dr["S_ADMD_GITHUBURL"]?.ToString().Trim()
                            };

                            ltData.Add(data);
                        }
                    }
                }

                ResponseAdminGroupData responseData = new ResponseAdminGroupData()
                {
                    resultCode = "10",
                    data = ltData
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetAdminGroupData Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }
    }
}
