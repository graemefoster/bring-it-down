using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BringItDownInNovelWays.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (Request.Form["leak"] == "1")
        {
            // Intentionally leak memory in a loop
            var list = new List<byte[]>();
            long lastLogged = 0;
            while (true)
            {
                // Allocate 10MB at a time
                list.Add(new byte[10 * 1024 * 1024]);
                long mem = GC.GetTotalMemory(false);
                if (mem - lastLogged > 50 * 1024 * 1024) // Log every 50MB
                {
                    _logger.LogInformation($"[FAST LEAK] Total memory: {mem / (1024 * 1024)} MB");
                    lastLogged = mem;
                }
                Thread.Sleep(10); // Slow down slightly to avoid instant crash
            }
        }
        if (Request.Form["slowleak"] == "1")
        {
            // Start a slow memory leak on a background thread
            Task.Run(() =>
            {
                var list = new List<byte[]>();
                int iterations = 1200; // 20 minutes at 1s per iteration
                long lastLogged = 0;
                for (int i = 0; i < iterations; i++)
                {
                    list.Add(new byte[1024 * 1024 / 10]); // Leak 100KB per second
                    long mem = GC.GetTotalMemory(false);
                    if (mem - lastLogged >  1024 * 1024) // Log every 1MB
                    {
                        _logger.LogInformation($"[SLOW LEAK] Total memory: {mem / (1024 * 1024)} MB");
                        lastLogged = mem;
                    }
                    Thread.Sleep(1000); // 1 second
                }
            });
        }
        if (Request.Form["cpu"] == "1")
        {
            // Ramp up CPU usage: start slow, then burn CPU rapidly
            int totalSeconds = 20;
            for (int second = 1; second <= totalSeconds; second++)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                double work = second * 0.05; // Start slow, ramp up
                while (sw.Elapsed.TotalSeconds < 1)
                {
                    // Do more work as time goes on
                    for (int i = 0; i < work * 1_000_000; i++)
                    {
                        double x = Math.Sqrt(i) * Math.Sin(i);
                    }
                }
                _logger.LogInformation($"[CPU RAMP] Second {second}/{totalSeconds} complete");
            }
            // Burn CPU at max for a few seconds before returning
            var burnSw = System.Diagnostics.Stopwatch.StartNew();
            while (burnSw.Elapsed.TotalSeconds < 5)
            {
                for (int i = 0; i < 10_000_000; i++)
                {
                    double x = Math.Sqrt(i) * Math.Sin(i);
                }
            }
            _logger.LogInformation("[CPU RAMP] Max CPU burn complete");
        }

        if (Request.Form["logicError"] == "1")
        {
            throw new InvalidOperationException("Sorry but an internal server error occurred. Please try again later.");
        }
        return Page();
    }
}