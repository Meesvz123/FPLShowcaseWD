using System.Text.Json.Serialization;

namespace FPL_Showcase_WD.Models;

public sealed record ApiFootballResponse<T>(
    [property: JsonPropertyName("response")] List<T> Response,
    [property: JsonPropertyName("paging")] PagingInfo? Paging);

public sealed record PagingInfo(
    [property: JsonPropertyName("current")] int Current,
    [property: JsonPropertyName("total")] int Total);

public sealed record PlayerResponse(
    [property: JsonPropertyName("player")] PlayerInfo Player,
    [property: JsonPropertyName("statistics")] List<PlayerStatistics> Statistics);

public sealed record PlayerInfo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("age")] int Age,
    [property: JsonPropertyName("nationality")] string Nationality,
    [property: JsonPropertyName("photo")] string Photo);

public sealed record PlayerStatistics(
    [property: JsonPropertyName("team")] TeamInfo Team,
    [property: JsonPropertyName("games")] GamesInfo Games);

public sealed record TeamInfo(
    [property: JsonPropertyName("name")] string Name);

public sealed record GamesInfo(
    [property: JsonPropertyName("position")] string Position);

public sealed record TeamResponse(
    [property: JsonPropertyName("team")] TeamSummary Team);

public sealed record TeamSummary(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name);