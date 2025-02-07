# Create Resource

## Create a project with Visual Studio 19 (Windows)

* Go to "File -> New -> Project..." now the Project Wizard should appear.
* In the left Column select "Installed -> Visual C# -> .NET".
* Now select "Class Library (.NET)" and choose "Name", "Location" and the "Solution name".
* To setup the correct NuGet Packages open the Manager under "Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution..."
* Select Browse and search for AltV.Net and install the packages "AltV.Net", ("AltV.Net.Async" when you need async thread save api access)
* Now go to "Project -> {Your Project Name} Properties... -> Build", here you can select the Output path where the dll should be saved.

See https://docs.microsoft.com/en-us/visualstudio/deployment/quickstart-deploy-to-local-folder?view=vs-2019 for automatically publish it in your resource folder or see the boilerplate project file.

Boilerplate YourProject.csproj:
```
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net6.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
      <!--Use latest version from https://www.nuget.org/packages/AltV.Net-->
      <PackageReference Include="AltV.Net" Version="9.0.2" />
    </ItemGroup>
    
    <!--This copies the publish directory to the resource folder which is named "my-server"-->
    
    <ItemGroup>
        <AllOutputFiles Include="$(OutputPath)\publish\*.*" />
    </ItemGroup>

    <Target Name="CopyFiles" AfterTargets="publish">
        <PropertyGroup>
            <CopiedFiles>$(OutputPath)\publish\*.*</CopiedFiles>

            <TargetLocation Condition=" '$(Configuration)' == 'Release' ">../../my-server/</TargetLocation>
        </PropertyGroup>
        <Copy Condition=" '$(TargetLocation)' != '' " SourceFiles="@(AllOutputFiles)" DestinationFolder="$(TargetLocation)" SkipUnchangedFiles="false" />
    </Target>

</Project>
```

You now have to create a single resource file in your project that is auto initialized on server startup.

MyResource.cs
```csharp
using System;
using AltV.Net.Client;

namespace My.Package
{
    internal class MyResource : Resource
    {
        public override void OnStart()
        {
            Console.WriteLine("Started");
        }

        public override void OnStop()
        {
            Console.WriteLine("Stopped");
        }
    }
}
```

## Compile the resource

To compile the resource from the command line use ```dotnet publish -c Release```

This will output the resource dll and all other dependencies including AltV.Net.dll in the yourresource/bin/Release/net5.0/publish folder.
Copy the dlls to the server resource folder ```altv-server/resources/{YourResourceName}/```.

To get the Resource running on the server, you have to create a "resource.cfg" file.

```
client-type: "csharp",
client-main: "path/to/YourProject.dll",
client-files: [
    path/to/folder/with/dlls
]
```

Now the resource needs to be added to the server.cfg.

```
resources: [
"{YourResourceName}"
]
```

Your server folder now look similar to this one

```
modules/
└── csharp-module.dll
resources/
└── my-example-csharp-resource/
    ├── Alt.Net.Example.dll
    ├── resource.cfg
    └── ... (any .dll dependency like "AltV.Net.dll", "mysql.dll", ...)
AltV.Net.Host.dll
AltV.Net.Host.runtimeconfig.json
server.cfg
altv-server.exe
```
