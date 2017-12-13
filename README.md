# bridgebang
VS Project generator to stress bridge referencing

## Overview

This project will create a solution with an arbitrary number of Bridge.NET projects referenced to a main project, with the objective to reproduce an out of memory exception being thrown by Bridge. Currently this happens consistently when the project has 250 references.

## Creating the test solution

By default this project currently creates a 250-projects-big solution, sufficient to consistently reproduce the out of memory exception while building the bridge application.

### 1. Open Solution

Open `BridgeBang\BridgeBang.sln` and build it. Then run the project. Either run with debugging or without debugging (faster) works fine.

You can now close the BridgeBang solution, unless you want to create test VS solutions with different settings. To change the generated solution, edit `BridgeBang/Program.cs` last lines (`Main()` method).

### 2. The generated project

The output test project will be then sitting under on the repo root `scenarios/testcase_name_here` (`scenarios/Vanilla250` for instance).

#### 2.1. Project files

*StaticScenario.sln*: This solution references the projects as "DLL references". This means it does not try to rebuild all the sub projects every time you rebuild the "main" project (which references to all subprojects). During build, if this solution can't find the DLL files, it attempts to build all the projects from the *SubProjectsOnly.sln* solution (below). Beware this build mode goes serial by default (only one msbuild process building the solution).

*Scenario.sln*: This solution has a main project linking to all other projects as "project references". But every time the main project is built, all sub projects are also rebuilt, so takes a while. By default it builds in parallel, so it's a good choice for first building before opening the solution above.

*SubProjectsOnly.sln*: This solution just contains the subprojects. Useful for *StaticScenario.sln* pre-build task to build all the projects without triggering the out of memory exception while trying to build the "main" project.

#### 2.2. Reproducing the issue

Once building either *StaticScenario.sln* or *Scenario.sln* with 250 or more sub-projects, the error is consistently triggered with current Bridge version.

## More information

This solution is based on the code provided by andersnm at Bridge issue at [bridgedotnet/Bridge#3259](https://github.com/bridgedotnet/Bridge/issues/3259#issuecomment-350828034).

The changes here make it so the output projects follows a VS2015 .NET 4.0 class library bridge project creation stepsj. First a project with only main + 1 sub project were created manually and, from that, the generic output was based, so that this should be very accurate to what a manually crafted project set should look like.
