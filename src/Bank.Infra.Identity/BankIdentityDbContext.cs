using Bank.Infra.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infra.Identity
{
    public class BankIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public BankIdentityDbContext(DbContextOptions<BankIdentityDbContext> options) : base(options)
        {
        }
    }
}
