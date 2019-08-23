using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CommonModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Extensions.Options;
using TestAPI.Utils;

namespace TestAPI.Controllers
{
    internal sealed class AssemblyResolver : IDisposable
    {
        private readonly ICompilationAssemblyResolver assemblyResolver;
        private readonly DependencyContext dependencyContext;
        private readonly AssemblyLoadContext loadContext;

        public AssemblyResolver(string path)
        {
            this.Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);

            this.dependencyContext = DependencyContext.Load(this.Assembly);

            this.assemblyResolver = new CompositeCompilationAssemblyResolver
                                    (new ICompilationAssemblyResolver[]
            {
            new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
            new ReferenceAssemblyPathResolver(),
            new PackageCompilationAssemblyResolver()
            });

            this.loadContext = AssemblyLoadContext.GetLoadContext(this.Assembly);
            this.loadContext.Resolving += OnResolving;
        }

        public Assembly Assembly { get; }

        public void Dispose()
        {
            this.loadContext.Resolving -= this.OnResolving;
        }

        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            bool NamesMatch(RuntimeLibrary runtime)
            {
                return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
            }

            RuntimeLibrary library =
                this.dependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
            if (library != null)
            {
                var wrapper = new CompilationLibrary(
                    library.Type,
                    library.Name,
                    library.Version,
                    library.Hash,
                    library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                    library.Dependencies,
                    library.Serviceable);

                var assemblies = new List<string>();
                this.assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);
                if (assemblies.Count > 0)
                {

                    return this.loadContext.LoadFromAssemblyPath(assemblies[0]);
                }
            }

            return null;
        }
    }
    public class FcdsAssemblyInfo
    {
        public List<string> AssembliesList = new List<string>
        {
            "CommonModels.dll",
            "CommonLibrary.dll"
        };
    }
    public class FcdsAssemblyResolver
    {
        public FcdsAssemblyResolver(string path, string version, FcdsAssemblyInfo fcdsAssemblyInfo = null)
        {
            if (!string.IsNullOrEmpty(path)) Path = path;
            if (!string.IsNullOrEmpty(Version)) Version = version;
            if (fcdsAssemblyInfo == null) fcdsAssemblyInfo = new FcdsAssemblyInfo();


            foreach (var dllname in fcdsAssemblyInfo.AssembliesList)
            {
                AssemblyResolvers.Add(dllname.Remove(dllname.Length - 4), new AssemblyResolver(System.IO.Path.Combine(Path, "", dllname)));
            }
        }

        private string Path { get; set; } = @"/efs/";
        private string Version { get; set; } = @"";
        internal Dictionary<string, AssemblyResolver> AssemblyResolvers { get; set; } = new Dictionary<string, AssemblyResolver>();
    }

    [Produces("application/json")]
    [Route("api/Base")]
    public class BaseController : Controller
    {

        public static List<string> lstMessages = new List<string>();


        public readonly IConfiguration configuration;

        public BaseController(IConfiguration configuration)
        {
            lstMessages.Add("Constructor BaseController");
            this.configuration = configuration;
            LoadLibrary();
        }
        public ICommonTest icommonTest;

        public ICommonTest icommonTest2;
        public void LoadLibrary()
        {         
            try
            {

                lstMessages.Add("Base Directory" + AppDomain.CurrentDomain.BaseDirectory);

                var rootDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

                lstMessages.Add(" rootDir " + rootDir);

                var listAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                lstMessages.Add(" listAssemblies count = " + listAssemblies.Count());



                lstMessages.Add("Loaded assemblies from current domain  ---------------------------------------------------------");
                string AssemblyPath = "/efs/CommonLibrary.dll";// configuration["AssemblyPath"];



                AssemblyResolver resolver = new AssemblyResolver("/efs/CommonModels.dll");

                foreach (TypeInfo type in resolver.Assembly.DefinedTypes)
                {
                    lstMessages.Add("CommonModels.type.name = " + type.Name);
                }

                resolver = new AssemblyResolver(AssemblyPath);

                foreach (TypeInfo type in resolver.Assembly.DefinedTypes)
                {
                    lstMessages.Add("commonlib.type.name = " + type.Name);
                }

                var samplemessagev1 = resolver.Assembly.GetType("CommonLibrary.CommonTestClass");
                dynamic obj = Activator.CreateInstance(samplemessagev1);

                if (obj == null)
                {
                    lstMessages.Add("  #####********Activator.CreateInstance********* null instance");
                }

                lstMessages.Add("CommonLibrary.CommonTestClass =  " + obj.CommonTestMethod());

                foreach (Type type in resolver.Assembly.GetTypes())
                {
                    lstMessages.Add(" type :" + type.FullName);
                    if (type != null && type.IsClass && type.FullName == "CommonLibrary.CommonTestClass")
                    {
                        var p = type.GetType();
                        icommonTest = Activator.CreateInstance(type) as ICommonTest;
                        lstMessages.Add("created instance");

                        icommonTest2 = resolver.Assembly.CreateInstance(type.FullName) as ICommonTest;
                        break;
                    }
                }
                               

                if (icommonTest == null)
                {
                    lstMessages.Add("  #####*****icommonTest************ null instance");
                }
                else
                {
                    lstMessages.Add("icommonTest-1 =   " + icommonTest.CommonTestMethod());
                }

                if (icommonTest2 == null)
                {
                    lstMessages.Add("  #####*****icommonTest2************ null instance");
                }
                else
                {
                    lstMessages.Add("icommonTest-2 =   " + icommonTest2.CommonTestMethod());
                }

                






                //FcdsAssemblyResolver fcdsAssemblyResolver = new FcdsAssemblyResolver("/efs/", "");
                //var samplemessagev1 = fcdsAssemblyResolver.AssemblyResolvers["CommonLibrary"].Assembly.GetType("CommonLibrary.CommonTestClass");
                //dynamic obj = Activator.CreateInstance(samplemessagev1);
                //icommonTest = obj as ICommonTest;


                // //////////////////////////////////////////

                //lstMessages.Add("configuration[\"AssemblyPath\"] : " + AssemblyPath);
                //var assemblyName = AssemblyName.GetAssemblyName(AssemblyPath);

                //lstMessages.Add("assemblyName: " + assemblyName);
                ////var assembly = listAssemblies.FirstOrDefault(e => e.FullName == assemblyName.FullName);
                ////if(assembly == null)

                //    lstMessages.Add("loading assemply" + AssemblyPath);
                //   var assembly = Assembly.Load(System.IO.File.ReadAllBytes("/efs/CommonModels.dll"));
                //    assembly = Assembly.Load(System.IO.File.ReadAllBytes(AssemblyPath));
                //    lstMessages.Add("loaded assemply" + AssemblyPath);


                //var ins = assembly.CreateInstance("CommonLibrary.CommonTestClass");
                //icommonTest = ins as ICommonTest;

                //if(ins == null)
                //{
                //    lstMessages.Add("null instance");
                //}


                //////////////////////////////////////////////////////




                //var types = assembly.GetType("CommonLibrary.CommonTestClass");// ("SampleLibrary.TestClass");
                // icommonTest = Activator.CreateInstance(types, null) as ICommonTest;
                //foreach (var type in types)
                //{
                //    lstMessages.Add(" type :" + type.FullName);
                //    if (type != null && type.IsClass && typeof(ICommonTest).IsAssignableFrom(type))
                //    {
                //        icommonTest = Activator.CreateInstance(type, null) as ICommonTest;
                //        lstMessages.Add("created instance");
                //        break;
                //    }
                //}
            }
            catch (Exception ex)
            {
                lstMessages.Add(ex.Message);
               
            }
            finally
            {
                //TODO dispose if any
            }
        }
    }
}