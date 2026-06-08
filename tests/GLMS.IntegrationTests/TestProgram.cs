using GLMS.API;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GLMS.IntegrationTests
{
    public class TestProgram : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }
    }
}