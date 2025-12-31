# Quick Start Guide

Get the new Leave System up and running in minutes!

## Prerequisites

- Node.js 18+ installed
- pnpm installed (or use npm/yarn)
- Access to Azure B2C tenant
- API backend running

## Installation

1. **Navigate to the project directory**
   ```bash
   cd e:\leave-system\leave-system\src\web-new
   ```

2. **Install dependencies**
   ```bash
   pnpm install
   ```

3. **Create environment file**
   
   Create a `.env` file in the root of `web-new`:
   ```env
   VITE_REACT_APP_B2C_CLIENT_ID=your_client_id_here
   VITE_REACT_APP_AUTHORITY_SIGNIN=https://your-tenant.b2clogin.com/your-tenant.onmicrosoft.com/B2C_1_signin
   VITE_REACT_APP_AUTHORITY_DOMAIN=your-tenant.b2clogin.com
   VITE_REACT_APP_B2C_SCOPE_API=https://your-tenant.onmicrosoft.com/api/access_as_user
   VITE_REACT_APP_API_URL=https://your-api-url.azurewebsites.net/api
   ```

4. **Start development server**
   ```bash
   pnpm dev
   ```

5. **Open browser**
   
   Navigate to `http://localhost:5173`

## First Time Setup

### 1. Sign In
- Click "Sign in" button
- Authenticate with your Azure B2C credentials
- You'll be redirected back to the app

### 2. Verify Roles
- Navigate to `/claims` to see your token claims
- Verify your roles are correctly assigned

### 3. Test Features

#### As Employee:
- Submit a leave request (`/submit-request`)
- View your leave requests (`/my-leaves`)
- Check your leave limits

#### As HR/Decision Maker:
- View pending requests (`/hr-panel`)
- Approve or reject requests
- View leave requests timeline (`/`)

#### As Admin:
- Manage users and roles (`/users`)
- Manage leave limits (`/limits`)
- Access all features

## Common Issues

### Issue: "No active account" error
**Solution**: Make sure you're signed in. Check if MSAL is properly initialized.

### Issue: API calls failing
**Solution**: 
1. Verify API URL in `.env` is correct
2. Check if API is running
3. Verify API scopes are correct

### Issue: Roles not loading
**Solution**: 
1. Check if `/roles/me` endpoint is accessible
2. Verify token has correct claims
3. Clear localStorage and sign in again

### Issue: Translation not working
**Solution**: 
1. Verify translation files exist in `public/locales/`
2. Check browser console for loading errors
3. Try changing language in browser settings

## Development Tips

### React Query DevTools
Press `Ctrl+Shift+D` (or `Cmd+Shift+D` on Mac) to toggle React Query DevTools. This shows:
- Active queries and their states
- Cache contents
- Query invalidation events
- Network requests

### Hot Module Replacement (HMR)
Changes to components will automatically reload in the browser without losing state.

### Debugging
1. Open browser DevTools (F12)
2. Check Console for errors
3. Check Network tab for API calls
4. Use React DevTools extension

## Building for Production

### Standard Build
```bash
pnpm build
```
Output: `dist/` folder

### Azure Static Web Apps Build
```bash
pnpm build:swa
```
Output: `../../out/deploy/app/` folder

### Preview Production Build
```bash
pnpm preview
```
Serves the production build locally at `http://localhost:4173`

## Testing the Build

1. **Build the app**
   ```bash
   pnpm build
   ```

2. **Preview locally**
   ```bash
   pnpm preview
   ```

3. **Test all features**
   - Sign in/out
   - Submit leave request
   - Approve/reject requests
   - Manage users
   - Manage limits

## Deployment

### Azure Static Web Apps

1. **Build for SWA**
   ```bash
   pnpm build:swa
   ```

2. **Deploy using Azure CLI**
   ```bash
   az staticwebapp deploy \
     --name your-app-name \
     --resource-group your-resource-group \
     --app-location ./out/deploy/app
   ```

3. **Or use GitHub Actions**
   - Push to GitHub
   - Azure Static Web Apps will automatically build and deploy

### Manual Deployment

1. **Build the app**
   ```bash
   pnpm build
   ```

2. **Upload `dist/` folder to your hosting provider**

3. **Configure server**
   - Ensure all routes redirect to `index.html`
   - Set up CORS if needed
   - Configure HTTPS

## Environment-Specific Configuration

### Development
```env
VITE_REACT_APP_API_URL=http://localhost:7071/api
```

### Staging
```env
VITE_REACT_APP_API_URL=https://your-app-staging.azurewebsites.net/api
```

### Production
```env
VITE_REACT_APP_API_URL=https://your-app.azurewebsites.net/api
```

## Useful Commands

```bash
# Install dependencies
pnpm install

# Start development server
pnpm dev

# Build for production
pnpm build

# Build for Azure Static Web Apps
pnpm build:swa

# Preview production build
pnpm preview

# Run linter
pnpm lint

# Extract translation strings
pnpm trans-extract
```

## Project Structure Overview

```
web-new/
├── public/
│   ├── locales/          # Translation files
│   └── logo.png          # Company logo
├── src/
│   ├── components/       # Reusable UI components
│   ├── config/          # Configuration files
│   ├── hooks/           # TanStack Query hooks
│   ├── pages/           # Page components
│   ├── services/        # Business logic services
│   ├── styles/          # Theme and styles
│   ├── types/           # TypeScript types
│   ├── utils/           # Utility functions
│   ├── App.tsx          # Main app component
│   └── main.tsx         # Entry point
├── .env                 # Environment variables (create this)
├── package.json         # Dependencies
├── vite.config.ts       # Vite configuration
└── tsconfig.json        # TypeScript configuration
```

## Key Features

✅ **Azure AD B2C Authentication** - Secure sign-in with MSAL  
✅ **Role-Based Access Control** - Different views for different roles  
✅ **TanStack Query** - Automatic caching and state management  
✅ **Material-UI** - Beautiful, responsive design  
✅ **Internationalization** - Polish and English support  
✅ **Type Safety** - Full TypeScript support  
✅ **DevTools** - React Query DevTools for debugging  

## Getting Help

### Documentation
- [README.md](./README.md) - Full documentation
- [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md) - Migration from old app
- [IMPLEMENTATION_SUMMARY.md](./IMPLEMENTATION_SUMMARY.md) - Technical details

### External Resources
- [TanStack Query Docs](https://tanstack.com/query/latest)
- [MSAL React Docs](https://github.com/AzureAD/microsoft-authentication-library-for-js/tree/dev/lib/msal-react)
- [Material-UI Docs](https://mui.com/)

### Support
Contact the development team for assistance.

---

**Happy coding! 🚀**

