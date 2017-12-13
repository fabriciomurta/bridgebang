using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BridgeBang
{
    public class CsProj
    {
        public string Name { get; }
        public string PjPath { get; }
        public string CsPath { get; }
        public Guid Guid { get; }
        public string GuidUC { get; }
        public string GuidLC { get; }
        public List<CsProj> References { get; }

        public CsProj(string name)
        {
            Name = name;
            PjPath = Path.Combine(name, name + ".csproj");
#if GITHUB
            CsPath = Path.Combine(name, name + ".cs");
#else
            CsPath = Path.Combine(name, "Class1.cs");
#endif
            Guid = Guid.NewGuid();
            GuidUC = Guid.ToString().ToUpperInvariant();
            GuidLC = Guid.ToString();
            References = new List<CsProj>();
        }

        public void Dump(string rootPath, string bridgeVersion, bool projectRefs = true)
        {
            var pjDir = Path.Combine(rootPath, Name);
            if (Directory.Exists(pjDir)) throw new Exception("Project directory '" + pjDir + "' already exists.");

            Directory.CreateDirectory(Path.Combine(rootPath, Name));

            File.WriteAllText(Path.Combine(rootPath, CsPath), GetCs());
            File.WriteAllText(Path.Combine(rootPath, PjPath), GetCsProj(bridgeVersion, projectRefs));

#if GITHUB
#else
            Directory.CreateDirectory(Path.Combine(pjDir, "Properties"));
            File.WriteAllText(Path.Combine(pjDir, "Properties", "AssemblyInfo.cs"), GetAsmi());
            File.WriteAllText(Path.Combine(pjDir, "bridge.json"), GetBridgeJson());
            File.WriteAllText(Path.Combine(pjDir, "packages.config"), GetPkgConfig(bridgeVersion));
#endif
        }

        string GetCsProj(string bridgeVersion, bool projectRefs)
        {
#if GITHUB
            return @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net40</TargetFramework>
    <NoStdLib>true</NoStdLib>
    <DisableImplicitFrameworkReferences>true</DisableImplicitFrameworkReferences>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <PackagesDir>..\packages</PackagesDir>
  </PropertyGroup>
" + (projectReferences.Count > 0 ?
@"  <ItemGroup>
" + string.Join("", projectReferences.Select(p =>

@"    <ProjectReference Include=""../" + p.Name + @""" />
")) + 
@"  </ItemGroup>" : "") + 
@"
  <ItemGroup>
    <PackageReference Include=""Bridge"">
      <Version>" + bridgeVersion + @"</Version>
    </PackageReference>
    <PackageReference Include=""Bridge.Html5"">
      <Version>" + bridgeVersion + @"</Version>
    </PackageReference>
  </ItemGroup>
</Project>";
#else
            var bridge_ver_fields = bridgeVersion.Split('.');
            var bridge_major_version = bridge_ver_fields[0] + "." + bridge_ver_fields[1] + ".0";
            var shouldpjref = projectRefs && References.Count > 0;
            var shoulddllref = !projectRefs && References.Count > 0;

            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>{" + GuidUC + @"}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>" + Name + @"</RootNamespace>
    <AssemblyName>" + Name + @"</AssemblyName>
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <NuGetPackageImportStamp>
    </NuGetPackageImportStamp>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <NoStdLib>true</NoStdLib>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <NoStdLib>true</NoStdLib>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""Bridge, Version=" + bridge_major_version + @".0, Culture=neutral, processorArchitecture=MSIL"">
      <HintPath>..\packages\Bridge.Core." + bridgeVersion + @"\lib\net40\Bridge.dll</HintPath>
    </Reference>
    <Reference Include=""Bridge.Html5, Version=" + bridge_major_version + @".0, Culture=neutral, processorArchitecture=MSIL"">
      <HintPath>..\packages\Bridge.Html5." + bridgeVersion + @"\lib\net40\Bridge.Html5.dll</HintPath>
    </Reference>" + (shoulddllref ? string.Join("", References.Select( r => @"
    <Reference Include=""" + r.Name + @""">
      <HintPath>..\" + r.Name + @"\bin\Debug\" + r.Name + @".dll</HintPath>
    </Reference>")) : "") + @"
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Class1.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
  </ItemGroup>
  <ItemGroup>
    <None Include=""bridge.json"" />
    <None Include=""packages.config"" />
  </ItemGroup>" + (shouldpjref ? @"
  <ItemGroup>" + string.Join("", References.Select(r => @"
    <ProjectReference Include=""..\" + r.PjPath + @""">
      <Project>{" + r.Guid.ToString() + @"}</Project>
      <Name>" + r.Name + @"</Name>
    </ProjectReference>")) + @"
  </ItemGroup>" : "") + @"
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
  <Import Project=""..\packages\Bridge.Min." + bridgeVersion + @"\build\Bridge.Min.targets"" Condition=""Exists('..\packages\Bridge.Min." + bridgeVersion + @"\build\Bridge.Min.targets')"" />
  <Target Name=""EnsureNuGetPackageBuildImports"" BeforeTargets=""PrepareForBuild"">
    <PropertyGroup>
      <ErrorText>This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {0}.</ErrorText>
    </PropertyGroup>
    <Error Condition=""!Exists('..\packages\Bridge.Min." + bridgeVersion + @"\build\Bridge.Min.targets')"" Text=""$([System.String]::Format('$(ErrorText)', '..\packages\Bridge.Min." + bridgeVersion + @"\build\Bridge.Min.targets'))"" />
  </Target>" + (shoulddllref ? @"
  <PropertyGroup>
    <PreBuildEvent>
@rem post-build event commandline.
@echo off
if not exist ""$(SolutionDir)\" + References[0].Name + @"\bin\$(ConfigurationName)\" + References[0].Name + @".dll"" (
 echo ""Should build the sub-projects...""
 ""$(MSBuildToolsPath)\msbuild.exe"" /p:Configuration=""$(ConfigurationName)"" ""$(SolutionDir)\SubProjectsOnly.sln""
)
    </PreBuildEvent>
  </PropertyGroup> " : "") + @"
</Project>
";
#endif
        }

        string GetCs()
        {
#if GITHUB
            return @"namespace Namespace" + Name + "{ public class Type" + Name + " { }}";
#else
            return @"namespace " + Name + @"
{
    public class Class1
    {" + (References.Count > 0 ? string.Join("", References.Select(r => @"
        public " + r.Name + ".Class1 Prop" + r.Name + @" { get; set; }")) : "") + @"
    }
}";
#endif
        }

        string GetAsmi()
        {
            return @"using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""" + Name + @""")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""" + Name + @""")]
[assembly: AssemblyCopyright(""Copyright ©  2017"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""" + GuidLC + @""")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]
[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";
        }

        static string GetBridgeJson()
        {
            return @"// See all bridge.json configuration options at:
// https://github.com/bridgedotnet/Bridge/wiki/global-configuration

{
  // The folder to output JavaScript (.js) files.
  ""output"": ""$(OutDir)/bridge/"",

  // Set to ""Minified"" to generate .min.js files.
  // Set to ""Both"" to generate both minified and non-minified .js files.
  // ""Formatted"" generates non-minified .js files.
  ""outputFormatting"": ""Formatted"",

  // Enable the Bridge Console.
  // Default is false.
  ""console"": {
    ""enabled"": true
  },

  // Enable browser debugging of C# files.
  // Default is false.
  ""sourceMap"": {
    ""enabled"": true
  },

  // Set to true to disable Reflection metadata generation.
  // Default is false.
  ""reflection"": {
    ""disabled"": false
  },

  // Generate TypeScript Definition (.d.ts) files.
  // Default is false.
  ""generateTypeScript"": false,

  // Delete everything from the output folder
  // Default is false
  ""cleanOutputFolderBeforeBuild"": false,

  // Set to true to enable bridge.report.log generation.
  // Default is false.
  ""report"": {
    ""enabled"": false
  },

  // Rules to manage generated JavaScript syntax.
  // Default is ""Managed""
  ""rules"": {
    ""anonymousType"": ""Plain"",
    ""arrayIndex"": ""Managed"",
    ""autoProperty"": ""Plain"",
    ""boxing"": ""Managed"",
    ""integer"": ""Managed"",
    ""lambda"": ""Plain""
  }
}
";
        }

        static string GetPkgConfig(string version)
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""Bridge"" version=""" + version + @""" targetFramework=""net40"" />
  <package id=""Bridge.Core"" version=""" + version + @""" targetFramework=""net40"" />
  <package id=""Bridge.Html5"" version=""" + version + @""" targetFramework=""net40"" />
  <package id=""Bridge.Min"" version=""" + version + @""" targetFramework=""net40"" />
</packages>";
        }
    }

    class Program
    {
        public static string GetSln(List<CsProj> csprojs)
        {
#if GITHUB
            return @"Microsoft Visual Studio Solution File, Format Version 12.00
\# Visual Studio 15
VisualStudioVersion = 15.0.27004.2010
MinimumVisualStudioVersion = 10.0.40219.1
" + string.Join("", csprojs.Select(p => 

@"Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = """ + p.Name + @""", """ + p.PjPath + @""", ""{" + p.GuidUC + @"}""
EndProject
")) +

@"Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
" + string.Join("", projects.Select(p =>

@"		{" + p.GuidUC + @"}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{" + p.GuidUC + @"}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{" + p.GuidUC + @"}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{" + p.GuidUC + @"}.Release|Any CPU.Build.0 = Release|Any CPU
")) +
@"	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal";
#else
            return @"
Microsoft Visual Studio Solution File, Format Version 12.00
\# Visual Studio 15
VisualStudioVersion = 15.0.26403.7
MinimumVisualStudioVersion = 10.0.40219.1
" + string.Join("", csprojs.Select(p =>

@"Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = """ + p.Name + @""", """ + p.PjPath + @""", ""{" + p.GuidUC + @"}""
EndProject
")) +

@"Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
" + string.Join("", csprojs.Select(p =>

@"		{" + p.GuidUC + @"}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{" + p.GuidUC + @"}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{" + p.GuidUC + @"}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{" + p.GuidUC + @"}.Release|Any CPU.Build.0 = Release|Any CPU
")) +
@"	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal";

#endif
        }

        static void Generate(string rootPath, int count, string masterBridgeVersion, string secBridgeVersion)
        {
            if (Directory.Exists(rootPath))
            {
                Console.WriteLine("Directory '" + rootPath + "' already exists. Skipping.");
                return;
            }

            Console.Write(@"Creating scenario: " + rootPath + @"
 - Referenced projects: " + count + @"
 - Main project's bridge version: " + masterBridgeVersion + @"
 - Reference projects' bridge version: " + secBridgeVersion + @"
Generating project set: ");
            var csprojs = new List<CsProj>();
            var masterCode = new StringBuilder();
            masterCode.AppendLine(@"namespace Main
{
    public class Class1
        {");
            for (var i = 0; i < count; i++)
            {
                var project = new CsProj("Project" + i.ToString());

                var csprojFile = project.PjPath;


                project.Dump(rootPath, secBridgeVersion);
                csprojs.Add(project);
            }
            masterCode.AppendLine(@"    }
}");

            var masterProj = new CsProj("Main");
            masterProj.References.AddRange(csprojs);
            masterProj.Dump(rootPath, masterBridgeVersion);

            var solution = new List<CsProj>(csprojs);
            solution.Add(masterProj);

            File.WriteAllText(Path.Combine(rootPath, "Scenario.sln"), GetSln(solution));

            // This project will link to the built DLLs, so that we don't require
            // rebuilding everything all the time. Should first build the 'Scenario.sln'
            // once, before this will work.
            var statiMasterProj = new CsProj("StaticMain");
            statiMasterProj.References.AddRange(csprojs);
            statiMasterProj.Dump(rootPath, masterBridgeVersion, false);
            File.WriteAllText(Path.Combine(rootPath, "StaticScenario.sln"), GetSln(new List<CsProj>() { statiMasterProj }));

            // This solution will only have the subprojects. It is used to build all
            // references if they are not while the static solution is built.
            File.WriteAllText(Path.Combine(rootPath, "SubProjectsOnly.sln"), GetSln(csprojs));

            Console.WriteLine("done.");
        }

        static void Main(string[] args)
        {
            // Ok, simply to check against the manually created project.
            //Generate("../scenarios/Vanilla1", 1, "16.5.1", "16.5.1");

            // Ok, runs fine in current 16.5.1 branch (without any out-of-memory fixes).
            //Generate("../scenarios/Vanilla100", 100, "16.5.1", "16.5.1");

            Generate("../scenarios/Vanilla250", 250, "16.5.1", "16.5.1");

            // Scenario 1: reference X assemblies where all are same version
            //Generate("../scenarios/Scenario1", 250, "16.4.1", "16.4.1");
            //Generate("../scenarios/Scenario2", 250, "16.5.0", "16.4.1");
        }
    }
}
