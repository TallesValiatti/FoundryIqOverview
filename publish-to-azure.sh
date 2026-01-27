#!/bin/bash

# ========================================
# Azure App Service Publish Script
# For .NET 10 Web Application on Linux
# ========================================

set -e  # Exit on any error

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored messages
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# ========================================
# Configuration Variables
# ========================================

# Azure Configuration (Update these values or set as environment variables)
RESOURCE_GROUP="${AZURE_RESOURCE_GROUP:-your-resource-group}"
APP_NAME="${AZURE_APP_NAME:-your-app-name}"

# Project Configuration
PROJECT_PATH="src/FoundryIqOverview.WebSite/FoundryIqOverview.WebSite.csproj"
PUBLISH_DIR="publish"

# ========================================
# Validate Prerequisites
# ========================================

print_info "Validating prerequisites..."

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    print_error "Azure CLI is not installed. Please install it from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK is not installed. Please install .NET 10 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
print_info "Using .NET SDK version: $DOTNET_VERSION"

# Check if project file exists
if [ ! -f "$PROJECT_PATH" ]; then
    print_error "Project file not found at: $PROJECT_PATH"
    exit 1
fi

# ========================================
# Azure Login Check
# ========================================

print_info "Checking Azure authentication..."

# Check if already logged in
if ! az account show &> /dev/null; then
    print_warning "Not logged in to Azure. Initiating login..."
    az login
else
    print_info "Already logged in to Azure"
    SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
    print_info "Using subscription: $SUBSCRIPTION_NAME"
fi

# ========================================
# Validate Configuration
# ========================================

if [ "$RESOURCE_GROUP" == "your-resource-group" ] || [ "$APP_NAME" == "your-app-name" ]; then
    print_error "Please set the required environment variables:"
    echo "  AZURE_RESOURCE_GROUP - Your Azure resource group name"
    echo "  AZURE_APP_NAME - Your App Service name"
    echo ""
    echo "Example:"
    echo "  export AZURE_RESOURCE_GROUP=myResourceGroup"
    echo "  export AZURE_APP_NAME=myWebApp"
    echo "  ./publish-to-azure.sh"
    exit 1
fi

# ========================================
# Verify App Service Exists
# ========================================

print_info "Verifying App Service exists..."

if ! az webapp show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
    print_error "App Service '$APP_NAME' not found in resource group '$RESOURCE_GROUP'"
    print_error "Please verify the names are correct."
    exit 1
fi

print_info "App Service found: $APP_NAME"

# ========================================
# Build and Publish Application
# ========================================

print_info "Cleaning previous publish directory..."
rm -rf "$PUBLISH_DIR"

print_info "Restoring NuGet packages..."
dotnet restore "$PROJECT_PATH"

print_info "Building application..."
dotnet build "$PROJECT_PATH" --configuration Release --no-restore

print_info "Publishing application..."
dotnet publish "$PROJECT_PATH" \
    --configuration Release \
    --output "$PUBLISH_DIR" \
    --no-build

# ========================================
# Deploy to Azure
# ========================================

print_info "Creating deployment package..."
cd "$PUBLISH_DIR"
zip -r ../deploy.zip . > /dev/null
cd ..

print_info "Deploying to Azure App Service..."
az webapp deployment source config-zip \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_NAME" \
    --src deploy.zip

# Clean up deployment package
rm -f deploy.zip

print_info "Cleaning up publish directory..."
rm -rf "$PUBLISH_DIR"

# ========================================
# Restart Web App
# ========================================

print_info "Restarting Web App..."
az webapp restart \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_NAME"

# ========================================
# Display Results
# ========================================

print_info "Deployment completed successfully!"
echo ""
print_info "====================================="
print_info "Deployment Summary"
print_info "====================================="
echo "Resource Group: $RESOURCE_GROUP"
echo "Web App Name: $APP_NAME"
echo ""

# Get the URL
APP_URL=$(az webapp show --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" --query defaultHostName -o tsv)
print_info "Application URL: https://$APP_URL"
echo ""

# Show logs command
print_info "To view live logs, run:"
echo "  az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP"
echo ""

# Show browse command
print_info "To open the app in your browser, run:"
echo "  az webapp browse --name $APP_NAME --resource-group $RESOURCE_GROUP"
echo ""

print_info "Done! 🚀"
