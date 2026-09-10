#!/usr/bin/env bash
set -euo pipefail

# Replace these sample values before running.
SUBSCRIPTION_ID="<subscription-id>"
LOCATION="centralindia"
RESOURCE_GROUP="rg-shipment-cicd-lab"
ACR_NAME="<globally-unique-acr-name>"
ENV_NAME="cae-shipment-lab"
DEV_APP="shipment-api-dev"
PROD_APP="shipment-api-prod"

az account set --subscription "$SUBSCRIPTION_ID"

az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION"

az acr create \
  --name "$ACR_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --sku Basic

az extension add --name containerapp --upgrade

az containerapp env create \
  --name "$ENV_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION"

# Create initial apps using Microsoft's hello-world image.
# The pipeline will replace this image.
for APP in "$DEV_APP" "$PROD_APP"; do
  az containerapp create \
    --name "$APP" \
    --resource-group "$RESOURCE_GROUP" \
    --environment "$ENV_NAME" \
    --image mcr.microsoft.com/azuredocs/containerapps-helloworld:latest \
    --target-port 80 \
    --ingress external
done

echo "Azure resources created."
echo "Next: configure managed identity/service principal, federated credential, ACR pull permissions, and GitHub environments."
