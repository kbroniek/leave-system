# Leave System - New Implementation

This is a reimplementation of the leave system using modern React patterns with TanStack Query and Azure MSAL.

## Tech Stack

- **React 19** - UI framework
- **TypeScript** - Type safety
- **@azure/msal-react** + **@azure/msal-browser** - Azure AD B2C authentication
- **TanStack Query (React Query)** - Server state management
- **Material-UI (MUI)** - Component library
- **React Router** - Client-side routing
- **Luxon** - Date/time handling
- **i18next** - Internationalization (Polish/English)
- **Vite** - Build tool

## Key Features

- Azure AD B2C authentication with MSAL
- Role-based access control (RBAC)
- Server state management with TanStack Query
- Automatic cache invalidation and refetching
- Optimistic updates for better UX
- Internationalization support (Polish and English)
- Responsive Material-UI design
- Type-safe API client

## Project Structure

```
src/
├── components/         # Reusable UI components
├── config/            # Configuration files (auth, i18n)
├── hooks/             # TanStack Query hooks for API calls
├── pages/             # Page components (routes)
├── services/          # Business logic services (API client, role manager)
├── styles/            # Theme and global styles
└── utils/             # Utility functions
```

## Getting Started

### Prerequisites

- Node.js 18+
- pnpm (recommended) or npm

### Installation

```bash
pnpm install
```

### Environment Variables

Create a `.env` file in the root directory with the following variables:

```env
VITE_REACT_APP_B2C_CLIENT_ID=your_client_id
VITE_REACT_APP_AUTHORITY_SIGNIN=your_authority_signin
VITE_REACT_APP_AUTHORITY_DOMAIN=your_authority_domain
VITE_REACT_APP_B2C_SCOPE_API=your_api_scope
VITE_REACT_APP_API_URL=your_api_url
```

### Development

```bash
pnpm dev
```

### Build

```bash
pnpm build
```

### Build for Azure Static Web Apps

```bash
pnpm build:swa
```
