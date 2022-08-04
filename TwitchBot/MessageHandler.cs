namespace TwitchBot;

public class MessageHandler
{
    private readonly FStatsParser _statsParser;
    private string Message { get; set; }
    private string Sender { get; set; }
    private string Channel { get; set; }
    private bool TwitchPingFlag { get; set; }

    public MessageHandler(string nickname)
    {
        _statsParser = new FStatsParser(nickname);
        Message = null;
        Sender = null;
        Channel = null;
    }

    public string GetEloCommandAnswer()
    {
        return _statsParser.GetFaceitElo().Result;
    }

    private void ProcessMessage(string ircMessage)
    {
        if (ircMessage == null)
        {
            throw new ArgumentException("IRC message cannot be null");
        }
        string[] split = ircMessage.Split(" ");
        if (ircMessage.StartsWith("PING"))
        {
            TwitchPingFlag = true;
            Message = split[1];
            return;
        }

        if (split.Length > 2 && split[1] == "PRIVMSG")
        {
            var exclamationPointPosition = split[0].IndexOf("!", StringComparison.Ordinal);
            Sender = split[0].Substring(1, exclamationPointPosition - 1);
            //Skip the first character, the first colon, then find the next colon
            var secondColonPosition = ircMessage.IndexOf(':', 1);  //the 1 here is what skips the first character
            Message = ircMessage[(secondColonPosition + 1)..];  //Everything past the second colon
            Channel = split[2].TrimStart('#');
        }
    }

    public Tuple<MessageStatus, string> GetAnswer(string ircMessage)
    {
        ProcessMessage(ircMessage);
        
        if (TwitchPingFlag)
        {
            return new Tuple<MessageStatus, string>(MessageStatus.TwitchPing, Message);
        }

        if (Message.StartsWith("!"))
        {
            if (Message.StartsWith("!exit"))
            {
                return new Tuple<MessageStatus, string>(MessageStatus.EmergencyExit, "");
            }
        }
        
        return new Tuple<MessageStatus, string>(MessageStatus.IgnoreMessage, "");
    }
}