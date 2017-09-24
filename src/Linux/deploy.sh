#!/bin/bash
DIR=`dirname $0`

sfctl application upload --path Voting/pkg/VotingAppPkg --debug
sfctl application provision --application-type-build-path VotingAppPkg --debug
sfctl application create --app-name fabric:/Voting --app-type VotingNetCoreType --app-version 1.0.0 --debug

