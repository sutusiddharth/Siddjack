using CLAPi.ExcelEngine.Api.BackGroundJob;
using CLAPi.ExcelEngine.Api.Services;
using CLAPi.ExcelEngine.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using ServiceDefaults.Extensions;
using ServiceDefaults.Middleware;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Service Defaults (applies base configuration defaults early)
builder.AddServiceDefaults();

// Add HttpContextAccessor (needed for accessing the HTTP context in services)
builder.Services.AddHttpContextAccessor();

// Add Configuration services (specific to the application's configuration needs)
builder.AddConfig();

// Add Controllers (MVC pattern and REST endpoints)
builder.Services.AddControllers();

// Add FluentValidation services (for automatic model validation via FluentValidation)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Add API Versioning (to support versioning of the API endpoints)
builder.AddApiVersioningConfigured();

// Add Global Exception Handler (to catch and process unhandled exceptions globally)
builder.AddGlobalExceptionHandler();

// Add Authentication services (handles user authentication and token management)
builder.Services.AddAuthentications();

// Register Dependency Injection for custom services (your application services go here)
builder.Services.AddDependencyInjection();

// Add Background service (for processing jobs in the background)
builder.Services.AddHostedService<QueuedHostedService>();

#region CORS
// Add CORS policy to allow specific hosts (allows cross-origin requests)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowOrigin",
        policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin();  // Modify to restrict to certain origins for security
        });
});
#endregion

var app = builder.Build();

// Map default endpoints before middleware to have them available globally
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    // Enable detailed error pages during development
    app.UseDeveloperExceptionPage();
}
else
{
    // Use global exception handler only in non-development environments
    app.UseGlobalExceptionHandler();
}

// Enable HTTPS redirection (forces all requests to be HTTPS)
app.UseHttpsRedirection();

// Custom middleware for processing requests (e.g., custom headers)
app.UseMiddleware<ActionMethodFromHeaderMiddleware>();

// Enable CORS (should be placed after the middleware that needs to run first)
app.UseCors("AllowOrigin");

// Enable Routing (should come before UseAuthorization)
app.UseRouting();

// Enable Authentication and Authorization (ensure users are authenticated before accessing secure routes)
app.UseAuthentication();  // Add authentication middleware for securing requests
app.UseAuthorization();

// Use any custom middleware for request-response logging or processing
app.UseHttpRequestResponseMiddleware();

// Map Controllers (after routing middleware)
app.MapControllers();

// Run the application asynchronously
await app.RunAsync();
