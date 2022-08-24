using Application.Activities;

var builder = WebApplication.CreateBuilder(args);

//Configure builder.Services

builder.Services.AddControllers(opt =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddValidatorsFromAssemblyContaining<Create>();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

//Configure httprequest pipeline
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseXContentTypeOptions();
app.UseReferrerPolicy(options => options.NoReferrer());
app.UseXXssProtection(options => options.EnabledWithBlockMode());
app.UseXfo(options => options.Deny());

app.UseCsp(options => options
    .BlockAllMixedContent()
    .StyleSources(s => s.Self().CustomSources("https://fonts.googleapis.com", "sha256-yR2gSI6BIICdRRE2IbNP1SJXeA5NYPbaM32i/Y8eS9o="))
    .FontSources(s => s.Self().CustomSources("https://fonts.gstatic.com", "data:"))
    .FormActions(s => s.Self())
    .FrameAncestors(s => s.Self())
    .ImageSources(s => s.Self().CustomSources(
        "https://res.cloudinary.com",
        "https://www.facebook.com",
        "https://platform-lookaside.fbsbx.com", "data:"))
    .ScriptSources(s => s.Self().CustomSources(
        "https://connect.facebook.net",
        "sha256-ynlrWxF/D1vebuto1EqQlncJkA9zYOAG/rAGDj4rmEk="))
);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
}
else
{
    // app.UseHsts();
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
        await next.Invoke();
    });
}


//app.UseRouting(); exists already

// wwwroot logic
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapHub<ChatHub>("/chat");
app.MapFallbackToController("Index", "Fallback");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

using var scope = app.Services.CreateScope(); // the scope will be disposed at the start
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>(); //service locator pattern from startup.cs
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context, userManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

await app.RunAsync();
