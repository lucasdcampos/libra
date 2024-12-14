using Microsoft.AspNetCore.SignalR;
using Libra;

public class WebLogger : Libra.ILogger
{

    // TODO: IMPLEMENT THIS!
    public WebLogger(object dummy)
    {
    }

    public async void Msg(string mensagem, string final = "\n")
    {

    }
}

public class LoggerHub : Hub
{
}