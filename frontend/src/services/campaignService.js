export async function createCampaign(payload) {
  const response = await fetch("/feedback-api/api/campaigns", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Failed to create campaign");
  }

  return response.json();
}

export async function updateCampaign(campaignId, payload) {
  const response = await fetch(`/feedback-api/api/campaigns/${campaignId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Failed to update campaign");
  }
}

export async function activateCampaign(campaignId) {
  const response = await fetch(`/feedback-api/api/campaigns/${campaignId}/activate`, {
    method: "POST",
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Failed to activate campaign");
  }

  return response.json();
}

export async function closeCampaign(campaignId) {
  const response = await fetch(`/feedback-api/api/campaigns/${campaignId}/close`, {
    method: "POST",
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Failed to close campaign");
  }

  return response.json();
}

export async function listCampaigns(status = "") {
  const params = new URLSearchParams();

  if (status) {
    params.set("status", status);
  }

  const response = await fetch(`/feedback-api/api/campaigns${params.toString() ? `?${params}` : ""}`);

  if (!response.ok) {
    throw new Error("Failed to fetch campaigns");
  }

  return response.json();
}

export async function listActiveCampaignsForUser(userId) {
  const response = await fetch(`/feedback-api/api/campaigns/active/${userId}`);

  if (!response.ok) {
    throw new Error("Failed to fetch active campaigns");
  }

  return response.json();
}

export async function getCampaignById(campaignId) {
  const response = await fetch(`/feedback-api/api/campaigns/${campaignId}`);

  if (!response.ok) {
    throw new Error("Failed to fetch campaign details");
  }

  return response.json();
}

export async function getCampaignUserProgress(campaignId, userId) {
  const response = await fetch(`/feedback-api/api/campaigns/${campaignId}/progress/${userId}`);

  if (!response.ok) {
    throw new Error("Failed to fetch campaign progress");
  }

  return response.json();
}

export async function getCampaignOverallProgress(campaignId) {
  const response = await fetch(`/feedback-api/api/campaigns/${campaignId}/progress`);

  if (!response.ok) {
    throw new Error("Failed to fetch campaign overall progress");
  }

  return response.json();
}

export async function getCampaignReport(campaignId) {
  const response = await fetch(`/feedback-api/api/campaigns/${campaignId}/report`);

  if (!response.ok) {
    throw new Error("Failed to fetch campaign report");
  }

  return response.json();
}

export async function submitCampaignFeedback(campaignId, payload) {
  const response = await fetch(`/feedback-api/api/campaigns/${campaignId}/feedback`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Failed to submit campaign feedback");
  }

  return response.json();
}
