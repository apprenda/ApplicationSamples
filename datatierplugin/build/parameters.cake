using Apprenda.Cake.Build;

public class BuildParameters : Apprenda.Cake.Build.BuildParametersBase
{
    const string _SolutionFile = "./ApprendaEFPlugin.sln";

    public BuildParameters(ICakeContext context) : base(context) { }

    public bool ForceObfuscation { get; set; }

    public bool ForceAssemblyInfo { get; set; }

    public string SolutionFile { get; set; }

    public Func<IFile, bool> NuGetPackIgnores { get; set; }

    public Func<IFileSystemInfo, bool> NuGetPackageCleanIgnores { get; set; }
    
    public AssemblyVersionManager VersionManager { get; set; }

    public IAssemblyInfoSettingsProvider AssemblyInfoSettings { get; set; }
	
    public List<FilePath> GetSampleSolutions()
    {
		return Context.GetFiles("./ContosoUniversity.*.sln", NuGetPackIgnores).ToList();
    }

    public static BuildParameters Load(ICakeContext context, BuildSystem buildSystem)
    {
        if (context == null) throw new ArgumentNullException("context");
        if (buildSystem == null) throw new ArgumentNullException("buildSystem");
		
		var buildInfo = new BuildInfo(context);
        
        return new BuildParameters(context)
        {
            SolutionFile = _SolutionFile,
            Target = context.Argument("Target", "Default"),
            Configuration = "Release",
            Platform = context.Argument("Platform", "AnyCPU"),
            ForceObfuscation = context.HasArgument("ForceObfuscation"),
            ForceAssemblyInfo = context.HasArgument("ForceAssemblyInfo"),
			BuildInfo = buildInfo,
			VersionManager = new AssemblyVersionManager(context),
            AssemblyInfoSettings = AssemblyInfoSettingsProvider.SemVer2(
                buildInfo, 
                new StoryPreReleaseNuGetPackageVersionMixin().WithCondition(MixinConditions.NotDefaultBranch)
            ),
            NuGetOptions = new NuGetPublicationOptions
            {
                ApiKey = context.Argument("ApiKey", "")
            },
            NuGetPackageCleanIgnores = (IFileSystemInfo info) => 
            { 
                var ignores = new[]
                {
                    "/Apprenda.ILMerge",
                    "/Apprenda.BuildDeploy"
                };                
                
                var item = info.Path.FullPath;                
                return !ignores.Any(s => item.Contains(s));
            }
        };
    }
}