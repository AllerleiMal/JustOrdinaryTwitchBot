using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;

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
    private MessageHandler _messageHandler;
    private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();
    
    
    public string GiveawayStatus { get; set; }
    
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
        // using (StreamReader reader = new StreamReader("config.json"))
        // {
        //     _messageHandler = new MessageHandler();
        //     var configData = reader.ReadToEndAsync().ToString();
        //     Console.WriteLine(configData);
        //     dynamic config = JsonConvert.DeserializeObject(configData);
        //     _ip = config.ip;
        //     _password = config.password;
        //     _botUsername = config.username;
        //     _port = config.port;
        //     GiveawayStatus = "Сегодня без розыгрыша :(";
        // }
        //
        _messageHandler = new MessageHandler();
        _ip = "irc.chat.twitch.tv";
        _password = "oauth:e3l1k290yhbv37o7ddtyvioemaofdp";
        _botUsername = "justordinarybot";
        _port = 6667;
        GiveawayStatus = "Сегодня без розыгрыша >(";
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
                int exclamationPointPosition = split[0].IndexOf("!");
                string username = split[0].Substring(1, exclamationPointPosition - 1);
                //Skip the first character, the first colon, then find the next colon
                int secondColonPosition = line.IndexOf(':', 1);//the 1 here is what skips the first character
                string message = line.Substring(secondColonPosition + 1);//Everything past the second colon
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
        Console.WriteLine("Chat bot connected");
    }

    private async Task PongTwitch(string message)
    {
        await _writer.WriteLineAsync($"PONG {message}");
        Console.WriteLine("Twitch ping was ponged");
    }
}