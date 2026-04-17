using System.Net;
using System.Net.Http.Json;
using FPL_Showcase_WD.Models;
using Microsoft.Extensions.Logging;

namespace FPL_Showcase_WD.Services;

public sealed class ApiFootballClient(HttpClient httpClient, ILogger<ApiFootballClient> logger) : IApiFootballClient
{
    public async Task<IReadOnlyList<PlayerResponse>> GetPremierLeaguePlayersAsync(int season)
    {
        var teamIds = await GetPremierLeagueTeamIdsAsync(season);
        var allPlayers = new List<PlayerResponse>();
        var seen = new HashSet<int>();

        foreach (var teamId in teamIds)
        {
            var page = 1;
            var maxPages = 50;

            while (page <= maxPages)
            {
                var url = $"players?league=39&season={season}&team={teamId}&page={page}";
                var result = await GetApiAsync<PlayerResponse>(url);

                if (result?.Response is null || result.Response.Count == 0)
                {
                    break;
                }

                foreach (var player in result.Response)
                {
                    if (seen.Add(player.Player.Id))
                    {
                        allPlayers.Add(player);
                    }
                }

                if (result.Paging?.Total is > 0)
                {
                    maxPages = result.Paging.Total;
                }

                logger.LogInformation("Team {TeamId} page {Page}/{MaxPages}: total {Total}",
                    teamId, page, maxPages, allPlayers.Count);

                page++;
                await Task.Delay(1100);
            }
        }

        return allPlayers;
    }

    private async Task<IReadOnlyList<int>> GetPremierLeagueTeamIdsAsync(int season)
    {
        var url = $"teams?league=39&season={season}";
        var result = await GetApiAsync<TeamResponse>(url);

        return result?.Response
            .Select(r => r.Team.Id)
            .Distinct()
            .ToList() ?? [];
    }

    private async Task<ApiFootballResponse<T>?> GetApiAsync<T>(string url)
    {
                while (true)
        {
            var response = await httpClient.GetAsync(url);

            if (response.StatusCode == (HttpStatusCode)429)
            {
                await Task.Delay(1500);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("API failed: {Status} for {Url}", response.StatusCode, url);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ApiFootballResponse<T>>();
        }
    }
}