
dotnet lambda deploy-function unad-functions-payment `
    --config-file .\serverless.template `
    --function-runtime dotnet8 `
    --function-role lambda_role `
    --function-memory-size 256 `
    --function-timeout 60 `
    --function-handler UnAd.Functions::UnAd.Functions.MessageHandler_Run_Generated::Run
