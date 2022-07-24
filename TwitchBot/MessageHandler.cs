namespace TwitchBot;

public class MessageHandler
{
    private FStatsParser _statsParser;
    public MessageHandler()
    {
        _statsParser = new FStatsParser("CockTale");
        var message = _statsParser.GetFaceitElo().Result;
        Console.WriteLine(message);
    }
}