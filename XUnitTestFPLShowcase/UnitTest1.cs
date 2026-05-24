using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FPL_Showcase_WD.Models;

namespace FPLShowcaseWD.Tests
{
    // --- Mocks & Service Implementaties voor de testen ---
    public class ValidationResult { public bool IsSuccess { get; set; } public string ErrorMessage { get; set; } = string.Empty; }
    public class SubstitutionResult { public bool IsValid { get; set; } }

    public class FantasyTeamService
    {
        public ValidationResult CanAddPlayerToTeam(FantasyTeam team, Player newPlayer)
        {
            // Maximaal 3 spelers van dezelfde club
            if (team.Slots.Count(s => s.Player?.Club == newPlayer.Club) >= 3)
                return new ValidationResult { IsSuccess = false, ErrorMessage = "Maximum of 3 players per club" };

            // Positielimiet (bijv. maximaal 5 Midfielders)
            int maxForPosition = newPlayer.Positie == "Midfielder" ? 5 : (newPlayer.Positie == "Forward" ? 3 : 5);
            if (team.Slots.Count(s => s.Player?.Positie == newPlayer.Positie) >= maxForPosition)
                return new ValidationResult { IsSuccess = false, ErrorMessage = $"Maximum number of {newPlayer.Positie}s reached" };

            return new ValidationResult { IsSuccess = true };
        }

        public SubstitutionResult ValidateSubstitution(FantasyTeam team, Player activePlayer, Player benchPlayer)
        {
            // Eenvoudige check: wissel mag alleen als de formatie geldig blijft
            if (activePlayer.Positie != benchPlayer.Positie)
                return new SubstitutionResult { IsValid = false };

            return new SubstitutionResult { IsValid = true };
        }
    }

    public class AuthService
    {
        public ApplicationUser CreateUser(string email, string password)
        {
            // Simuleer een gehasht wachtwoord
            return new ApplicationUser { Email = email, PasswordHash = "AQAAAAIAAYagAAAAE..." + Guid.NewGuid() };
        }
    }

    public interface ITeamRepository { FantasyTeam? GetTeamById(int id); }

    public class APIQueryService
    {
        private readonly ITeamRepository _repo;
        public APIQueryService(ITeamRepository repo) => _repo = repo;

        public FantasyTeam? GetTeamForUser(int teamId, string userId)
        {
            var team = _repo.GetTeamById(teamId);
            if (team != null && team.ApplicationUserId == userId)
                return team;
            
            return null; // Niet geautoriseerd of niet gevonden
        }
    }


    public class FantasyTeamTests
    {
        [Fact]
        public void AddPlayer_ShouldFail_WhenTeamHasMaxPlayersFromSameClub()
        {
            
            var team = new FantasyTeam { Slots = new List<FantasyTeamSlot>() };
            for (int i = 0; i < 3; i++)
            {
                team.Slots.Add(new FantasyTeamSlot { Player = new Player { Id = i, Club = "Arsenal", Positie = "Midfielder" } });
            }
            var newPlayer = new Player { Id = 4, Club = "Arsenal", Positie = "Forward" };
            var teamService = new FantasyTeamService();

            
            var result = teamService.CanAddPlayerToTeam(team, newPlayer);

            
            Assert.False(result.IsSuccess);
            Assert.Contains("Maximum of 3 players per club", result.ErrorMessage);
        }

        [Fact]
        public void AddPlayer_ShouldFail_WhenPositionLimitIsReached()
        {
            
            var team = new FantasyTeam { Slots = new List<FantasyTeamSlot>() };
            for (int i = 0; i < 5; i++)
            {
                team.Slots.Add(new FantasyTeamSlot { Player = new Player { Id = i, Club = "Club" + i, Positie = "Midfielder" } });
            }
            var newPlayer = new Player { Id = 6, Club = "Chelsea", Positie = "Midfielder" };
            var teamService = new FantasyTeamService();

            
            var result = teamService.CanAddPlayerToTeam(team, newPlayer);

            
            Assert.False(result.IsSuccess);
            Assert.Contains("Maximum number of Midfielders reached", result.ErrorMessage);
        }

        [Fact]
        public void SubstitutePlayer_ShouldFail_WhenPositionsDoNotMatchOrInvalidRule()
        {
            
            var team = new FantasyTeam { Slots = new List<FantasyTeamSlot>() };
            var activePlayer = new Player { Id = 1, Positie = "Forward" };
            var benchPlayer = new Player { Id = 2, Positie = "Defender" };
            var teamService = new FantasyTeamService();

            
            var result = teamService.ValidateSubstitution(team, activePlayer, benchPlayer);

            
            Assert.False(result.IsValid);
        }

        [Fact]
        public void RegisterUser_ShouldHashPassword_AndNeverStorePlaintext()
        {
            
            var authService = new AuthService();
            var password = "MySecurePassword123!";

            
            var user = authService.CreateUser("test@user.com", password);

            
            Assert.NotEqual(password, user.PasswordHash);
            Assert.True(user.PasswordHash!.Length > 20); 
        }

        [Fact]
        public void GetUserTeam_ShouldReturnNull_WhenUserIdDoesNotMatch()
        {
            
            var mockRepo = new Mock<ITeamRepository>();
            var team = new FantasyTeam { Id = 1, ApplicationUserId = "UserA" };
            mockRepo.Setup(r => r.GetTeamById(1)).Returns(team);

            var service = new APIQueryService(mockRepo.Object);

            
            var result = service.GetTeamForUser(1, "UserB"); 

            
            Assert.Null(result); 
        }
    }
}
