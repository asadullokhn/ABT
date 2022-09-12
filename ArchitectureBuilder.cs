using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ABT
{
    public sealed partial class ArchitectureBuilder
    {
        private readonly string _apiControllers;
        private readonly string _apiExtensions;

        private readonly string _apiProjectNamespace;
        private readonly string _apiProperties;
        private readonly string _connectionString;

        private readonly string _data;
        private readonly string _dataContexts;
        private readonly string _dataIRepositories;
        private readonly string _dataRepositories;

        private readonly string _domain;
        private readonly string _domainCommons;
        private readonly string _domainEntities;
        private readonly EfContext _efContext;
        private readonly bool _includeApiProject;
        private readonly string _path;

        private readonly string _service;
        private readonly string _serviceDtos;
        private readonly string _serviceExceptions;
        private readonly string _serviceExtensions;
        private readonly string _serviceHelpers;
        private readonly string _serviceInterfaces;
        private readonly string _serviceMappers;
        private readonly string _serviceMiddlewares;
        private readonly string _serviceServices;
        private readonly string _serviceViewModels;
        private readonly string _solutionName;

        public ArchitectureBuilder(EfContext efContext, string connectionString, bool includeApiProject = false)
        {
            _includeApiProject = includeApiProject;
            _efContext = efContext;
            _connectionString = connectionString;
            _path = GetSolutionDir() + "/";
            _solutionName = GetSolutionFileName();

            _domain = Path.Combine(_path, _solutionName + ".Domain");
            _domainEntities = Path.Combine(_domain, "Entities");
            _domainCommons = Path.Combine(_domain, "Commons");

            _data = Path.Combine(_path, _solutionName + ".Data");
            _dataContexts = Path.Combine(_data, "Contexts");
            _dataIRepositories = Path.Combine(_data, "IRepositories");
            _dataRepositories = Path.Combine(_data, "Repositories");

            _service = Path.Combine(_path, _solutionName + ".Service");
            _serviceDtos = Path.Combine(_service, "DTOs");
            _serviceViewModels = Path.Combine(_service, "ViewModels");
            _serviceInterfaces = Path.Combine(_service, "Interfaces");
            _serviceExceptions = Path.Combine(_service, "Exceptions");
            _serviceExtensions = Path.Combine(_service, "Extensions");
            _serviceMiddlewares = Path.Combine(_service, "Middlewares");
            _serviceMappers = Path.Combine(_service, "Mappers");
            _serviceHelpers = Path.Combine(_service, "Helpers");
            _serviceServices = Path.Combine(_service, "Services");

            if (includeApiProject)
            {
                _apiProjectNamespace = Path.Combine(_path, _solutionName + ".Api");
                _apiControllers = Path.Combine(_apiProjectNamespace, "Controllers");
                _apiExtensions = Path.Combine(_apiProjectNamespace, "Extensions");
                _apiProperties = Path.Combine(_apiProjectNamespace, "Properties");
            }

            ConfigureSolution();
            CreateFolders();
            CreateCsProjects();
            CreateDataFiles();
        }

        private void CreateFolders()
        {
            var foldersAsList = new List<string>
            {
                _domain, _domainEntities, _domainCommons,
                _data, _dataContexts, _dataIRepositories, _dataRepositories,
                _service, _serviceDtos, _serviceInterfaces, _serviceServices, _serviceViewModels,
                _serviceExceptions, _serviceMappers, _serviceMiddlewares, _serviceHelpers, _serviceExtensions
            };

            if (_includeApiProject)
            {
                foldersAsList.Add(_apiProjectNamespace!);
                foldersAsList.Add(_apiControllers!);
                foldersAsList.Add(_apiExtensions!);
                foldersAsList.Add(_apiProperties!);
            }

            for (var index = 0; index < foldersAsList.Count; index++)
            {
                var folder = foldersAsList[index];

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            }
        }

        private void CreateCsProjects()
        {
            var domainCsproj = Path.Combine(_domain, _solutionName + ".Domain.csproj");
            File.WriteAllText(domainCsproj, Resources.DomainLayerCsProj());

            var dataCsproj = Path.Combine(_data, _solutionName + ".Data.csproj");
            File.WriteAllText(dataCsproj, Resources.DataLayerCsProj(_solutionName, _efContext));

            var serviceCsproj = Path.Combine(_service, _solutionName + ".Service.csproj");
            File.WriteAllText(serviceCsproj, Resources.ServiceLayerCsProj(_solutionName));
        }

        private void CreateDomainFiles(Type entity)
        {
            #region Creating Entity

            var csFile = entity.Name + ".cs";
            var path = _path + entity.Assembly.FullName!.Split(", ").First();
            var tSourcePath = Directory.GetFiles(path, csFile, SearchOption.AllDirectories).First();

            var content = string.Join('\n', File.ReadAllLines(tSourcePath)
                .Select(l => l.Contains("namespace") ? $"namespace {_solutionName}.Domain.Entities" : l));

            File.WriteAllText(Path.Combine(_domainEntities, csFile), content);

            #endregion

            #region Pagination Params and MetaData

            File.WriteAllText(Path.Combine(_domainCommons, "PaginationParameters.cs"),
                Resources.PaginationParameters(_solutionName));

            File.WriteAllText(Path.Combine(_domainCommons, "PaginationMetaData.cs"),
                Resources.PaginationMetaData(_solutionName));

            #endregion

            #region Base Error Details

            File.WriteAllText(Path.Combine(_domainCommons, "ErrorDetails.cs"),
                Resources.ErrorDetails(_solutionName));

            #endregion
        }

        private void CreateDataFiles()
        {
            #region I + Repositories

            File.WriteAllText(Path.Combine(_dataIRepositories, "IGenericRepository.cs"),
                Resources.GenericRepositoryInterface(_solutionName));

            File.WriteAllText(Path.Combine(_dataRepositories, "GenericRepository.cs"),
                Resources.GenericRepository(_solutionName));

            #endregion

            #region I + UnitOfWork

            File.WriteAllText(Path.Combine(_dataIRepositories, "IUnitOfWork.cs"),
                Resources.UnitOfWorkInterface(_solutionName));

            File.WriteAllText(Path.Combine(_dataRepositories, "UnitOfWork.cs"),
                Resources.UnitOfWork(_solutionName));

            #endregion

            #region Context

            File.WriteAllText(Path.Combine(_dataContexts, $"{_solutionName}DbContext.cs"),
                Resources.DatabaseContext(_solutionName, _efContext, _connectionString));

            #endregion
        }

        private void CreateServiceFiles(string modelName, Type forCreation, Type viewModel)
        {
            #region CreationDto

            var csFile = forCreation.Name + ".cs";
            var path = _path + forCreation.Assembly.FullName!.Split(", ").First();
            var tForCreationPath = Directory.GetFiles(path, csFile, SearchOption.AllDirectories).First();

            var content = string.Join('\n',
                File.ReadAllLines(tForCreationPath)
                    .Select(l => l.Contains("namespace") ? $"namespace {_solutionName}.Service.DTOs" : l));

            File.WriteAllText(Path.Combine(_serviceDtos, csFile), content);

            #endregion

            #region ViewModel

            if (viewModel is not null)
            {
                csFile = viewModel.Name + ".cs";
                path = _path + viewModel.Assembly.FullName!.Split(", ").First();
                var tViewModelPath = Directory.GetFiles(path, csFile, SearchOption.AllDirectories).First();

                content = string.Join('\n', File.ReadAllLines(tViewModelPath)
                    .Select(l => l.Contains("namespace") ? $"namespace {_solutionName}.Service.ViewModels" : l));

                File.WriteAllText(Path.Combine(_serviceViewModels, csFile), content);
            }

            #endregion

            #region Services

            File.WriteAllText(Path.Combine(_serviceInterfaces, $"I{modelName}Service.cs"),
                Resources.ServiceInterface(_solutionName, modelName));

            File.WriteAllText(Path.Combine(_serviceServices, $"{modelName}Service.cs"),
                Resources.Service(_solutionName, modelName));

            #endregion

            #region MappingProfile

            var mapperPath = Path.Combine(_serviceMappers, "MapperProfile.cs");

            if (!File.Exists(mapperPath))
                File.WriteAllText(mapperPath, Resources.MappingProfile(_solutionName));

            content = File.ReadAllText(mapperPath)[..^8];
            if (!content.Contains($"<{modelName}, {forCreation.Name}>"))
                content += $"\t\t\tCreateMap<{modelName}, {forCreation.Name}>().ReverseMap();\n";

            if (viewModel is not null && !content.Contains($"<{modelName}, {viewModel.Name}>"))
            {
                if (!content.Contains(".Service.ViewModels;"))
                    content = $"using {_solutionName}.Service.ViewModels;\n" + content;

                content += $"\t\t\tCreateMap<{modelName}, {viewModel.Name}>().ReverseMap();\n";
            }

            File.WriteAllText(mapperPath, content + "\t\t}\n\t}\n}");

            #endregion

            #region Extensions | Helpers | Exceptions | Middlewares

            File.WriteAllText(Path.Combine(_serviceExtensions, "CollectionExtensions.cs"),
                Resources.CollectionExtensions(_solutionName));

            File.WriteAllText(Path.Combine(_serviceHelpers, "HttpContextHelper.cs"),
                Resources.HttpContextHelper(_solutionName));

            File.WriteAllText(Path.Combine(_serviceExceptions, "HttpStatusCodeException.cs"),
                Resources.HttpStatusCodeException(_solutionName));

            File.WriteAllText(Path.Combine(_serviceMiddlewares, "CustomExceptionMiddleware.cs"),
                Resources.CustomExceptionMiddleware(_solutionName));

            #endregion
        }

        private void ConfigureSolution()
        {
            var path = GetSolutionPath();
            var oldSlnContent = File.ReadAllText(path);

            if (!File.Exists(path.Replace(".sln", ".originalsln")))
                File.WriteAllText(path.Replace(".sln", ".originalsln"), oldSlnContent);

            var otherProjects = GetOtherProjectNames();

            var content = Resources.SolutionSln(_solutionName, otherProjects.First(p =>
                p != $"{_solutionName}.Data" && p != $"{_solutionName}.Domain" && p != $"{_solutionName}.Service")!);

            for (var index = 0; index < otherProjects.Length; index++)
            {
                var otherProject = otherProjects[index];

                if (!content.Contains($"\"{otherProject}\""))
                    content = Resources.AddNewReference(content, otherProject!);
            }

            File.WriteAllText(path, content);
        }

        public void Create<TEntity, TForCreation, TViewModel>()
        {
            Create(typeof(TEntity), typeof(TForCreation), typeof(TViewModel));
        }

        public void Create<TEntity, TForCreation>()
        {
            Create(typeof(TEntity), typeof(TForCreation), null);
        }

        private void Create(Type entity, Type forCreation, Type viewModel)
        {
            // Entity
            CreateDomainFiles(entity);

            // CreationDto, ViewModel
            CreateServiceFiles(entity.Name, forCreation, viewModel);

            // Adding new model's repository to unitOfWork
            AddRepositoryToUnitOfWork(entity.Name);
        }

        private void AddRepositoryToUnitOfWork(string modelName)
        {
            #region IUnitOfWork

            var repos = File.ReadAllText(Path.Combine(_dataIRepositories, "IUnitOfWork.cs"));

            repos = (repos.Contains($"using {_solutionName}.Domain.Entities;")
                ? ""
                : $"using {_solutionName}.Domain.Entities;\n") + repos;

            repos = new string(repos.Take(repos.Length - 30).ToArray()) +
                    $"\tIGenericRepository<{modelName}> {modelName}s {{ get; }}\n\t\tTask SaveChangesAsync();\n\t}}\n}}";
            
            File.WriteAllText(Path.Combine(_dataIRepositories, "IUnitOfWork.cs"), repos);

            #endregion

            #region UnitOfWork

            var s = File.ReadAllText(Path.Combine(_dataRepositories, "UnitOfWork.cs"));

            var a = s.Split('\n').ToList();

            if (!a.Any(p => p.Equals($"using {_solutionName}.Domain.Entities;")))
                a.Insert(0, $"using {_solutionName}.Domain.Entities;");

            a.Insert(12, $"\t\tpublic IGenericRepository<{modelName}> {modelName}s {{ get; }}");
            a.Insert(a.Count - 8, $"\t\t\t{modelName}s = new GenericRepository<{modelName}>(dbContext);");

            s = string.Join("\n", a);

            File.WriteAllText(Path.Combine(_dataRepositories, "UnitOfWork.cs"), s);

            #endregion
        }

        #region Getters

        private string GetAssemblyDir()
        {
            return Path.GetDirectoryName(GetAssemblyPath())!;
        }

        private string GetAssemblyPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        private string[] GetOtherProjectNames()
        {
            var result = new List<string>();

            foreach (var dir in Directory.GetDirectories(_path))
                if (Directory.GetFiles(dir).Any(p => p.Contains(".csproj")))
                    result.Add(dir);

            return result.Select(Path.GetFileName).ToArray();
        }

        private string GetSolutionDir()
        {
            return Directory.GetParent(GetSolutionPath())!.FullName;
        }

        private string GetSolutionFileName()
        {
            var path = GetSolutionPath();

            return path.Split(path.Contains('/') ? '/' : '\\').Last()[..^4];
        }

        private string GetSolutionPath()
        {
            var currentDirPath = GetAssemblyDir();
            while (currentDirPath != null)
            {
                var fileInCurrentDir = Directory.GetFiles(currentDirPath).Select(f => f.Split(@"\").Last()).ToArray();
                var solutionFileName =
                    fileInCurrentDir.SingleOrDefault(f =>
                        f.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase));
                if (solutionFileName != null)
                    return Path.Combine(currentDirPath, solutionFileName);

                currentDirPath = Directory.GetParent(currentDirPath)?.FullName;
            }

            throw new FileNotFoundException("Cannot find solution file path");
        }

        #endregion
    }
}