using CarChecker_Real;


var builder = WebApplication.CreateBuilder(args);

//List of potential secret directories
var secretPaths = new[]
{
    "/run/secrets",                // Docker default
    Path.Combine(Directory.GetCurrentDirectory(), "secrets")  // Local dev fallback
};

foreach (var path in secretPaths)
{
    if (Directory.Exists(path))
    {
        builder.Configuration.AddKeyPerFile(path, optional: true);

        break; // Stop after the first valid path
    }
}

// Add services to the container.
// this reads/ grabs all controllers
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<Token>();
builder.Services.AddScoped<IPlateLookupService, PlateLookupService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITextelService, TextelService>();

builder.Services.AddHttpClient("Paylock", client =>
{
    var url = builder.Configuration["Paylock:URL"];
    client.BaseAddress = new Uri(url);
});
builder.Services.AddHttpClient("Textel", client =>
{
    var url = builder.Configuration["Textel:URL"];
    client.BaseAddress = new Uri(url);
});
   

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
// this adds controllers
app.MapControllers();
// no UI is needed yo bro
/*app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();*/

app.Run();