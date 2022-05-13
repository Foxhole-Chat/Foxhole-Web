let socket = new WebSocket("wss://127.0.0.1:8443/msg_ws");

socket.onmessage = function (event)
{
	alert(`[message] Data received from server: ${event.data}`);
};