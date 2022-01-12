#!/bin/sh

# user secrets automatically available for development env
dotnet user-secrets set "telegram_app_id" "12345"
dotnet user-secrets set "telegram_api_hash" "1a2b3c4d5e"
dotnet user-secrets set "telegram_phone_number" "+70123456789"
dotnet user-secrets set "telegram_bot_token" "12345:abcde"