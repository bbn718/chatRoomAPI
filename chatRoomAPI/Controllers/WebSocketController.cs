using chatRoomAPI.Model.Request;
using chatRoomAPI.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;

namespace chatRoomAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private static ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();
        private static ConcurrentDictionary<string, List<string>> _rooms = new ConcurrentDictionary<string, List<string>>();

        [HttpGet("{chatRoomCode}")]
        public async Task Get(string chatRoomCode, [FromQuery]string account)
        {
            try
            {
                if (!HttpContext.WebSockets.IsWebSocketRequest)
                {
                    ResponseErrorMessage errorData = new ResponseErrorMessage()
                    {
                        resultCode = "01",
                        errorMessage = "非 WebSocket 請求！"
                    };

                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    await Response.WriteAsJsonAsync(errorData);
                    return;
                }

                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                _clients.TryAdd(account, webSocket);

                // 加入指定房間
                _rooms.AddOrUpdate(chatRoomCode, new List<string> { account }, (key, existingList) =>
                {
                    existingList.Add(account);
                    return existingList;
                });

                // 開始處理 WebSocket 連接
                await HandleWebSocketConnection(webSocket, account, chatRoomCode);
            }
            catch (Exception ex)
            {
                ResponseErrorMessage errorData = new ResponseErrorMessage()
                {
                    resultCode = "01",
                    errorMessage = $"WebSocket Error： {ex.Message}，請聯繫開發人員！"
                };

                Response.StatusCode = StatusCodes.Status409Conflict;

                await Response.WriteAsJsonAsync(errorData);
            }
        }

        /// <summary>
        /// WebSocket 連線後邏輯實現
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="account"></param>
        /// <param name="chatRoomCode"></param>
        /// <returns></returns>
        private async Task HandleWebSocketConnection(WebSocket webSocket, string account, string chatRoomCode)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            try
            {
                while (true)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var jsonMessage = JsonConvert.DeserializeObject<RequestWebSocket>(message);

                    if (jsonMessage == null)
                        continue;

                    // 發送到指定房間
                    await SendMessageToRoom(jsonMessage);
                }
            }
            finally
            {
                _clients.TryRemove(account, out _);

                if (_rooms.TryGetValue(chatRoomCode, out var users))
                {
                    users.Remove(account);
                    if (users.Count == 0)
                    {
                        _rooms.TryRemove(chatRoomCode, out _);
                    }
                }
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        /// <summary>
        /// 發送訊息給前端
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="jsonMessage"></param>
        /// <param name="senderId"></param>
        /// <returns></returns>
        private async Task SendMessageToRoom(RequestWebSocket jsonMessage)
        {
            if (_rooms.TryGetValue(jsonMessage.chatRoomCode, out var users))
            {
                ResponseWebSocket response = new ResponseWebSocket
                {
                    chatRoomCode = jsonMessage.chatRoomCode,
                    senderId = jsonMessage.account,
                    message = jsonMessage.message
                };

                var jsonResponse = JsonConvert.SerializeObject(response);
                var bytesToSend = Encoding.UTF8.GetBytes(jsonResponse);

                foreach (var user in users)
                {
                    if (_clients.TryGetValue(user, out var socket))
                    {
                        await socket.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }

    }
}
