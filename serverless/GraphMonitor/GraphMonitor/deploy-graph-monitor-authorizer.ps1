
dotnet lambda deploy-function graph-monitor-authorizer `
    --config-file .\serverless.template `
    --function-runtime dotnet8 `
    --function-role lambda_role `
    --function-memory-size 256 `
    --function-timeout 60 `
    --function-handler GraphMonitor::GraphMonitor.Authorizer_Authorize_Generated::Authorize
