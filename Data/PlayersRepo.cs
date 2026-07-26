using AgilitySportsAPI.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace AgilitySportsAPI.Data;

public class PlayersRepo : BaseRepo, IPlayersRepo
{
    public PlayersRepo(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IEnumerable<PlayerSummaryDto>?> GetPlayers(
        ILogger<PlayerSummaryDto> logger,
        string? sportCode,
        string? teamCode,
        string? positionCode,
        string? search)
    {
        logger.LogInformation("Fetching V2 players.");

        try
        {
            var sql = @"
                    select
                        p.playerID as PlayerId
                        ,p.sportCode as SportCode
                        ,p.teamCode as TeamCode
                        ,t.teamShortName as TeamName
                        ,p.positionCode as PositionCode
                        ,pc.positionDesc as PositionDesc
                        ,p.firstName as FirstName
                        ,p.lastName as LastName
                        ,p.number as JerseyNumber
                        ,p.seasonYear as SeasonYear
                    from core.Players p
                    left join core.Teams t
                        on t.sportCode = p.sportCode
                        and t.teamCode = p.teamCode
                    left join reference.PositionCodes pc
                        on pc.sportCode = p.sportCode
                        and pc.positionCode = p.positionCode
                    where (@sportCode is null or p.sportCode = @sportCode)
                        and (@teamCode is null or p.teamCode = @teamCode)
                        and (@positionCode is null or p.positionCode = @positionCode)
                        and (@search is null or p.firstName like @searchLike or p.lastName like @searchLike)
                    order by p.lastName, p.firstName, p.playerID;";

            using var connection = new SqlConnection(base.connectionString);
            await base.GenToken(connection);

            var trimmedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var trimmedSportCode = string.IsNullOrWhiteSpace(sportCode) ? null : sportCode.Trim();
            var trimmedTeamCode = string.IsNullOrWhiteSpace(teamCode) ? null : teamCode.Trim();
            var trimmedPositionCode = string.IsNullOrWhiteSpace(positionCode) ? null : positionCode.Trim();

            return await connection.QueryAsync<PlayerSummaryDto>(sql, new
            {
                sportCode = trimmedSportCode,
                teamCode = trimmedTeamCode,
                positionCode = trimmedPositionCode,
                search = trimmedSearch,
                searchLike = trimmedSearch == null ? null : $"%{trimmedSearch}%"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching V2 players.");
            return null;
        }
    }

    public async Task<PlayerSummaryDto?> GetPlayerById(ILogger<PlayerSummaryDto> logger, int playerId)
    {
        logger.LogInformation("Fetching V2 player by ID {PlayerId}.", playerId);
        try
        {
            var sql = @"
                    select
                        p.playerID as PlayerId
                        ,p.sportCode as SportCode
                        ,p.teamCode as TeamCode
                        ,t.teamShortName as TeamName
                        ,p.positionCode as PositionCode
                        ,pc.positionDesc as PositionDesc
                        ,p.firstName as FirstName
                        ,p.lastName as LastName
                        ,p.number as JerseyNumber
                        ,p.seasonYear as SeasonYear
                    from core.Players p
                    left join core.Teams t
                        on t.sportCode = p.sportCode
                        and t.teamCode = p.teamCode
                    left join reference.PositionCodes pc
                        on pc.sportCode = p.sportCode
                        and pc.positionCode = p.positionCode
                    where p.playerID = @playerId;";

            using var connection = new SqlConnection(base.connectionString);
            await base.GenToken(connection);
            return await connection.QuerySingleOrDefaultAsync<PlayerSummaryDto>(sql, new { playerId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching V2 player by ID.");
            return null;
        }
    }

    public async Task<PlayerStatsDto?> GetPlayerStats(ILogger<PlayerSummaryDto> logger, int playerId)
    {
        logger.LogInformation("Fetching V2 player stats for PlayerId {PlayerId}.", playerId);
        try
        {
            var sql = @"
                    select
                        p.playerID as PlayerId
                        ,p.sportCode as SportCode
                        ,mlb.bats as Bats
                        ,mlb.throws as Throws
                        ,mlb.battingAverage as BattingAverage
                        ,mlb.homeRuns as HomeRuns
                        ,mlb.era as Era
                        ,nba.pointsPerGame as PointsPerGame
                        ,nba.reboundsPerGame as ReboundsPerGame
                        ,nba.assistsPerGame as AssistsPerGame
                        ,nfl.sacks as Sacks
                        ,nfl.touchdowns as Touchdowns
                        ,nhl.handed as Handed
                        ,nhl.goals as Goals
                        ,nhl.penaltyMinutes as PenaltyMinutes
                        ,nhl.points as Points
                        ,nhl.savePct as SavePct
                    from core.Players p
                    left join stats.MLBPlayerStats mlb
                        on mlb.playerID = p.playerID
                        and mlb.sportCode = p.sportCode
                    left join stats.NBAPlayerStats nba
                        on nba.playerID = p.playerID
                        and nba.sportCode = p.sportCode
                    left join stats.NFLPlayerStats nfl
                        on nfl.playerID = p.playerID
                        and nfl.sportCode = p.sportCode
                    left join stats.NHLPlayerStats nhl
                        on nhl.playerID = p.playerID
                        and nhl.sportCode = p.sportCode
                    where p.playerID = @playerId;";

            using var connection = new SqlConnection(base.connectionString);
            await base.GenToken(connection);
            return await connection.QuerySingleOrDefaultAsync<PlayerStatsDto>(sql, new { playerId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching V2 player stats.");
            return null;
        }
    }

    public async Task<int> CreatePlayer(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        PlayerUpsertDto player)
    {
        var sql = @"
                insert into core.Players (
                    sportCode
                    ,teamCode
                    ,positionCode
                    ,firstName
                    ,lastName
                    ,dateOfBirth
                    ,heightInches
                    ,weight
                    ,number
                    ,college
                    ,birthCityState
                    ,birthCountry
                    ,draftYear
                    ,seasonYear
                )
                output inserted.playerID
                values (
                    @sportCode
                    ,@teamCode
                    ,@positionCode
                    ,@firstName
                    ,@lastName
                    ,@dateOfBirth
                    ,@height
                    ,@weight
                    ,@number
                    ,@college
                    ,@birthCityState
                    ,@birthCountry
                    ,@draftYear
                    ,@seasonYear
                );";

        return await connection.ExecuteScalarAsync<int>(
            sql,
            new
            {
                sportCode = player.SportCode?.Trim(),
                teamCode = player.TeamCode?.Trim(),
                positionCode = player.PositionCode?.Trim(),
                firstName = player.FirstName?.Trim(),
                lastName = player.LastName?.Trim(),
                dateOfBirth = player.DateOfBirth,
                height = player.Height?.Trim(),
                weight = player.Weight,
                number = player.Number,
                college = player.College?.Trim(),
                birthCityState = player.BirthCityState?.Trim(),
                birthCountry = player.BirthCountry?.Trim(),
                draftYear = player.DraftYear,
                seasonYear = player.SeasonYear
            },
            transaction);
    }

    public async Task<bool> UpdatePlayer(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        int playerId,
        PlayerUpsertDto player)
    {
        var sql = @"
                update core.Players
                set
                    sportCode = @sportCode
                    ,teamCode = @teamCode
                    ,positionCode = @positionCode
                    ,firstName = @firstName
                    ,lastName = @lastName
                    ,dateOfBirth = @dateOfBirth
                    ,heightInches = @height
                    ,weight = @weight
                    ,number = @number
                    ,college = @college
                    ,birthCityState = @birthCityState
                    ,birthCountry = @birthCountry
                    ,draftYear = coalesce(@draftYear, draftYear)
                    ,seasonYear = coalesce(@seasonYear, seasonYear)
                where playerID = @playerId;";

        var rows = await connection.ExecuteAsync(
            sql,
            new
            {
                playerId,
                sportCode = player.SportCode?.Trim(),
                teamCode = player.TeamCode?.Trim(),
                positionCode = player.PositionCode?.Trim(),
                firstName = player.FirstName?.Trim(),
                lastName = player.LastName?.Trim(),
                dateOfBirth = player.DateOfBirth,
                height = player.Height?.Trim(),
                weight = player.Weight,
                number = player.Number,
                college = player.College?.Trim(),
                birthCityState = player.BirthCityState?.Trim(),
                birthCountry = player.BirthCountry?.Trim(),
                draftYear = player.DraftYear,
                seasonYear = player.SeasonYear
            },
            transaction);

        return rows > 0;
    }

    public async Task<bool> DeletePlayer(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        int playerId)
    {
        var sql = @"delete from core.Players where playerID = @playerId;";
        var rows = await connection.ExecuteAsync(sql, new { playerId }, transaction);
        return rows > 0;
    }

    public async Task<string?> GetPlayerSportCode(
        SqlConnection connection,
        SqlTransaction transaction,
        int playerId)
    {
        var sql = @"select sportCode from core.Players where playerID = @playerId;";
        return await connection.QuerySingleOrDefaultAsync<string>(sql, new { playerId }, transaction);
    }

    public async Task<bool> UpsertPlayerStats(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        int playerId,
        string sportCode,
        PlayerStatsUpsertDto stats)
    {
        var normalizedSport = sportCode.Trim().ToUpperInvariant();
        var sql = normalizedSport switch
        {
            "MLB" => @"
                    if exists (select 1 from stats.MLBPlayerStats where playerID = @playerId and sportCode = @sportCode)
                    begin
                        update stats.MLBPlayerStats
                        set bats = @bats
                            ,throws = @throws
                            ,battingAverage = @battingAverage
                            ,homeRuns = @homeRuns
                            ,era = @era
                        where playerID = @playerId and sportCode = @sportCode;
                    end
                    else
                    begin
                        insert into stats.MLBPlayerStats(playerID, sportCode, bats, throws, battingAverage, homeRuns, era)
                        values(@playerId, @sportCode, @bats, @throws, @battingAverage, @homeRuns, @era);
                    end;",
            "NBA" => @"
                    if exists (select 1 from stats.NBAPlayerStats where playerID = @playerId and sportCode = @sportCode)
                    begin
                        update stats.NBAPlayerStats
                        set pointsPerGame = @pointsPerGame
                            ,reboundsPerGame = @reboundsPerGame
                            ,assistsPerGame = @assistsPerGame
                        where playerID = @playerId and sportCode = @sportCode;
                    end
                    else
                    begin
                        insert into stats.NBAPlayerStats(playerID, sportCode, pointsPerGame, reboundsPerGame, assistsPerGame)
                        values(@playerId, @sportCode, @pointsPerGame, @reboundsPerGame, @assistsPerGame);
                    end;",
            "NFL" => @"
                    if exists (select 1 from stats.NFLPlayerStats where playerID = @playerId and sportCode = @sportCode)
                    begin
                        update stats.NFLPlayerStats
                        set sacks = @sacks
                            ,touchdowns = @touchdowns
                        where playerID = @playerId and sportCode = @sportCode;
                    end
                    else
                    begin
                        insert into stats.NFLPlayerStats(playerID, sportCode, sacks, touchdowns)
                        values(@playerId, @sportCode, @sacks, @touchdowns);
                    end;",
            "NHL" => @"
                    if exists (select 1 from stats.NHLPlayerStats where playerID = @playerId and sportCode = @sportCode)
                    begin
                        update stats.NHLPlayerStats
                        set handed = @handed
                            ,goals = @goals
                            ,penaltyMinutes = @penaltyMinutes
                            ,points = @points
                            ,savePct = @savePct
                        where playerID = @playerId and sportCode = @sportCode;
                    end
                    else
                    begin
                        insert into stats.NHLPlayerStats(playerID, sportCode, handed, goals, penaltyMinutes, points, savePct)
                        values(@playerId, @sportCode, @handed, @goals, @penaltyMinutes, @points, @savePct);
                    end;",
            _ => throw new ArgumentException($"Unsupported sportCode '{sportCode}' for stats upsert.")
        };

        await connection.ExecuteAsync(
            sql,
            new
            {
                playerId,
                sportCode = normalizedSport,
                bats = stats.Bats?.Trim(),
                throws = stats.Throws?.Trim(),
                battingAverage = stats.BattingAverage,
                homeRuns = stats.HomeRuns,
                era = stats.Era,
                pointsPerGame = stats.PointsPerGame,
                reboundsPerGame = stats.ReboundsPerGame,
                assistsPerGame = stats.AssistsPerGame,
                sacks = stats.Sacks,
                touchdowns = stats.Touchdowns,
                handed = stats.Handed?.Trim(),
                goals = stats.Goals,
                penaltyMinutes = stats.PenaltyMinutes,
                points = stats.Points,
                savePct = stats.SavePct
            },
            transaction);

        return true;
    }

    public async Task DeletePlayerStats(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        int playerId,
        string sportCode)
    {
        var normalizedSport = sportCode.Trim().ToUpperInvariant();
        var sql = normalizedSport switch
        {
            "MLB" => @"delete from stats.MLBPlayerStats where playerID = @playerId and sportCode = @sportCode;",
            "NBA" => @"delete from stats.NBAPlayerStats where playerID = @playerId and sportCode = @sportCode;",
            "NFL" => @"delete from stats.NFLPlayerStats where playerID = @playerId and sportCode = @sportCode;",
            "NHL" => @"delete from stats.NHLPlayerStats where playerID = @playerId and sportCode = @sportCode;",
            _ => throw new ArgumentException($"Unsupported sportCode '{sportCode}' for stats delete.")
        };

        await connection.ExecuteAsync(sql, new { playerId, sportCode = normalizedSport }, transaction);
    }
}
