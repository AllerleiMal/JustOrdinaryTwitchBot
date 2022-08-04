using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Xml.Serialization;
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
        _messageHandler = new MessageHandler("CockTale");
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
            var answer = _messageHandler.GetAnswer(await _reader.ReadLineAsync());
            switch (answer.Item1)
            {
                case MessageStatus.CommandMessage:
                    await SendMessage(answer.Item2);
                    break;
                case MessageStatus.EmergencyExit:
                    return;
                case MessageStatus.IgnoreMessage:
                    continue;
                case MessageStatus.TwitchPing:
                    await PongTwitch(answer.Item2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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