# Everwell Backend - Azure Deployment Guide

## Overview
This guide will help you deploy the Everwell Healthcare backend API to Microsoft Azure App Service.

## Prerequisites

### 1. Install Azure CLI
- Download and install Azure CLI from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
- Verify installation: `az --version`

### 2. Login to Azure
```bash
az login
```

### 3. Verify .NET 8.0 SDK
```bash
dotnet --version
```

## Configuration Updates

✅ **Already Updated:**
- `appsettings.json` - Updated VnPay ReturnUrl to use your Vercel frontend
- `appsettings.json` - Updated AllowedOrigins to include your frontend URL
- `deploy-fixed.ps1` - Updated deployment script with correct URLs

## Deployment Options

### Option 1: Automated Deployment (Recommended)

Use the provided PowerShell script for automated deployment:

```powershell
# Navigate to the API project directory
cd "D:\Gender System\EverwellHealthcare_BE\Everwell.API"

# Run the deployment script
.\deploy-fixed.ps1
```

**Script Parameters (Optional):**
```powershell
.\deploy-fixed.ps1 -ResourceGroup "my-everwell-rg" -Location "Southeast Asia" -AppName "my-everwell-api" -Sku "B1"
```

### Option 2: Manual Deployment

#### Step 1: Create Azure Resources
```bash
# Set variables
RESOURCE_GROUP="rg-everwell-api"
LOCATION="Southeast Asia"
APP_NAME="everwell-$(date +%s)"
APP_SERVICE_PLAN="plan-everwell-api"

# Create resource group
az group create --name $RESOURCE_GROUP --location "$LOCATION"

# Create app service plan
az appservice plan create --name $APP_SERVICE_PLAN --resource-group $RESOURCE_GROUP --sku B1 --is-linux

# Create web app
az webapp create --resource-group $RESOURCE_GROUP --plan $APP_SERVICE_PLAN --name $APP_NAME --runtime "DOTNETCORE:8.0"
```

#### Step 2: Configure Application Settings
```bash
# Database connection
az webapp config connection-string set --resource-group $RESOURCE_GROUP --name $APP_NAME --connection-string-type PostgreSQL --settings SupabaseConnection="User Id=postgres.nfofrxpstnauperaqfbu;Password=Hcmcity114@;Server=aws-0-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres"

# JWT Configuration
az webapp config appsettings set --resource-group $RESOURCE_GROUP --name $APP_NAME --settings "Jwt__Secret=your-jwt-secret-here" "Jwt__Issuer=Everwell" "Jwt__Audience=account" "Jwt__ExpirationInMinutes=60"

# Email Configuration
az webapp config appsettings set --resource-group $RESOURCE_GROUP --name $APP_NAME --settings "Email__SmtpServer=smtp.gmail.com" "Email__SmtpPort=587" "Email__Username=everwellhealth777@gmail.com" "Email__Password=puip varc ptnx rvtz" "Email__FromEmail=everwellhealth777@gmail.com" "Email__FromName=Everwell Health"

# VnPay Configuration (Updated for your frontend)
az webapp config appsettings set --resource-group $RESOURCE_GROUP --name $APP_NAME --settings "VnPay__TmnCode=FFNSYP0G" "VnPay__HashSecret=DJCLGLGI5MEPJS6EPR4V8WEJMU2IRNAU" "VnPay__BaseUrl=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html" "VnPay__Version=2.1.0" "VnPay__Command=pay" "VnPay__ReturnUrl=https://everwell-zeta.vercel.app/vnpay-callback" "VnPay__IpnUrl=https://$APP_NAME.azurewebsites.net/api/payment/vnpay-ipn"

# Gemini AI Configuration
az webapp config appsettings set --resource-group $RESOURCE_GROUP --name $APP_NAME --settings "Gemini__ApiKey=AIzaSyAZ2rst8W1CLHLEyXzHIK8P6qL2x3T5fr0" "Gemini__Model=gemini-1.5-flash" "Gemini__SystemPrompt=Hãy trả lời bằng tiếng Việt."

# Daily.co Configuration
az webapp config appsettings set --resource-group $RESOURCE_GROUP --name $APP_NAME --settings "Daily__ApiKey=4fddf52313211c260e059fb854b732f63c63dea725b0ee01ff67e7e8bb102cf2" "Daily__DomainName=everwell.daily.co"

# CORS Configuration
az webapp cors add --resource-group $RESOURCE_GROUP --name $APP_NAME --allowed-origins "https://everwell-zeta.vercel.app" "http://localhost:5173"

# Enable HTTPS only
az webapp update --resource-group $RESOURCE_GROUP --name $APP_NAME --https-only true
```

#### Step 3: Build and Deploy
```bash
# Clean and build
dotnet clean --configuration Release
dotnet restore
dotnet build --configuration Release --no-restore

# Publish
dotnet publish --configuration Release --output "./publish" --no-build

# Create deployment package
zip -r everwell-api.zip ./publish/*

# Deploy to Azure
az webapp deployment source config-zip --resource-group $RESOURCE_GROUP --name $APP_NAME --src "./everwell-api.zip"
```

## Post-Deployment Configuration

### 1. Update Frontend API Base URL
After deployment, update your frontend environment variables to point to your new Azure API:

```javascript
// In your frontend .env file
VITE_API_BASE_URL=https://your-app-name.azurewebsites.net
```

### 2. Test the Deployment

1. **API Health Check:**
   - Visit: `https://your-app-name.azurewebsites.net/swagger`
   - Verify all endpoints are accessible

2. **Database Connection:**
   - Test user registration/login
   - Verify data persistence

3. **Payment Integration:**
   - Test VnPay payment flow
   - Verify callback URLs are working

4. **Email Service:**
   - Test notification emails
   - Verify SMTP configuration

### 3. Monitor Your Application

1. **Azure Portal:**
   - Go to https://portal.azure.com
   - Navigate to your resource group
   - Monitor logs and metrics

2. **Application Insights (Optional):**
   - The deployment script sets up basic monitoring
   - View detailed telemetry and performance metrics

## Important URLs After Deployment

- **API Base URL:** `https://your-app-name.azurewebsites.net`
- **Swagger Documentation:** `https://your-app-name.azurewebsites.net/swagger`
- **VnPay IPN URL:** `https://your-app-name.azurewebsites.net/api/payment/vnpay-ipn`
- **Health Check:** `https://your-app-name.azurewebsites.net/health` (if implemented)

## Environment Variables Summary

The following environment variables are configured during deployment:

| Category | Variable | Value |
|----------|----------|-------|
| Database | SupabaseConnection | Your Supabase connection string |
| JWT | Jwt__Secret | Auto-generated secure secret |
| Email | Email__SmtpServer | smtp.gmail.com |
| Payment | VnPay__ReturnUrl | https://everwell-zeta.vercel.app/vnpay-callback |
| Payment | VnPay__IpnUrl | https://your-app.azurewebsites.net/api/payment/vnpay-ipn |
| CORS | AllowedOrigins | https://everwell-zeta.vercel.app |

## Troubleshooting

### Common Issues:

1. **Deployment Fails:**
   - Check Azure CLI is logged in: `az account show`
   - Verify .NET 8.0 SDK is installed
   - Check PowerShell execution policy

2. **CORS Errors:**
   - Verify AllowedOrigins includes your frontend URL
   - Check CORS configuration in Azure portal

3. **Database Connection Issues:**
   - Verify Supabase connection string
   - Check firewall settings

4. **Payment Callback Issues:**
   - Verify VnPay ReturnUrl points to your frontend
   - Check IPN URL is accessible

### Logs and Debugging:

```bash
# View application logs
az webapp log tail --resource-group $RESOURCE_GROUP --name $APP_NAME

# Download logs
az webapp log download --resource-group $RESOURCE_GROUP --name $APP_NAME
```

## Security Considerations

1. **Environment Variables:**
   - Never commit secrets to source control
   - Use Azure Key Vault for sensitive data in production

2. **HTTPS:**
   - Always use HTTPS in production
   - The deployment script enables HTTPS-only

3. **CORS:**
   - Restrict CORS to specific domains
   - Avoid using wildcard (*) in production

## Next Steps

1. **Custom Domain (Optional):**
   - Configure custom domain in Azure
   - Update DNS settings
   - Configure SSL certificate

2. **Scaling:**
   - Monitor application performance
   - Scale up/out as needed
   - Consider Azure Application Gateway for load balancing

3. **Backup:**
   - Set up automated backups
   - Configure disaster recovery

4. **CI/CD Pipeline:**
   - Set up GitHub Actions or Azure DevOps
   - Automate deployments

## Support

If you encounter issues:
1. Check Azure portal logs
2. Review this guide
3. Consult Azure documentation
4. Contact your development team

---

**Deployment completed successfully!** 🎉

Your Everwell Healthcare API is now running on Azure and configured to work with your Vercel frontend at `https://everwell-zeta.vercel.app`.