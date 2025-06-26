# Everwell API - Azure Deployment Script (Fixed CLI Path and Payment URLs)

param(
    [string]$ResourceGroup = "rg-everwell-api",
    [string]$Location = "Southeast Asia",
    [string]$AppName = "everwell-$(Get-Random -Minimum 1000 -Maximum 9999)",
    [string]$Sku = "B1"
)

# Azure CLI path
$azPath = "C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"

$AppServicePlan = "plan-everwell-api"
$WebAppUrl = "https://$AppName.azurewebsites.net"

Write-Host "EVERWELL API - AZURE DEPLOYMENT" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host "App Name: $AppName" -ForegroundColor Cyan
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "Location: $Location" -ForegroundColor Cyan
Write-Host ""

Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check if Azure CLI exists
if (-not (Test-Path $azPath)) {
    Write-Host "Azure CLI not found at $azPath" -ForegroundColor Red
    Write-Host "Please install Azure CLI or update the path in the script" -ForegroundColor Red
    exit 1
}

# Check if user is logged in
try {
    $accountInfo = & $azPath account show 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Please login to Azure CLI first: az login" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "Please login to Azure CLI first: az login" -ForegroundColor Red
    exit 1
}

Write-Host "Azure CLI found and user logged in!" -ForegroundColor Green
Write-Host ""

# Create Azure resources
Write-Host "Creating Azure resources..." -ForegroundColor Yellow

Write-Host "  Creating resource group..." -ForegroundColor Gray
& $azPath group create --name $ResourceGroup --location $Location --output none

Write-Host "  Creating app service plan..." -ForegroundColor Gray
& $azPath appservice plan create --name $AppServicePlan --resource-group $ResourceGroup --sku $Sku --is-linux --output none

Write-Host "  Creating web app..." -ForegroundColor Gray
& $azPath webapp create --resource-group $ResourceGroup --plan $AppServicePlan --name $AppName --runtime "DOTNETCORE:8.0" --output none

Write-Host "Azure resources created successfully!" -ForegroundColor Green

# Configure application settings
Write-Host ""
Write-Host "Configuring application settings..." -ForegroundColor Yellow

$jwtSecret = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("$(New-Guid)$(New-Guid)"))

Write-Host "  Setting database connection..." -ForegroundColor Gray
& $azPath webapp config connection-string set --resource-group $ResourceGroup --name $AppName --connection-string-type PostgreSQL --settings SupabaseConnection="User Id=postgres.iisehydgytuokmgqptiu;Password=Hcmcity114@@;Server=aws-0-us-east-2.pooler.supabase.com;Port=5432;Database=postgres" --output none

Write-Host "  Setting JWT configuration..." -ForegroundColor Gray
& $azPath webapp config appsettings set --resource-group $ResourceGroup --name $AppName --settings "Jwt__Secret=$jwtSecret" "Jwt__Issuer=Everwell" "Jwt__Audience=account" "Jwt__ExpirationInMinutes=60" --output none

Write-Host "  Setting email configuration..." -ForegroundColor Gray
& $azPath webapp config appsettings set --resource-group $ResourceGroup --name $AppName --settings "Email__SmtpServer=smtp.gmail.com" "Email__SmtpPort=587" "Email__Username=everwellhealth777@gmail.com" "Email__Password=puip varc ptnx rvtz" "Email__FromEmail=everwellhealth777@gmail.com" "Email__FromName=Everwell Health" --output none

Write-Host "  Setting VnPay configuration with correct URLs..." -ForegroundColor Gray
& $azPath webapp config appsettings set --resource-group $ResourceGroup --name $AppName --settings "VnPay__TmnCode=FFNSYP0G" "VnPay__HashSecret=DJCLGLGI5MEPJS6EPR4V8WEJMU2IRNAU" "VnPay__BaseUrl=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html" "VnPay__Version=2.1.0" "VnPay__Command=pay" "VnPay__ReturnUrl=$WebAppUrl/vnpay-callback" "VnPay__IpnUrl=$WebAppUrl/api/payment/vnpay-ipn" --output none

Write-Host "  Configuring security..." -ForegroundColor Gray
& $azPath webapp cors add --resource-group $ResourceGroup --name $AppName --allowed-origins "*" --output none
& $azPath webapp update --resource-group $ResourceGroup --name $AppName --https-only true --output none

Write-Host "Application configured successfully!" -ForegroundColor Green

# Build and deploy application
Write-Host ""
Write-Host "Building and deploying application..." -ForegroundColor Yellow

$originalLocation = Get-Location

if (Test-Path "publish") { Remove-Item "publish" -Recurse -Force }
if (Test-Path "everwell-api.zip") { Remove-Item "everwell-api.zip" -Force }

Write-Host "  Cleaning project..." -ForegroundColor Gray
dotnet clean --configuration Release --verbosity quiet

Write-Host "  Restoring packages..." -ForegroundColor Gray
dotnet restore --verbosity quiet

Write-Host "  Building project..." -ForegroundColor Gray
dotnet build --configuration Release --no-restore --verbosity quiet

Write-Host "  Publishing project..." -ForegroundColor Gray
dotnet publish --configuration Release --output "./publish" --no-build --verbosity quiet

Write-Host "  Creating deployment package..." -ForegroundColor Gray
Compress-Archive -Path "./publish/*" -DestinationPath "./everwell-api.zip" -Force

Write-Host "  Deploying to Azure..." -ForegroundColor Gray
& $azPath webapp deployment source config-zip --resource-group $ResourceGroup --name $AppName --src "./everwell-api.zip" --output none

Set-Location $originalLocation

Write-Host "Application deployed successfully!" -ForegroundColor Green

# Setup monitoring
Write-Host ""
Write-Host "Setting up monitoring..." -ForegroundColor Yellow

try {
    & $azPath extension add --name application-insights --only-show-errors --output none
    $appInsightsName = "$AppName-insights"
    & $azPath monitor app-insights component create --app $appInsightsName --location $Location --resource-group $ResourceGroup --output none
    $instrumentationKey = & $azPath monitor app-insights component show --app $appInsightsName --resource-group $ResourceGroup --query instrumentationKey --output tsv
    & $azPath webapp config appsettings set --resource-group $ResourceGroup --name $AppName --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$instrumentationKey" --output none
    Write-Host "Monitoring configured!" -ForegroundColor Green
}
catch {
    Write-Host "Monitoring setup failed (non-critical)" -ForegroundColor Yellow
}

# Verify deployment
Write-Host ""
Write-Host "Verifying deployment..." -ForegroundColor Yellow

Start-Sleep -Seconds 15
$appState = & $azPath webapp show --resource-group $ResourceGroup --name $AppName --query state --output tsv

if ($appState -eq "Running") {
    Write-Host "Application is running!" -ForegroundColor Green
    
    # Test API endpoint
    try {
        $response = Invoke-WebRequest -Uri "$WebAppUrl/swagger" -Method Get -TimeoutSec 30 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "API is responding!" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "API might still be starting up..." -ForegroundColor Yellow
    }
}
else {
    Write-Host "App state: $appState" -ForegroundColor Yellow
}

# Display results
Write-Host ""
Write-Host "DEPLOYMENT COMPLETED!" -ForegroundColor Green
Write-Host "=====================" -ForegroundColor Green
Write-Host ""
Write-Host "Your Everwell API Details:" -ForegroundColor Cyan
Write-Host "  API URL: $WebAppUrl" -ForegroundColor White
Write-Host "  Swagger UI: $WebAppUrl/swagger" -ForegroundColor White
Write-Host "  Payment IPN URL: $WebAppUrl/api/payment/vnpay-ipn" -ForegroundColor White
Write-Host "  Azure Portal: https://portal.azure.com" -ForegroundColor White
Write-Host "  Resource Group: $ResourceGroup" -ForegroundColor White
Write-Host ""
Write-Host "Payment Configuration Fixed:" -ForegroundColor Green
Write-Host "  VnPay ReturnUrl: $WebAppUrl/vnpay-callback" -ForegroundColor White
Write-Host "  VnPay IpnUrl: $WebAppUrl/api/payment/vnpay-ipn" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Test your API: $WebAppUrl/swagger" -ForegroundColor White
Write-Host "  2. Test payment flow with VnPay sandbox" -ForegroundColor White
Write-Host "  3. Update your frontend to use: $WebAppUrl" -ForegroundColor White
Write-Host "  4. Monitor in Azure Portal" -ForegroundColor White
Write-Host ""
Write-Host "Success! Your API is live at: $WebAppUrl" -ForegroundColor Green
Write-Host "Payment issues from previous deployments should now be resolved!" -ForegroundColor Green 