using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace ApplicationDbContext
{
    public class ApplicationDbContext : IdentityDBContext<IndentiyUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        base(options)
        {
        builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseInMemoryDatabase("AppDb"));

        builder.Services.AddAuthorization();
        }
    }

    
}
