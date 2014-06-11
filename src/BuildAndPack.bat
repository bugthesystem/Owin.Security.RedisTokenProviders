SET MsBuildPath="C:\Program Files (x86)\MSBuild\12.0\Bin"
SET NuGetExe=.nuget\nuget.exe

½NuGetExe% Owin.Security.RedisTokenProviders.sln 

%MsBuildPath%\MsBuild.exe build.proj

.nuget\NuGet.exe pack OAuth.RedisRefreshTokenProvider\OAuth.RedisRefreshTokenProvider.nuspec