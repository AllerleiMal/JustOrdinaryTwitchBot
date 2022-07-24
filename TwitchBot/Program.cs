using System.Net.Sockets;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;

namespace TwitchBot
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            //TODO: bot don't write to chat
            var chatBot = new ChatBot();
            chatBot.Run().SafeFireAndForget();
            await chatBot.JoinChannel("ustaluj");
            await chatBot.SendMessage("Hey my bot has started up");
            await Task.Delay(-1);
            
            chatBot.OnMessage += async (sender, twitchChatMessage) =>
            {
                Console.WriteLine($"{twitchChatMessage.Sender} said '{twitchChatMessage.Message}'");
                //Listen for !hey command
                if (twitchChatMessage.Message.StartsWith("!hey"))
                {
                    await chatBot.SendMessage($"Hey there {twitchChatMessage.Sender}");
                }
            };
            
            await Task.Delay(-1);
        }
    }
}