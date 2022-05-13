const Get_Messages = async () => {
	const response = await fetch("https://127.0.0.1:8443/Channel/0/Messages/");
	const messages = await response.json(); //extract JSON from the http response

	let message_container = document.getElementById("message_container");

	for (let i = 0; i < messages.length; ++i)
	{
		message_container.innerHTML += messages[i].content + "<br/>";
	}
}
Get_Messages();