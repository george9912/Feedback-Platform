import { loginRequest } from "../authConfig";

//const API_BASE_URL = "https://userservice-dev-george-d4cshqfud3h3ewgv.westeurope-01.azurewebsites.net";
const API_BASE_URL = "https://localhost:7204";
//const API_BASE_URL = "https://userservice-dev-george-d4cshqfud3h3ewgv.westeurope-01.azurewebsites.net";

async function getAccessToken(instance, account) {
  const response = await instance.acquireTokenSilent({
    ...loginRequest,
    account,
  });

  return response.accessToken;
}

export async function getUsers(instance, account) {
  const token = await getAccessToken(instance, account);

  const response = await fetch(`${API_BASE_URL}/api/users`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Failed to fetch users");
  }

  return response.json();
}

export async function getMyProfile(instance, account) {
  const token = await getAccessToken(instance, account);

  const response = await fetch(`${API_BASE_URL}/api/users/my-profile`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Failed to fetch profile");
  }

  return response.json();
}

export async function syncTenantUsers(instance, account) {
  const token = await getAccessToken(instance, account);

  const response = await fetch(`${API_BASE_URL}/api/users/sync-tenant`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Failed to sync users");
  }

  return response.json();
}

export async function syncMe(instance, account) {
  const token = await getAccessToken(instance, account);

  const response = await fetch(`${API_BASE_URL}/api/users/sync-me`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Failed to sync current user");
  }

  return response.json();
}

export async function askChat(instance, account, question) {
  const token = await getAccessToken(instance, account);

  const response = await fetch(`${API_BASE_URL}/api/chat`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ question }),
  });

  if (!response.ok) {
    throw new Error("Failed to fetch chat answer");
  }

  const data = await response.json();
  return data.answer;
}