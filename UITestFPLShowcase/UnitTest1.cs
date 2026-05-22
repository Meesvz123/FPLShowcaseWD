using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

namespace FPLShowcaseWD.UITests
{
    public class AppUITests
    {
        private const string AppUrl = "https://localhost:7244";

        [Fact]
        public async Task Login_InvalidCredentials_ShouldShowError()
        {
            Microsoft.Playwright.Program.Main(new[] { "install" });
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            await page.GotoAsync($"{AppUrl}/Identity/Account/Login"); 
            await page.FillAsync("input[type='email']", "fout@email.com");
            await page.FillAsync("input[type='password']", "FoutWachtwoord123!");
            await page.ClickAsync("button[type='submit']");

            var errorIsVisible = await page.IsVisibleAsync("text=Ongeldige login.");
            Assert.True(errorIsVisible);
        }

        [Fact]
        public async Task FantasyTeam_Page_ShouldRedirectToLogin_WhenNotAuthenticated()
        {
            Microsoft.Playwright.Program.Main(new[] { "install" });
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();

            await page.GotoAsync($"{AppUrl}/FantasyTeam");

            Assert.Contains("Login", page.Url);
        }
    }
}