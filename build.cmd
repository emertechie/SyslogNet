@echo off

pushd %~dp0

dotnet build --configuration Release

pushd SyslogNet.Client.Tests
dotnet xunit --configuration Release
popd

pushd SyslogNet.Client
dotnet pack --configuration Release
popd

popd