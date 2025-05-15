using ChatRoomAPI.Configuration;
using ChatRoomAPI.Model.Request;
using ChatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace ChatRoomAPI.Controllers
{
    [ApiExplorerSettings(GroupName = "front")]
    [Route("api/front/[controller]")]
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
            string strSqlChar = @"SELECT 0
                                  FROM CHAT_ROOM
                                  WHERE S_CHAR_CODE = @S_CHAR_CODE";
            string strSqlChai = @"SELECT S_CHAI_CHARNAME,
                                         S_CHAI_USEDNICKNAME,
                                         S_CHAI_USEDPICTURE,
                                         S_CHAI_HISTMESSAGE,
                                         D_CHAI_TIMESTAMP
                                  FROM CHATROOM_ITEM
                                  JOIN CHAT_ROOM ON S_CHAI_CHARCODE = S_CHAR_CODE
                                  WHERE S_CHAI_CHARCODE = @S_CHAI_CHARCODE";
            string strSqlWhere = "";
            string strSqlOrderBy = " ORDER BY D_CHAI_TIMESTAMP DESC";
            string strSqlLimit = @" OFFSET (@page - 1) * @limit ROWS
                                    FETCH NEXT @limit ROWS ONLY";
            List<SqlParameter> parameters = new List<SqlParameter>();
            DataTable dtbChai = new DataTable();

            ResponseGetChatRoomInfo result = new ResponseGetChatRoomInfo()
            {
                resultCode = "10",
            };
            ResponseGetChatRoomInfoData data = new ResponseGetChatRoomInfoData()
            {
                chatRoomCode = chatRoomCode
            };

            ResponseErrorMessage errorData = new ResponseErrorMessage()
            {
                resultCode = "01",
            };

            try
            {
                //page為必填且應該為正整數
                if (page is null)
                {
                    errorData.errorMessage = "URL - page 參數應為必填！";

                    return Ok(errorData);
                }
                else if (page < 1)
                {
                    errorData.errorMessage = "URL - page 參數應為正整數！";

                    return Ok(errorData);
                }

                //limit為必填
                if (limit is null)
                {
                    errorData.errorMessage = "URL - limit 參數應為必填！";

                    return Ok(errorData);
                }
                else if (limit < 1)
                {
                    errorData.errorMessage = "URL - limit 參數應為正整數！";

                    return Ok(errorData);
                }

                parameters.Add(new SqlParameter("@S_CHAI_CHARCODE", SqlDbType.VarChar) { Value = chatRoomCode });
                parameters.Add(new SqlParameter("@page", SqlDbType.VarChar) { Value = page });
                parameters.Add(new SqlParameter("@limit", SqlDbType.VarChar) { Value = limit });

                if (keyWord is not null)
                {
                    strSqlWhere += " AND S_CHAI_HISTMESSAGE LIKE @S_CHAI_HISTMESSAGE";
                    parameters.Add(new SqlParameter("@S_CHAI_HISTMESSAGE", SqlDbType.VarChar) { Value = "%" + keyWord.Trim() + "%" });
                }

                using (SqlConnection connection = new SqlConnection(new DatabaseSettings(_configuration).connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(strSqlChar, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@S_CHAR_CODE", SqlDbType.VarChar) { Value = chatRoomCode });

                        object charCode = command.ExecuteScalar();

                        if (charCode is null)
                        {
                            result.message = "該聊天室已不存在！";

                            return Ok(result);
                        }
                    }

                    using (SqlCommand command = new SqlCommand(strSqlChai, connection))
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dtbChai);
                    }
                }

                if (dtbChai.Rows.Count == 0)
                {
                    result.message = "該聊天室無歷史訊息！";

                    return Ok(result);
                }

                data.chatRoomName = dtbChai.Rows[0]["S_CHAI_CHARNAME"].ToString().Trim();

                foreach (DataRow dr in dtbChai.Rows)
                {
                    chatRoomUsersHistoryData historyData = new chatRoomUsersHistoryData()
                    {
                        userNickName = dr["S_CHAI_USEDNICKNAME"].ToString().Trim(),
                        userPic = dr["S_CHAI_USEDPICTURE"].ToString().Trim(),
                        historyMessage = dr["S_CHAI_HISTMESSAGE"].ToString().Trim(),
                        timestamp = Convert.ToDateTime(dr["D_CHAI_TIMESTAMP"])
                    };

                    data.historyData.Add(historyData);
                }

                result.data = data;
                result.message = "取得聊天室資料成功！";

                return Ok(result);
            }
            catch (Exception ex)
            {
                errorData.errorMessage = $"GetChatRoomInfo Error： {ex.Message}，請聯繫開發人員！";

                return Conflict(errorData);
            }
        }
    }
}
