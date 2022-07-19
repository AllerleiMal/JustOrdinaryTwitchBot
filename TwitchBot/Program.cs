using System.Net.Sockets;

namespace TwitchBot
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            string ip = "irc.chat.twitch.tv";
            int port = 6667;
            string password = "oauth:on14n7g0h83u4gwinlw0oklt1kwzoe";
            string botUsername = "justordinarybot";

            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ip, port);
            var streamReader = new StreamReader(tcpClient.GetStream());
            var streamWriter = new StreamWriter(tcpClient.GetStream()) { NewLine = "\r\n", AutoFlush = true};
            //https://www.twitch.tv/ustaluj
            await streamWriter.WriteLineAsync($"PASS {password}");
            await streamWriter.WriteLineAsync($"NICK {botUsername}");
            await streamWriter.WriteLineAsync($"JOIN #ustaluj");
            await streamWriter.WriteLineAsync($"PRIVMSG #ustaluj :Hey I just started my IRC bot");

            while (true)
            {
                string line = await streamReader.ReadLineAsync();
                Console.WriteLine(line);
                
                string[] split = line.Split(" ");
                //PING :tmi.twitch.tv
                //Respond with PONG :tmi.twitch.tv
                if (line.StartsWith("PING"))
                {
                    Console.WriteLine("PING");
                    await streamWriter.WriteLineAsync($"PONG {split[1]}");
                }

                if (split.Length > 1 && split[1] == "PRIVMSG")
                {
                    //:mytwitchchannel!mytwitchchannel@mytwitchchannel.tmi.twitch.tv 
                    // ^^^^^^^^
                    //Grab this name here
                    int exclamationPointPosition = split[0].IndexOf("!");
                    string username = split[0].Substring(1, exclamationPointPosition - 1);
                    //Skip the first character, the first colon, then find the next colon
                    int secondColonPosition = line.IndexOf(':', 1);//the 1 here is what skips the first character
                    string message = line.Substring(secondColonPosition + 1);//Everything past the second colon
                    Console.WriteLine($"{username} said '{message}'");
                }
            }
        }
    }
}