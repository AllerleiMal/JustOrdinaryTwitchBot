using System.Net.Sockets;
using Newtonsoft.Json;

namespace TwitchBot;

public class ChatBot
{
    private string _ip;
    private int _port;
    private string _password;
    private string _botUsername;
    private string _target;
    private StreamReader _reader;
    private StreamWriter _writer;
    private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();
    
    public event TwitchChatEventHandler OnMessage = delegate { };
    public delegate void TwitchChatEventHandler(object sender, TwitchChatMessage e);

    public class TwitchChatMessage : EventArgs
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public string Channel { get; set; }
    }

    public ChatBot()
    {
        using (StreamReader reader = new StreamReader("config.json"))
        {
            string? configData = reader.ReadToEndAsync().ToString();
            dynamic config = JsonConvert.DeserializeObject(configData);
            _ip = config.ip;
            _password = config.password;
            _botUsername = config.username;
            _port = config.port;
        }
    }

    public async Task Run()
    {
        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(_ip, _port);
        _reader = new StreamReader(tcpClient.GetStream());
        _writer = new StreamWriter(tcpClient.GetStream()) { NewLine = "\r\n", AutoFlush = true };
        await _writer.WriteLineAsync($"PASS {_password}");
        await _writer.WriteLineAsync($"NICK {_botUsername}");
        connected.SetResult(0);
        
        while (true)
        {
            string line = await _reader.ReadLineAsync();
            Console.WriteLine(line);
                
            string[] split = line.Split(" ");
            if (line.StartsWith("PING"))
            {
                await PongTwitch(split[1]);
            }

            if (split.Length > 2 && split[1] == "PRIVMSG")
            {
                int exclamationPointPosition = split[0].IndexOf("!", StringComparison.Ordinal);
                string username = split[0].Substring(1, exclamationPointPosition - 1);
                int secondColonPosition = line.IndexOf(':', 1);
                string message = line.Substring(secondColonPosition + 1);
                string channel = split[2].TrimStart('#');
                    
                OnMessage(this, new TwitchChatMessage
                {
                    Message = message,
                    Sender = username,
                    Channel = channel
                });
            }
        }
    }

    public async Task SendMessage(string message)
    {
        await connected.Task;
        await _writer.WriteLineAsync($"PRIVMSG #{_target} :{message}");
    }

    public async Task JoinChannel(string channel)
    {
        await connected.Task;
        await _writer.WriteLineAsync($"JOIN #{channel}");
        _target = channel;
    }

    private async Task PongTwitch(string message)
    {
        await _writer.WriteLineAsync($"PONG {message}");
        Console.WriteLine("Twitch ping was ponged");
    }
}