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

export async function searchUsers(instance, account, query = "", page = 1, pageSize = 12) {
  const token = await getAccessToken(instance, account);

  const params = new URLSearchParams({ q: query, page, pageSize });

  const response = await fetch(`${API_BASE_URL}/api/users/search?${params}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Failed to search users");
  }

  return response.json(); // { users, totalCount, page, pageSize, totalPages }
}

export async function getAllUsersForPicker(instance, account, query = "") {
  const pageSize = 100;
  let page = 1;
  let totalPages = 1;
  const all = [];

  do {
    const data = await searchUsers(instance, account, query, page, pageSize);
    all.push(...(data.users || []));
    totalPages = data.totalPages || 1;
    page += 1;
  } while (page <= totalPages);

  const uniqueById = new Map();
  for (const user of all) {
    if (!uniqueById.has(user.id)) {
      uniqueById.set(user.id, user);
    }
  }

  return Array.from(uniqueById.values());
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

export async function updateUser(instance, account, userId, payload) {
  const token = await getAccessToken(instance, account);

  const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    let details = "Failed to update user";
    try {
      details = await response.text();
    } catch {
      // Keep generic message if response body cannot be read.
    }
    throw new Error(details || "Failed to update user");
  }
}

function splitDisplayName(displayName = "") {
  const parts = displayName.trim().split(/\s+/).filter(Boolean);
  if (!parts.length) {
    return { firstName: "Unknown", lastName: "User" };
  }

  if (parts.length === 1) {
    return { firstName: parts[0], lastName: "User" };
  }

  return {
    firstName: parts[0],
    lastName: parts.slice(1).join(" "),
  };
}

export async function updateUserRole(instance, account, user, newRole) {
  const nameFromDisplay = splitDisplayName(user.displayName || "");
  const firstName = (user.firstName || "").trim() || nameFromDisplay.firstName;
  const lastName = (user.lastName || "").trim() || nameFromDisplay.lastName;

  await updateUser(instance, account, user.id, {
    firstName,
    lastName,
    email: (user.email || "").trim(),
    role: newRole,
    department: (user.department || "").trim(),
  });
}