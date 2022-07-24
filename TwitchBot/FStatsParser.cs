using AngleSharp;

namespace TwitchBot;

public class FStatsParser
{
    private string _profileUri;
    private static IConfiguration _config = Configuration.Default.WithDefaultLoader();
    

    public FStatsParser(string nickname)
    {
        _profileUri = "https://faceitstats.com/player/" + nickname;
    }
    
    public async Task<string> GetFaceitElo()
    {
        var document = await BrowsingContext.New(_config).OpenAsync(_profileUri);
        var selector = "div.col-6.col-sm-6.col-md-4.col-lg-2 h5";
        var cells = document.QuerySelectorAll(selector);
        var eloAmount = int.Parse(cells[0].FirstChild.NodeValue);
        int SECOND_LEVEL_ELO = 801;
        int ELO_PER_LEVEL = 150;
        // 1 level granted for everyone with 1-800 elo, 1 level is added due to ranges(to avoid using Math.Round) --> +2
        int skillLevel;
        if (eloAmount < SECOND_LEVEL_ELO)
        {
            skillLevel = 1;
        }
        else
        {
            skillLevel = 2 + (eloAmount - SECOND_LEVEL_ELO) / ELO_PER_LEVEL;
        }
        return $"Skill level: {skillLevel}; Elo: {eloAmount}";
    }

    public string GetFaceitStats()
    {
        return "";
    }
    
}