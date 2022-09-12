using System;
using System.Linq;

namespace ABT
{

    public sealed partial class ArchitectureBuilder
    {
        private abstract class Resources
        {
            #region CsProjs & References

            internal static string SolutionSln(string solutionName, string currentLayerName)
            {
                var solutionId = Guid.NewGuid().ToString().ToUpper();
                var projectId = Guid.NewGuid().ToString().ToUpper();
                var currentLayerId = Guid.NewGuid().ToString().ToUpper();
                var domainId = Guid.NewGuid().ToString().ToUpper();
                var dataId = Guid.NewGuid().ToString().ToUpper();
                var serviceId = Guid.NewGuid().ToString().ToUpper();

                return $@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.2.32616.157
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{{projectId}}}"") = ""{solutionName}.Domain"", ""{solutionName}.Domain\{solutionName}.Domain.csproj"", ""{{{domainId}}}""
EndProject
Project(""{{{projectId}}}"") = ""{currentLayerName}"", ""{currentLayerName}\{currentLayerName}.csproj"", ""{{{currentLayerId}}}""
EndProject
Project(""{{{projectId}}}"") = ""{solutionName}.Service"", ""{solutionName}.Service\{solutionName}.Service.csproj"", ""{{{serviceId}}}""
EndProject
Project(""{{{projectId}}}"") = ""{solutionName}.Data"", ""{solutionName}.Data\{solutionName}.Data.csproj"", ""{{{dataId}}}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{{domainId}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{domainId}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{domainId}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{domainId}}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{{currentLayerId}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{currentLayerId}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{currentLayerId}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{currentLayerId}}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{{serviceId}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{serviceId}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{serviceId}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{serviceId}}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{{dataId}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{dataId}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{dataId}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{dataId}}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {{{solutionId}}}
	EndGlobalSection
EndGlobal
";
            }

            internal static string AddNewReference(string solutionContent, string newReferenceName)
            {
                var newReferenceId = Guid.NewGuid().ToString().ToUpper();
                var buildMode = $@"
		{{{newReferenceId}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{newReferenceId}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{newReferenceId}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{newReferenceId}}}.Release|Any CPU.Build.0 = Release|Any CPU";

                var projectId = new string(solutionContent
                    .Skip(solutionContent.IndexOf("Project", StringComparison.Ordinal) + "Project(\"{".Length)
                    .Take(36)
                    .ToArray());

                var projectInsertIndex = solutionContent.LastIndexOf("EndProject", StringComparison.Ordinal) +
                                         "EndProject".Length;
                var buildModeInsertIndex =
                    solutionContent.LastIndexOf("Any CPU", StringComparison.Ordinal) + "Any CPU".Length;

                var newProject =
                    $@"
Project(""{{{projectId}}}"") = ""{newReferenceName}"", ""{newReferenceName}\{newReferenceName}.csproj"", ""{{{newReferenceId}}}""
EndProject";

                var content = solutionContent[..projectInsertIndex] + newProject +
                              solutionContent[projectInsertIndex..buildModeInsertIndex] + buildMode +
                              solutionContent[buildModeInsertIndex..];
                return content;
            }

            internal static string DomainLayerCsProj()
            {
                return @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>
</Project>";
            }

            internal static string ServiceLayerCsProj(string solutionName)
            {
                return $@"<Project Sdk=""Microsoft.NET.Sdk"">
	<PropertyGroup>
	    <TargetFramework>net5.0</TargetFramework>
	</PropertyGroup>

	<ItemGroup>
        <ProjectReference Include=""..\{solutionName}.Data\{solutionName}.Data.csproj"" />
        <ProjectReference Include=""..\{solutionName}.Domain\{solutionName}.Domain.csproj"" />
	</ItemGroup>

	<ItemGroup>
        <PackageReference Include=""AutoMapper"" Version=""11.0.1"" />
        <PackageReference Include=""Microsoft.AspNetCore.Http.Abstractions"" Version=""2.2.0"" />
        <PackageReference Include=""Microsoft.AspNetCore.Mvc.NewtonsoftJson"" Version=""5.0.17"" />
        <PackageReference Include=""Microsoft.IdentityModel.Tokens"" Version=""6.10.0"" />
        <PackageReference Include=""System.IdentityModel.Tokens.Jwt"" Version=""6.10.0"" />
	</ItemGroup>
</Project>";
            }

            internal static string DataLayerCsProj(string solutionName, EfContext context)
            {
                var (ef, version) = context switch
                {
                    EfContext.Npgsql => ("Npgsql.EntityFrameworkCore.PostgreSQL", "5.0.10"),
                    EfContext.Mysql => ("MySql.EntityFrameworkCore", "5.0.16"),
                    EfContext.Sqlite => ("Microsoft.EntityFrameworkCore.Sqlite", "5.0.17"),
                    EfContext.Mssql => ("Microsoft.EntityFrameworkCore.SqlServer", "5.0.17"),
                    _ => throw new ArgumentOutOfRangeException(nameof(context), context, null)
                };

                return $@"<Project Sdk=""Microsoft.NET.Sdk"">
	<PropertyGroup>
	    <TargetFramework>net5.0</TargetFramework>
	</PropertyGroup>

	<ItemGroup>
        <ProjectReference Include=""..\{solutionName}.Domain\{solutionName}.Domain.csproj"" />
	</ItemGroup>

	<ItemGroup>
        <PackageReference Include=""{ef}"" Version=""{version}"" />
	</ItemGroup>
</Project>";
            }

            #endregion

            #region Domain Layer Resources

            internal static string PaginationParameters(string solutionName)
            {
                return $@"namespace {solutionName}.Domain.Commons
{{
    public class PaginationParameters
    {{
        private const int maxPageSize = 20;
        private int pageSize;

        public int PageSize
        {{
            get => pageSize;
            set => pageSize = value > maxPageSize ? maxPageSize : value;
        }}

        public int PageIndex {{ get; set; }}
    }}
}}";
            }

            internal static string PaginationMetaData(string solutionName)
            {
                return $@"using System;

namespace {solutionName}.Domain.Commons
{{
    public class PaginationMetaData
    {{
        public PaginationMetaData(int totalCount, int pageSize, int pageIndex)
        {{
            CurrentPage = pageIndex;
            TotalCount = totalCount;
            TotalPages = (int) Math.Ceiling(totalCount / (double) pageSize);
        }}

        public int CurrentPage {{ get; }}
        public int TotalCount {{ get; }}
        public int TotalPages {{ get; }}

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }}
}}";
            }

            internal static string ErrorDetails(string solutionName)
            {
                return $@"namespace {solutionName}.Domain.Commons
{{
    /// <summary>
    /// This class will be used for returning errors
    /// </summary>
    public class ErrorDetails
    {{
        public string Message {{ get; set; }} = string.Empty;
        public int StatusCode {{ get; set; }}
    }}
}}";
            }

            #endregion

            #region Data Layer Resources

            internal static string GenericRepositoryInterface(string solutionName)
            {
                return $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace {solutionName}.Data.IRepositories
{{
    public interface IGenericRepository<TSource> where TSource : class
    {{
        ValueTask<EntityEntry<TSource>> CreateAsync(TSource source);
        TSource Update(TSource source);
        Task<TSource> GetAsync(Expression<Func<TSource, bool>> expression);
        Task<bool> AnyAsync(Expression<Func<TSource, bool>> expression);
        IQueryable<TSource> Where(Expression<Func<TSource, bool>> expression = null);
        void DeleteRange(IEnumerable<TSource> sources);
    }}
}}";
            }

            internal static string GenericRepository(string solutionName)
            {
                return $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using {solutionName}.Data.Contexts;
using {solutionName}.Data.IRepositories;


namespace {solutionName}.Data.Repositories
{{
    public class GenericRepository<TSource> : IGenericRepository<TSource> where TSource : class
    {{
        private readonly {solutionName}DbContext _dbContext;
        private readonly DbSet<TSource> _dbSet;

        public GenericRepository({solutionName}DbContext dbContext)
        {{
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TSource>();
        }}

        public ValueTask<EntityEntry<TSource>> CreateAsync(TSource source)
            => _dbSet.AddAsync(source);

        public TSource Update(TSource source)
            => _dbSet.Update(source).Entity;

        public Task<TSource> GetAsync(Expression<Func<TSource, bool>> expression)
            => _dbSet.FirstOrDefaultAsync(expression);

        public Task<bool> AnyAsync(Expression<Func<TSource, bool>> expression) => _dbSet.AnyAsync(expression);

        public void DeleteRange(IEnumerable<TSource> sources) => _dbSet.RemoveRange(sources);

        public IQueryable<TSource> Where(Expression<Func<TSource, bool>> expression = null)
            => expression is null ? _dbSet : _dbSet.Where(expression);
    }}
}}";
            }

            internal static string UnitOfWorkInterface(string solutionName)
            {
                return $@"using System;
using System.Threading.Tasks;
using {solutionName}.Domain.Entities;

namespace {solutionName}.Data.IRepositories
{{
	public interface IUnitOfWork : IDisposable
	{{
		Task SaveChangesAsync();
	}}
}}";
            }

            internal static string UnitOfWork(string solutionName)
            {
                return $@"using System;
using System.Threading.Tasks;
using {solutionName}.Domain.Entities;
using {solutionName}.Data.Contexts;
using {solutionName}.Data.IRepositories;

namespace {solutionName}.Data.Repositories
{{

    public class UnitOfWork : IUnitOfWork
    {{
        private readonly {solutionName}DbContext _dbContext;

        public UnitOfWork({solutionName}DbContext dbContext)
        {{
            _dbContext = dbContext;
        }}

        public void Dispose() => GC.SuppressFinalize(this);

        public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();
    }}
}}";
            }

            internal static string DatabaseContext(string solutionName, EfContext context, string connectionString)
            {
                var databaseContext = context switch
                {
                    EfContext.Npgsql => "UseNpgsql",
                    EfContext.Mysql => "UseMySQL",
                    EfContext.Sqlite => "UseSqlite",
                    EfContext.Mssql => "UseSqlServer",
                    _ => throw new Exception("Not suitable database context was given")
                };

                return $@"using Microsoft.EntityFrameworkCore;

namespace {solutionName}.Data.Contexts
{{
    public class {solutionName}DbContext : DbContext
    {{
        public {solutionName}DbContext(DbContextOptions<{solutionName}DbContext> options): base(options)
        {{
            
        }}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {{
            
        }}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {{
            string connectionString = ""{connectionString}"";

            optionsBuilder.{databaseContext}(connectionString);
        }}

    }}
}}";
            }

            #endregion

            #region Service Layer Resources

            internal static string ServiceInterface(string solutionName, string modelName)
            {
                return $@"using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using {solutionName}.Domain.Commons;
using {solutionName}.Domain.Entities;

namespace {solutionName}.Service.Interfaces
{{
    public interface I{modelName}Service
    {{
        Task<{modelName}> CreateAsync({modelName} model);

        Task<{modelName}> UpdateAsync(int id, {modelName} model);

        Task<bool> DeleteAsync(Expression<Func<{modelName}, bool>> expression);

        Task<{modelName}> GetAsync(Expression<Func<{modelName}, bool>> expression);

        Task<IEnumerable<{modelName}>> GetAllAsync(Expression<Func<{modelName}, bool>> expression = null,
            PaginationParameters parameters = null);
    }}
}}";
            }

            internal static string Service(string solutionName, string modelName)
            {
                return $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using {solutionName}.Data.IRepositories;
using {solutionName}.Domain.Commons;
using {solutionName}.Domain.Entities;
using {solutionName}.Service.Exceptions;
using {solutionName}.Service.Extensions;
using {solutionName}.Service.Interfaces;
using AutoMapper;

namespace {solutionName}.Service.Services
{{
    public class {modelName}Service : I{modelName}Service
    {{
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public {modelName}Service(IUnitOfWork unitOfWork, IMapper mapper)
        {{
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }}

        public async Task<{modelName}> CreateAsync({modelName} model)
        {{
            // Logic goes here...
            
            var result = (await _unitOfWork.{modelName}s.CreateAsync(model)).Entity;
            await _unitOfWork.SaveChangesAsync();

            return result;
        }}

        public async Task<{modelName}> UpdateAsync(int id, {modelName} model)
        {{
            var exist{modelName} = await _unitOfWork.{modelName}s.GetAsync({modelName.ToLower()} => {modelName.ToLower()}.Id == id);
            
            if (exist{modelName} is null)
            {{
                throw new HttpStatusCodeException(400, ""{modelName} not found!"");
            }}
            
            // Updates goes here
            // model -> exist{modelName}
            _unitOfWork.{modelName}s.Update(exist{modelName});
            await _unitOfWork.SaveChangesAsync();

            return exist{modelName};
        }}

        public async Task<bool> DeleteAsync(Expression<Func<{modelName}, bool>> expression)
        {{
            var {modelName.ToLower()}s = _unitOfWork.{modelName}s.Where(expression);
            
            if (!{modelName.ToLower()}s.Any())
            {{
                throw new HttpStatusCodeException(400, ""{modelName} not found!"");
            }}

            _unitOfWork.{modelName}s.DeleteRange({modelName.ToLower()}s);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }}

        public Task<{modelName}> GetAsync(Expression<Func<{modelName}, bool>> expression)
            => _unitOfWork.{modelName}s.GetAsync(expression);

        public Task<IEnumerable<{modelName}>> GetAllAsync(Expression<Func<{modelName}, bool>> expression = null,
            PaginationParameters parameters = null)
            => Task.FromResult<IEnumerable<{modelName}>>(_unitOfWork.{modelName}s.Where(expression).ToPagedList(parameters));
    }}
}}";
            }

            internal static string MappingProfile(string solutionName)
            {
                return $@"using {solutionName}.Service.ViewModels;
using AutoMapper;
using {solutionName}.Domain.Entities;
using {solutionName}.Service.DTOs;

namespace {solutionName}.Service.Mappers
{{
	public class MapperProfile : Profile
	{{
		public MapperProfile()
		{{" + "\t\t}\n\t}\n}";
            }

            internal static string CollectionExtensions(string solutionName)
            {
                return $@"using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using {solutionName}.Domain.Commons;
using static {solutionName}.Service.Helpers.HttpContextHelper;

namespace {solutionName}.Service.Extensions
{{

    public static class CollectionExtensions
    {{
        public static IEnumerable<T> ToPagedList<T>(this IEnumerable<T> source, PaginationParameters parameters)
            where T : class
        {{
            parameters ??= new PaginationParameters {{PageSize = 20, PageIndex = 1}};

            var paginationMetaData = new PaginationMetaData(source.Count(), parameters.PageSize, parameters.PageIndex);

            if (ResponseHeaders.ContainsKey(""X-Pagination""))
                ResponseHeaders.Remove(""X-Pagination"");

            ResponseHeaders.Add(""X-Pagination"", JsonConvert.SerializeObject(paginationMetaData));

            ResponseHeaders.Add(""Access-Control-Expose-Headers"", ""X-Pagination"");

            return parameters.PageSize >= 0 && parameters.PageIndex > 0
                ? source.Skip(parameters.PageSize * (parameters.PageIndex - 1)).Take(parameters.PageSize)
                : source;
        }}

        public static IQueryable<T> ToPagedList<T>(this IQueryable<T> source, PaginationParameters parameters)
            where T : class
        {{
            parameters ??= new PaginationParameters {{PageSize = 20, PageIndex = 1}};

            var paginationMetaData = new PaginationMetaData(source.Count(), parameters.PageSize, parameters.PageIndex);

            if (ResponseHeaders.ContainsKey(""X-Pagination""))
                ResponseHeaders.Remove(""X-Pagination"");

            ResponseHeaders.Add(""X-Pagination"", JsonConvert.SerializeObject(paginationMetaData));

            ResponseHeaders.Add(""Access-Control-Expose-Headers"", ""X-Pagination"");

            return parameters.PageSize >= 0 && parameters.PageIndex > 0
                ? source.Skip(parameters.PageSize * (parameters.PageIndex - 1)).Take(parameters.PageSize)
                : source;

        }}
    }}
}}";
            }

            internal static string HttpStatusCodeException(string solutionName)
            {
                return $@"using System;
using Newtonsoft.Json.Linq;

namespace {solutionName}.Service.Exceptions
{{
    public class HttpStatusCodeException : Exception
    {{
        public int StatusCode {{ get; set; }}
        public string ContentType {{ get; set; }} = @""text/plain"";

        public HttpStatusCodeException(int statusCode, string message)
            : base(message)
        {{
            this.StatusCode = statusCode;
        }}

        public HttpStatusCodeException(int statusCode, Exception inner)
            : this(statusCode, inner.ToString())
        {{

        }}

        public HttpStatusCodeException(int statusCode, JObject errorObject)
            : this(statusCode, errorObject.ToString())
        {{
            this.ContentType = @""application/json"";
        }}
    }}
}}";
            }

            internal static string HttpContextHelper(string solutionName)
            {
                return $@"using Microsoft.AspNetCore.Http;

namespace {solutionName}.Service.Helpers
{{
    public class HttpContextHelper
    {{
        public static IHttpContextAccessor Accessor;
        public static HttpContext Context => Accessor.HttpContext ?? new DefaultHttpContext();
        public static IHeaderDictionary ResponseHeaders => Context.Response?.Headers ?? new HeaderDictionary();

        public static long? UserId => long.Parse(Context?.User?.FindFirst(""Id"")?.Value ?? ""0"");
        public static string Role => Context?.User?.FindFirst(""Role"")?.Value;
    }}
}}";
            }

            internal static string CustomExceptionMiddleware(string solutionName)
            {
                return $@"using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using {solutionName}.Domain.Commons;
using {solutionName}.Service.Exceptions;

namespace {solutionName}.Service.Middlewares
{{
    public class CustomExceptionMiddleware
    {{
        private readonly RequestDelegate _next;

        public CustomExceptionMiddleware(RequestDelegate next)
        {{
            _next = next;
        }}

        public async Task Invoke(HttpContext context /* other dependencies */)
        {{
            try
            {{
                await _next(context);
            }}
            catch (HttpStatusCodeException ex)
            {{
                await HandleExceptionAsync(context, ex);
            }}
            catch (Exception ex)
            {{
                await HandleExceptionAsync(context, ex);
            }}
        }}

        private Task HandleExceptionAsync(HttpContext context, HttpStatusCodeException exception)
        {{
            context.Response.ContentType = ""application/json"";
            var result = new ErrorDetails
            {{
                Message = exception.Message,
                StatusCode = exception.StatusCode
            }};
            context.Response.StatusCode = exception.StatusCode;
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }}

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {{
            context.Response.ContentType = ""application/json"";
            var result = new ErrorDetails
            {{
                Message = exception.ToString(),
                StatusCode = 500
            }};
            context.Response.StatusCode = 500;
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }}
    }}
}}";
            }

            #endregion

            #region ApiLayerResources

            internal static string CustomServices(string solutionName, string apiProjectNamespace)
            {
                return $@"using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using {solutionName}.Data.IRepositories;
using {solutionName}.Data.Repositories;
using {solutionName}.Domain.Models;
using {solutionName}.Service.Interfaces;
using {solutionName}.Service.Mappers;
using {solutionName}.Service.Services;

namespace {apiProjectNamespace}.Extensions;

public static class CustomServices
{{
    /// <summary>
    /// Custom Services
    /// </summary>
    /// <param name=""services""></param>
    public static void AddCustomServices(this IServiceCollection services)
    {{
        // here services need to be implemented

        services.AddAutoMapper(typeof(MapperProfile));
    }}
    
    /// <summary>
    /// Setup JWT service for security using token
    /// </summary>
    /// <param name=""services""></param>
    /// <param name=""config""></param>
    public static void AddJwtService(this IServiceCollection services, IConfiguration config)
    {{
        services.AddAuthentication();

        // Jwt bearer configuration
        services.AddAuthentication(options =>
        {{
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }}).AddJwtBearer(options =>
        {{
            options.TokenValidationParameters = new TokenValidationParameters
            {{
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config[""Jwt:Issuer""],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config[""Jwt:Key""]))
            }};
        }});
        services.AddMvc().AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
    }}

    /// <summary>
    /// Setup CORs
    /// </summary>
    /// <param name=""services""></param>
    public static void AddCorsService(this IServiceCollection services)
    {{
        services.AddCors(options =>
        {{
            options.AddPolicy(""AllowAll"", builder =>
            {{
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }});
        }});
    }}

    /// <summary>
    /// Setup swagger
    /// </summary>
    /// <param name=""services""></param>
    public static void AddSwaggerService(this IServiceCollection services)
    {{
        services.AddSwaggerGen(c =>
        {{
            c.SwaggerDoc(""v1"", new OpenApiInfo {{Title = ""{apiProjectNamespace}"", Version = ""v1""}});

            c.AddSecurityDefinition(""Bearer"", new OpenApiSecurityScheme
            {{
                Name = ""Authorization"",
                Description =
                    ""JWT Authorization header using the Bearer scheme. Example: \""Authorization: Bearer {{token}}\"""",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            }});

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {{
                {{
                    new OpenApiSecurityScheme
                    {{
                        Reference = new OpenApiReference
                        {{
                            Type = ReferenceType.SecurityScheme,
                            Id = ""Bearer""
                        }}
                    }},
                    new string[] {{ }}
                }}
            }});
        }});
    }}   
}}";
            }

            internal static string ApiProgramCs(string solutionName, string apiProjectNamespace)
            {
                return $@"using Microsoft.EntityFrameworkCore;
using {apiProjectNamespace}.Extensions;
using {solutionName}.Data.Contexts;
using {solutionName}.Service.Helpers;
using {solutionName}.Service.Middlewares;
using Serilog;
using Serilog.Events;

var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""Logs"", ""log.txt"");
Log.Logger = new LoggerConfiguration().WriteTo.File
(
    path,
    rollingInterval: RollingInterval.Day,
    restrictedToMinimumLevel: LogEventLevel.Information,
    outputTemplate: ""{{Timestamp:yyyy-MM-dd HH:mm:ss}} [{{Level:u3}}] {{Message}}{{NewLine}}{{Exception}}""
).CreateLogger();

try
{{
    Log.Information(""Application has been started!"");
    var builder = WebApplication.CreateBuilder(args);
    var connectionString = builder.Configuration.GetConnectionString(""DefaultConnection"");
    
    // Add services to the container.
    builder.Services.AddControllers();

    builder.Services.AddDbContext<{solutionName}DbContext>(optionsBuilder =>
        optionsBuilder.UseNpgsql(connectionString, contextOptionsBuilder =>
            contextOptionsBuilder.MigrationsAssembly(""{apiProjectNamespace}"")));
    
    // Custom
    builder.Services.AddCustomServices();
    builder.Services.AddCorsService();
    builder.Services.AddJwtService(builder.Configuration);
    builder.Services.AddSwaggerService();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {{
        app.UseSwagger();
        app.UseSwaggerUI();
    }}

    HttpContextHelper.Accessor = app.Services.GetService<IHttpContextAccessor>() ?? new HttpContextAccessor();
    
    // Custom Middleware
    app.UseMiddleware<CustomExceptionMiddleware>();
    
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}}
catch (Exception exception)
{{
    Log.Fatal(exception, ""Application couldn't be started!"");
}}
finally
{{
    Log.CloseAndFlush();
}}
";
            }

            internal static string AppSettingsJson(string connectionString)
            {
                var random = new Random();
                var issuer = new string("someKindIssuerStringShouldBeHere123432".OrderBy(_ => random.Next()).ToArray());
                var audience = new string("hereCouldBeSomeGoodWords(*)!".OrderBy(_ => random.Next()).ToArray());
                var key = new string("#$@@1234321CoolPackageIsHere".OrderBy(_ => random.Next()).ToArray());

                return $@"{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning"",
      ""Microsoft"": ""Warning"",
      ""Microsoft.Hosting.Lifetime"": ""Information""
    }}
  }},
  ""AllowedHosts"": ""*"",
  ""Jwt"": {{
    ""Key"": ""This application build by ABT package. {key.Take(10)}"",
    ""Issuer"": ""{issuer.Take(20)}"",
    ""Audience"": ""{audience.Take(15)}"",
    ""Expire"": ""3000""
  }},
  ""ConnectionStrings"": {{
    ""DefaultConnection"": ""{connectionString}""
  }}
}}
";
            }

            internal static string LaunchSettings(string apiProjectNamespace)
            {
                return $@"{{
  ""$schema"": ""https://json.schemastore.org/launchsettings.json"",
  ""iisSettings"": {{
    ""windowsAuthentication"": false,
    ""anonymousAuthentication"": true,
    ""iisExpress"": {{
      ""applicationUrl"": ""http://localhost:41725"",
      ""sslPort"": 44390
    }}
  }},
  ""profiles"": {{
    ""{apiProjectNamespace}"": {{
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""applicationUrl"": ""https://localhost:7184;http://localhost:5136"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }}
    }},
    ""IIS Express"": {{
      ""commandName"": ""IISExpress"",
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }}
    }}
  }}
}}
";
            }

            #endregion
        }
    }
}