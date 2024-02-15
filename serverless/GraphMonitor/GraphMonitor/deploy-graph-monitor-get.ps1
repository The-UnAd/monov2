
dotnet lambda deploy-function graph-monitor-get `
    --config-file .\serverless.template `
    --function-runtime dotnet8 `
    --function-role lambda_execution_role `
    --function-memory-size 256 `
    --function-timeout 60 `
    --function-handler GraphMonitor::GraphMonitor.GetUrlFunction_GetUrl_Generated::GetUrl
