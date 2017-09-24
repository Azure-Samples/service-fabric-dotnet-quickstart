#!/bin/bash
DIR=`dirname $0`

sfctl application upload --path Voting/pkg/VotingAppPkg
sfctl application provision --application-type-build-path VotingAppPkg --debug
sfctl application create --app-name fabric:/Voting --app-type VotingNetCoreType --app-version 1.0.0 --debug
sfctl service create --app-id Voting --name fabric:/Voting/VotingWeb --service-type VotingWebType --stateless --activation-mode ExclusiveProcess --singleton-scheme --instance-count 1
sfctl service create --app-id Voting --name fabric:/Voting/VotingData --service-type VotingDataType --stateful --activation-mode ExclusiveProcess --min-replica-set-size 3 --target-replica-set-size 3 --int-scheme --int-scheme-low -9223372036854775808 --int-scheme-high 9223372036854775807 --int-scheme-count 1

