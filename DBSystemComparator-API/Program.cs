using Cassandra;
using DBSystemComparator_API.Database;
using DBSystemComparator_API.Repositories.Implementations;
using DBSystemComparator_API.Repositories.Interfaces;
using DBSystemComparator_API.Services.Implementations;
using DBSystemComparator_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
    return new MongoClient(mongoConnectionString);
});
builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("MongoDB");
    var databaseName = "DBSystemComparator-DB";
    return new MongoDbContext(connectionString, databaseName);
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

// PostgreSQL
builder.Services.AddScoped<IPostgreSQLRepository>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("PostgreSQL");
    return new PostgreSQLRepository(connectionString);
});

// SQLServer
builder.Services.AddScoped<ISQLServerRepository>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("SQLServer");
    return new SQLServerRepository(connectionString);
});

builder.Services.AddScoped<IMongoDBRepository, MongoDBRepository>();
builder.Services.AddScoped<ICassandraRepository, CassandraRepository>();

builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IErrorLogService, ErrorLogService>();
builder.Services.AddScoped<IScenarioService, ScenarioService>();

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

// PostgreSQL
using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("PostgreSQL");
    await PostgreSQLSeeder.CreateDatabaseAsync(connectionString);
}

// SQLServer
using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("SQLServer");
    await SQLServerSeeder.CreateDatabaseAsync(connectionString);
}

// MongoDb
using (var scope = app.Services.CreateScope())
{
    var mongoDbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await MongoDbSeeder.CreateCollectionsAndIndexesAsync(mongoDbContext.Database);
}

// Cassandra
using (var scope = app.Services.CreateScope())
{
    var session = scope.ServiceProvider.GetRequiredService<Cassandra.ISession>();
    await CassandraSeeder.CreateTablesAsync(session);
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
