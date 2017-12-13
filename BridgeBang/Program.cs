using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BridgeBang
{
    class Program
    {
        static string GetCsProj(string bridgeVersion, List<string> projectReferences)
        {
            return @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net47</TargetFramework>
    <NoStdLib>true</NoStdLib>
    <DisableImplicitFrameworkReferences>true</DisableImplicitFrameworkReferences>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>
" + (projectReferences.Count > 0 ?
@"  <ItemGroup>
" + string.Join("", projectReferences.Select(p =>

@"    <ProjectReference Include=""../" + p + @""" />
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
        }

        static string GetCs(string postfix)
        {
            return @"namespace Namespace" + postfix + "{ public class Type" + postfix + " { }}";
        }

        static string GetSln(List<string> csprojs)
        {
            var projects = csprojs.Select(csproj => new { FileName = csproj, Name = Path.GetFileNameWithoutExtension(csproj), Guid = Guid.NewGuid() } );

            return @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 15
VisualStudioVersion = 15.0.27004.2010
MinimumVisualStudioVersion = 10.0.40219.1
" + string.Join("", projects.Select(p => 

@"Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = """ + p.Name + @""", """ + p.FileName + @""", ""{" + p.Guid.ToString().ToUpperInvariant() + @"}""
EndProject
")) +

@"Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
" + string.Join("", projects.Select(p =>

@"		{" + p.Guid.ToString().ToUpperInvariant() + @"}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{" + p.Guid.ToString().ToUpperInvariant() + @"}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{" + p.Guid.ToString().ToUpperInvariant() + @"}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{" + p.Guid.ToString().ToUpperInvariant() + @"}.Release|Any CPU.Build.0 = Release|Any CPU
")) +
@"	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal";

        }

        static void Generate(string rootPath, string masterBridgeVersion)
        {
            // Scenario 1: reference X assemblies where all are same version
            var csprojs = new List<string>();
            var masterCode = new StringBuilder();
            masterCode.AppendLine("namespace Main { public class MainMain { ");
            for (var i = 0; i < 250; i++)
            {
                var id = "Project" + i.ToString();
                var cs = GetCs(id);
                var csproj = GetCsProj("16.4.1", new List<string>());

                var csprojFile = Path.Combine(id, id + ".csproj");
                var csFile = Path.Combine(id, id + ".cs");

                Directory.CreateDirectory(Path.Combine(rootPath, id));

                File.WriteAllText(Path.Combine(rootPath, csFile), cs);
                File.WriteAllText(Path.Combine(rootPath, csprojFile), csproj);
                csprojs.Add(csprojFile);
                masterCode.AppendLine("  public Namespace" + id + ".Type" + id + " Property" + id + " { get; set; }");
            }
            masterCode.AppendLine("}}");

            var masterCsproj = GetCsProj(masterBridgeVersion, csprojs);
            Directory.CreateDirectory(Path.Combine(rootPath, "Master"));
            File.WriteAllText(Path.Combine(rootPath, "Master/Main.csproj"), masterCsproj);
            File.WriteAllText(Path.Combine(rootPath, "Master/Main.cs"), masterCode.ToString());

            csprojs.Add("Master/Main.csproj");

            var sln = GetSln(csprojs);
            File.WriteAllText(Path.Combine(rootPath, "Scenario.sln"), sln);

        }

        static void Main(string[] args)
        {
            Generate("../Scenario1", "16.4.1");
            Generate("../Scenario2", "16.5.0");
        }
    }
}
