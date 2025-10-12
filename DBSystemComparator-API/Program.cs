using Cassandra;
using DBSystemComparator_API.Database;
using DBSystemComparator_API.Repositories.Implementations;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Implementations;
using DBSystemComparator_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL
builder.Services.AddDbContext<PostgresDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// SQL Server
builder.Services.AddDbContext<SqlServerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
    return new MongoClient(mongoConnectionString);
});
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("DBSystemComparator-DB");
});

// Cassandra
var cassContactPoint = builder.Configuration.GetValue<string>("ConnectionStrings:CassandraContactPoint");
var cassPort = builder.Configuration.GetValue<int>("ConnectionStrings:CassandraPort");
var cassKeyspace = builder.Configuration.GetValue<string>("ConnectionStrings:CassandraKeyspace");

builder.Services.AddSingleton<Cassandra.ISession>(sp =>
{
    var cluster = Cluster.Builder()
        .AddContactPoint(cassContactPoint)
        .WithPort(cassPort)
        .Build();

    var session = cluster.Connect();

    var replication = "{ 'class' : 'SimpleStrategy', 'replication_factor' : 1 }";
    session.Execute($@"
        CREATE KEYSPACE IF NOT EXISTS {cassKeyspace}
        WITH replication = {replication};
    ");

    return cluster.Connect(cassKeyspace);
});

builder.Services.AddScoped<IDataCountService, DataCountService>();
builder.Services.AddScoped<IDataSetService, DataSetService>();
builder.Services.AddScoped<IErrorLogService, ErrorLogService>();
builder.Services.AddScoped<IScenarioService, ScenarioService>();

builder.Services.AddScoped<IDataCountRepository, DataCountRepository>();
builder.Services.AddScoped<IDataSetRepository, DataSetRepository>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(a =>
{
    a.AddPolicy(builder.Configuration["Cors:ClientPolicy"], b => {
        b.WithOrigins(builder.Configuration["Cors:ValidAudience"]).AllowCredentials().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    MongoDbSeeder.CreateCollections(db);
}

using (var scope = app.Services.CreateScope())
{
    var session = scope.ServiceProvider.GetRequiredService<Cassandra.ISession>();
    CassandraSeeder.CreateTables(session);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder.Configuration["Cors:ClientPolicy"]);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
