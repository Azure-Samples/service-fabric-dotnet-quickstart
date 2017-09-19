#!/bin/bash
DIR=`dirname $0`

sfctl cluster select --endpoint http://localhost:19080
sfctl application upload --path Voting/pkg/VotingAppPkg
sfctl application provision --application-type-build-path VotingAppPkg
sfctl application create --app-name fabric:/Voting --app-type VotingNetCoreType --app-version 1.0

