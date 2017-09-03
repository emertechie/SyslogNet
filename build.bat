dotnet build .\SyslogNet.sln
cd .\SyslogNet.Client.Tests\
dotnet restore
dotnet xunit
cd ..
dotnet pack .\SyslogNet.Client\SyslogNet.Client.csproj