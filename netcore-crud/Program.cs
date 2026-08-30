using FluentValidation;
using netcore_crud.Authorization;
using netcore_crud.Caching;
using netcore_crud.Validators.CardFile;
using netcore_crud.Validators.Login;
using netcore_crud.Validators.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(); // Session

// FluentValidation
//builder.Services.AddValidatorsFromAssemblyContaining<SecurityValidator>(); 
//builder.Services.AddValidatorsFromAssemblyContaining<ManageUsersValidator>(); 
//builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>(); 
//builder.Services.AddValidatorsFromAssemblyContaining<CardFileValidator>(); 
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddMemoryCache(); // Cache
builder.Services.AddSingleton<CacheManager>(); //Remove Cache

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();



app.UseMiddleware<PermissionMiddleware>();

//app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
