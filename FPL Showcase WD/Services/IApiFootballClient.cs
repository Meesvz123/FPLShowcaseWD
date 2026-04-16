using FPL_Showcase_WD.Models;

namespace FPL_Showcase_WD.Services;

public interface IApiFootballClient
{
    Task<IReadOnlyList<PlayerResponse>> GetPremierLeaguePlayersAsync(int season);
}