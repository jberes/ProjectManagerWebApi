using Microsoft.EntityFrameworkCore;
using ProjectManagerWebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Task = ProjectManagerWebApi.Models.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//builder.Services.AddSqlServer<ProjectTrackerContext>(builder.Configuration.GetConnectionString("DefaultConnection"));

//var connString = builder.Configuration.GetConnectionString("AzureConnection");

var keyVaultEndpoint = new Uri(builder.Configuration["VaultKey"]);
var secretClient = new SecretClient(keyVaultEndpoint, new DefaultAzureCredential());
KeyVaultSecret kvs = secretClient.GetSecret("projecttrackersecret2");

builder.Services.AddDbContext<ProjectTrackerContext>(o => o.UseSqlServer(kvs.Value));
//builder.Services.AddDbContext<ProjectTrackerContext>(o => o.UseSqlServer(connString));


builder.Services.AddScoped<ProjectTrackerContextProcedures>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ******************************************************************
// GET / POST / PUT / DEL for TASKS

app.MapGet("tasks", async ([FromServices] ProjectTrackerContext db) =>
{ return await db.Tasks.ToListAsync(); });

app.MapPost("task", async ([FromServices] ProjectTrackerContext db, [FromBody] Task task) =>
{
    var newTask = new Task()
    {
        TaskName = task.TaskName,
        DateUpdated = DateTime.Now,
        DateDue = task.DateDue,
        ProjectId = task.ProjectId,
        AssignedToEmail = task.AssignedToEmail,
        Priority = task.Priority,
    };
    await db.Tasks.AddAsync(newTask);
    await db.SaveChangesAsync();
    return TypedResults.Ok(newTask);
});

app.MapPut("task", async ([FromServices] ProjectTrackerContext db, [FromBody] Task task) =>
{

    var dbTask = await db.Tasks.FindAsync(task.TaskId);
    if (dbTask == null)
    {
        return TypedResults.Ok(dbTask);
    }

    dbTask.ProjectId = task.ProjectId;
    dbTask.TaskName = task.TaskName;
    dbTask.DateUpdated = System.DateTime.Now;
    dbTask.DateDue = task.DateDue;
    dbTask.AssignedToEmail = task.AssignedToEmail;
    dbTask.Priority = task.Priority;
    await db.SaveChangesAsync();
    return TypedResults.Ok(dbTask);
});


app.MapDelete("task/{id}", async ([FromServices] ProjectTrackerContextProcedures db, int id) =>
{
    var op = new OutputParameter<int>();
    await db.sp_Delete_TaskAsync(id, op);
    return await db.sp_Select_TaskAsync(id, op);
});


app.MapGet("projects", async ([FromServices] ProjectTrackerContextProcedures db) =>
{
    var op = new OutputParameter<int>();
    return await db.sp_Select_ProjectsAsync(op);
});

app.UseHttpsRedirection();
app.Run();