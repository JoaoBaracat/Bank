using Microsoft.AspNetCore.Identity;

namespace Bank.Infra.Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
