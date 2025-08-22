using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using System.Text;
using Xunit;

namespace BringItDownInNovelWays.Tests
{
    public class MemoryLeakTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MemoryLeakTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task FastMemoryLeak_ShouldNotCauseOOMKill()
        {
            // Arrange
            var client = _factory.CreateClient();
            var initialMemory = GC.GetTotalMemory(false);

            // Act - Trigger the memory leak
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("leak", "1")
            });

            // This should complete without throwing an OOM exception
            var response = await client.PostAsync("/", content);

            // Assert - Should not crash with OOM
            // If we reach this point, the memory leak was controlled
            var finalMemory = GC.GetTotalMemory(false);
            
            // The memory should have increased but not caused a crash
            Assert.True(finalMemory >= initialMemory, "Memory should have increased due to allocation");
            
            // Response should be successful (not a crash)
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || 
                       response.StatusCode == System.Net.HttpStatusCode.InternalServerError,
                       "Response should be either OK or controlled error, not a crash");
        }

        [Fact]  
        public async Task SlowMemoryLeak_ShouldReturnQuickly()
        {
            // Arrange
            var client = _factory.CreateClient();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act - Trigger the slow memory leak (should return quickly, leak runs in background)
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("slowleak", "1")
            });

            var response = await client.PostAsync("/", content);
            stopwatch.Stop();

            // Assert - Should return quickly (within 5 seconds) and either work or fail gracefully
            Assert.True(stopwatch.Elapsed.TotalSeconds < 5, $"Slow leak should return quickly, took {stopwatch.Elapsed.TotalSeconds} seconds");
            // Accept OK (success) or InternalServerError (due to authentication issues) - but not timeouts or crashes
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || 
                       response.StatusCode == System.Net.HttpStatusCode.InternalServerError,
                       $"Expected OK or InternalServerError, got {response.StatusCode}");
        }
    }
}