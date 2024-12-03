using chatRoomAPI.Configuration;
using chatRoomAPI.Model.Request;
using chatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;

namespace chatRoomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetChatRoomInfoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GetChatRoomInfoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{chatRoomCode}")]
        public IActionResult GetChatRoomInfo(string chatRoomCode, [FromQuery]int? page, int? limit, string? keyWord, bool? active)
        {
            string strSql = @"SELECT
                              FROM";
            string strSqlWhere = @" WHERE 1 = 1";
            string strOrderBy = @" ORDER BY  DESC";

            try
            {
                //page為必填
                if (page is null)
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "URL - page 參數錯誤！"
                    };

                    return Ok(errorData);
                }

                DatabaseSettings dbs = new DatabaseSettings(_configuration);
                string connectionString = dbs.connectionString;



                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                }




                return Ok();
            }
            catch (Exception ex)
            {
                return Conflict();
            }
        }
    }
}
