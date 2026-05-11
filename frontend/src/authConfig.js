export const msalConfig = {
  auth: {
    clientId: "e44c6305-2e87-4df4-b84d-c7fec0b6f84f",
    authority: "https://login.microsoftonline.com/544f8ac3-ce4c-47d1-9b72-284ac54b8d1c",
    redirectUri: "http://localhost:5173",
    postLogoutRedirectUri: "http://localhost:5173",
  },
  cache: {
    cacheLocation: "sessionStorage",
  },
};

export const loginRequest = {
  scopes: [
    "api://1f3cce39-08b2-4757-8978-e766269de6f8/access_as_user"
  ]
};

export const apiConfig = {
  userMeEndpoint: "https://localhost:7204/api/users/me",
};