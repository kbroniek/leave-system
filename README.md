# Leave system

[![.NET](https://github.com/kbroniek/leave-system/actions/workflows/dotnet.yml/badge.svg)](https://github.com/kbroniek/leave-system/actions/workflows/dotnet.yml)
[![DEV 02 - SWA](https://github.com/kbroniek/leave-system/actions/workflows/azure-static-web-apps-kind-flower-056781103.yml/badge.svg)](https://github.com/kbroniek/leave-system/actions/workflows/azure-static-web-apps-kind-flower-056781103.yml)
[![DEV - Roles for B2C - func-leave-system-dev-02](https://github.com/kbroniek/leave-system/actions/workflows/develop_func-leave-system-dev-02.yml/badge.svg)](https://github.com/kbroniek/leave-system/actions/workflows/develop_func-leave-system-dev-02.yml)

System to manage leaves.

## Overview

The project uses Azure B2C and custom Roles function to put roles in to the token claims.

Whole application you can deploy as a Static Web App.

- Backend - Azure Functions.
- Frontend - React (vite).

Database is CosmosDB.

## How to deploy

Check this [Readme](https://github.com/kbroniek/leave-system-iac).

## Troubleshooting

If you have the error when you deploy PR in to the Azure:

> The content server has rejected the request with: BadRequest
Reason: The number of static files was too large.

Probably you have an error in your code. Check logs and find errors.
