
dotnet lambda deploy-function unad-functions-product `
    --config-file .\serverless.template `
    --function-runtime dotnet8 `
    --function-role lambda_role `
    --function-memory-size 256 `
    --function-timeout 60 `
    --function-handler UnAd.Functions::UnAd.Functions.StripeProductWebhook_Run_Generated::Run
