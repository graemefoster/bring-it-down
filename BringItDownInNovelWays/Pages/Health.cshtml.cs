using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BringItDownInNovelWays.Pages;

[AllowAnonymous]
public class Health : PageModel
{
    public void OnGet()
    {
        
    }
}