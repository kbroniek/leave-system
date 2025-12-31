/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_REACT_APP_B2C_CLIENT_ID: string;
  readonly VITE_REACT_APP_AUTHORITY_SIGNIN: string;
  readonly VITE_REACT_APP_AUTHORITY_DOMAIN: string;
  readonly VITE_REACT_APP_B2C_SCOPE_API: string;
  readonly VITE_REACT_APP_API_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
