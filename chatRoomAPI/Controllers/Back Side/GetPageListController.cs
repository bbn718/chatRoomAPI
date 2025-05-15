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
using System.Xml.Linq;

namespace ChatRoomAPI.Controllers.Back_Side
{
    [ApiExplorerSettings(GroupName = "back")]
    [Route("api/back/[controller]")]
    [ApiController]
    public class GetPageListController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public GetPageListController(IConfiguration configuration, ITokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpGet]
        public IActionResult GetPageList([FromQuery] string account)
        {
            string strSqlPagm = @"SELECT DISTINCT AD.S_ADMD_NAME, PM.S_PAGM_PATH, PM.S_PAGM_ID
                                     FROM ADMIN_DATA AD
                                     JOIN AUTH_GROUP AG ON AD.I_ADMD_AUTGID = AG.S_AUTG_ID
                                     JOIN PAGE_MANAGEMENT PM ON AG.I_AUTG_PAGMSYSNO = PM.I_PAGM_SYSNO
                                     JOIN COMPONENT_MANAGEMENT CM ON AG.I_AUTG_COMMSYSNO = CM.I_COMM_SYSNO
                                     WHERE S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT";
            List<GetPageList> ltData = new List<GetPageList>();

            try
            {
                if (string.IsNullOrEmpty(account))
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

                    using (SqlCommand command = new SqlCommand(strSqlPagm, connection))
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
                                errorMessage = "查無頁面管理資料，請聯繫最高管理員 Neil！"
                            };

                            return Ok(errorData);
                        }

                        foreach (DataRow row in dtbResult.Rows)
                        {
                            string pagmId = row["S_PAGM_ID"].ToString().Trim();
                            string pagmPath = row["S_PAGM_PATH"].ToString().Trim();
                            var commList = GetComponentNames(account, pagmId, connection);

                            if (commList == null || commList.Count == 0)
                            {
                                ResponseErrorMessage errorData = new ResponseErrorMessage()
                                {
                                    resultCode = "01",
                                    errorMessage = "查無對應元件資料，請聯繫最高管理員 Neil！"
                                };

                                return Ok(errorData);
                            }

                            ltData.Add(new GetPageList
                            {
                                pageName = pagmId,
                                path = pagmPath,
                                authentication = commList.ToArray()
                            });
                        }
                    }
                }

                ResponseGetPageList responseData = new ResponseGetPageList()
                {
                    resultCode = "10",
                    data = ltData,
                    message = "取得頁面管理資料成功！"
                };

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"GetPageData Error： {ex.Message}，請聯繫開發人員！"
                };

                return Conflict(errorData);
            }
        }

        private List<string> GetComponentNames(string account, string pageId, SqlConnection connection)
        {
            string strSqlGetComm = @"SELECT DISTINCT CM.S_COMM_NAME
                                     FROM ADMIN_DATA AD
                                     JOIN AUTH_GROUP AG ON AD.I_ADMD_AUTGID = AG.S_AUTG_ID
                                     JOIN PAGE_MANAGEMENT PM ON AG.I_AUTG_PAGMSYSNO = PM.I_PAGM_SYSNO
                                     JOIN COMPONENT_MANAGEMENT CM ON AG.I_AUTG_COMMSYSNO = CM.I_COMM_SYSNO
                                     WHERE AD.S_ADMD_ACCOUNT = @S_ADMD_ACCOUNT AND PM.S_PAGM_ID = @S_PAGM_ID";

            using (SqlCommand cmd = new SqlCommand(strSqlGetComm, connection))
            {
                cmd.Parameters.Add("@S_ADMD_ACCOUNT", SqlDbType.VarChar).Value = account;
                cmd.Parameters.Add("@S_PAGM_ID", SqlDbType.VarChar).Value = pageId;

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                using (DataTable dt = new DataTable())
                {
                    adapter.Fill(dt);
                    return dt.AsEnumerable()
                             .Select(r => r["S_COMM_NAME"].ToString().Trim())
                             .ToList();
                }
            }
        }
    }
}
