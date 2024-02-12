using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace ApplicationDbContext
{
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddAuthorization(this Microsoft.Extensions.DependencyInjection.IServiceCollection services);

    public static Microsoft.AspNetCore.Builder.IEndpointConventionBuilder MapIdentityApi<TUser>(this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints) where TUser : class, new();

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
