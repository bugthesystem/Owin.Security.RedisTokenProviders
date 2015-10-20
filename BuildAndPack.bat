SET MsBuildPath="C:\Program Files (x86)\MSBuild\12.0\Bin"
SET NuGetExe=.nuget\nuget.exe

½NuGetExe% restore Owin.Security.RedisTokenProviders.sln 

%MsBuildPath%\MsBuild.exe build.proj

.nuget\NuGet.exe pack Owin.Security.RedisTokenProviders\Owin.Security.RedisTokenProviders.nuspec