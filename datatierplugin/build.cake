#reference "System.IO.Compression"
#addin "nuget:?package=Cake.Apprenda&version=0.4.1"
#tool "nuget:?package=xunit.runner.console"

#load "./build/parameters.cake"

using System.IO.Compression;

var Parameters = BuildParameters.Load(Context, BuildSystem);

Task("Clean-NuGet-Packages")    
    .Does(() => {
        Information("Cleaning NuGet packages directory...");
        CleanDirectory("./packages");
    });

Task("Clean")    
    .Does(() => {
        Information("Cleaning working directory...");
		CleanDirectories("./plugin");
        CleanDirectories("./src/**/bin");
        CleanDirectories("./src/**/obj");
        CleanDirectories("./tests/**/bin");
        CleanDirectories("./tests/**/obj");
    });

Task("Clean-Archives")
	.Does(() => {
		Information("Cleaning archive directories...");
		CleanDirectories("./archives");
		CleanDirectories("./temp");
	});
	
Task("Clean-All")    
    .IsDependentOn("Clean-NuGet-Packages")
    .IsDependentOn("Clean");

Task("Restore-Nuget-Packages")    
    .Does(() => {
        Information("Restoring NuGet packages...");
        NuGetRestore(Parameters.SolutionFile);
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() => {
        // be explicit about this --> .SetMSBuildPlatform(MSBuildPlatform.x86)
        // msbuild.exe from /amd64/ causes issues with SmartAssembly, see below
        /*
        error : SmartAssembly build failed:
        error : The type initializer for ' . ' threw an exception.
        ...
        warning :    at SmartAssembly.ConsoleApp.Run(String[] )
        */
        MSBuild(Parameters.SolutionFile, settings => settings
            .SetConfiguration(Parameters.Configuration)
			.UseToolVersion(MSBuildToolVersion.VS2015)
			.SetMaxCpuCount(0)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .SetMSBuildPlatform(MSBuildPlatform.x86)
            .SetNodeReuse(false) // disable node reuse to prevent locks on packages post-build 
        );
    });

Task("Clean-Build")
    .IsDependentOn("Clean-All")
    .IsDependentOn("Build");    

Task("Run-Unit-Tests")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .Does(() => {
        XUnit2("./tests/**/bin/**/*.Tests.dll");
    });

Task("Restore-Sample-Nuget-Packages")    
    .Does(() => {
        Information("Restoring Sample NuGet packages...");
		foreach(var sln in Parameters.GetSampleSolutions())
		{
			NuGetRestore(sln.FullPath);
		}
    });
	
Task("Build-Samples")
	.IsDependentOn("Restore-Sample-Nuget-Packages") 
	.Does(() => {
		foreach(var sln in Parameters.GetSampleSolutions())
		{
		     MSBuild(sln.FullPath, settings => settings
				.SetConfiguration(Parameters.Configuration)
				.UseToolVersion(MSBuildToolVersion.VS2015)
				.SetMaxCpuCount(0)
				.SetPlatformTarget(PlatformTarget.MSIL)
				.SetMSBuildPlatform(MSBuildPlatform.x86)
				.SetNodeReuse(false) // disable node reuse to prevent locks on packages post-build 
			);
		}
	});
	
Task("Build-Archives")    
    .IsDependentOn("Clean-Build")
	.IsDependentOn("Build-Samples")
    .Does(() => {
        Information("Generating archives using configuration '{0}'...", Parameters.Configuration);

		Information("Creating .archives folder");
		CreateDirectory(".archives");
        
		var acsPath = @"C:\Program Files (x86)\Apprenda\Tools\ACS\acs.exe";
		
		if (!FileExists(acsPath))
		{
			throw new Exception(string.Format("ACS was not found, do you have the Apprenda SDK installed?  Looked for ACS here: {0}", acsPath));
		}
		
		var pluginFolder = "./.plugin";
		var tempPluginFolder = "./.temp/persistence/custom";
		
        foreach(var sln in Parameters.GetSampleSolutions())
        {
			var slnName = sln.GetFilename().FullPath;
			
			//get "v#"
			var verAlias = slnName.Substring(slnName.IndexOf(".v")+1, slnName.IndexOf(".sln")-slnName.IndexOf(".v")-1);
			
			//path to the solution/version's archive
			var zipPath = string.Format(@".\.archives\ContosoUniversityArchive.{0}.zip", verAlias);
            Information("Creating archive, '{1}', for '{0}'", sln.FullPath, zipPath);
            
			var acsArgs = string.Format("NewPackage -Sln \"{0}\" -O \"{1}\" -B -PrivateRoot ContosoUniversity", sln.FullPath.Replace(@"/",@"\"), zipPath);
			
			//create the baseline archive, this will be configured for custom persistence API use after this
			Information(string.Format("Creating base image of archive"));
			Debug(string.Format("Executing: acs {0}", acsArgs));
			StartProcess(acsPath, acsArgs);
			
			Information(string.Format("Cleaning temp folder"));
			CleanDirectories(".temp");
			
			Information(string.Format("Cleaning temp folder"));
			CleanDirectories(".temp");

			Information(string.Format("Creating plugin image"));
			CopyDirectory(pluginFolder, tempPluginFolder);
			//platform plugin handling must find exactly one config file in the folder.  In this particular case the apps data assembly's config has all the necessary and equivalent entries.
			DeleteFiles(string.Format("{0}/EFPlugin.dll.config", tempPluginFolder));
			CopyDirectory(string.Format("./src/sample/{0}/ContosoUniversityData/bin", verAlias), tempPluginFolder);
			
			AddPluginToArchive(zipPath, pluginFolder);
        }       
    });

private void AddPluginToArchive(string archivePath, string pluginFolder)
{
	Information(string.Format("Adding plugin to archive"));
	using (var outputStream = new FileStream(archivePath, FileMode.Open))
	using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Update))
	{
		foreach (var file in GetFiles(string.Format("{0}/*", pluginFolder)))
		{
			var fileName = file.GetFilename().ToString();
			var archiveRelativePath = string.Format("persistence/custom/{0}", fileName);
			
			var filePath = MakeAbsolute(file).FullPath;
			Information(string.Format("Adding {0} to archive", filePath));
			archive.CreateEntryFromFile(filePath, archiveRelativePath);
		}
	}
}
	
Task("Clean-Build-Archives")    
    .IsDependentOn("Clean-Archives")
	.IsDependentOn("Build-Archives");
	
Task("Default")    
    .IsDependentOn("Clean-Build")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Clean-Build-Archives");

RunTarget(Parameters.Target);