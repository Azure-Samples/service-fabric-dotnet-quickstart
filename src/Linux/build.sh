#!/bin/bash
DIR=`dirname $0`

cd Voting/
rm -rf pkg
mkdir -p pkg/VotingAppPkg/
cp -r ApplicationPackageRoot/ApplicationManifest.xml pkg/VotingAppPkg/ApplicationManifest.xml
cd -

cd VotingWeb/
dotnet restore -s /opt/microsoft/sdk/servicefabric/csharp/packages -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/servicefabric-lkg-release/api/v3/index.json
dotnet build
cp -r PackageRoot/ ../Voting/pkg/VotingAppPkg/VotingWebPkg/
dotnet publish -o ../Voting/pkg/VotingAppPkg/VotingWebPkg/Code
cd -

cd VotingData/
dotnet restore -s /opt/microsoft/sdk/servicefabric/csharp/packages -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/servicefabric-lkg-release/api/v3/index.json
dotnet build
cp -r PackageRoot/ ../Voting/pkg/VotingAppPkg/VotingDataPkg/
dotnet publish -o ../Voting/pkg/VotingAppPkg/VotingDataPkg/Code
cd -