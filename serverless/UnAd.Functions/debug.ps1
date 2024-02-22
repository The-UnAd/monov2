param (
    [Parameter(Mandatory=$true)]
    [string]$functionName
)

sam local start-api --warm-containers LAZY --template-file .\serverless.template -p 7071 -d 5858 --debug-function $functionName

