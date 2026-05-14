export async function createFeedback(payload) {
  const response = await fetch("/feedback-api/api/feedback/fast", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new Error("Failed to create feedback");
  }

  return response.json();
}

export async function getFeedbacksByUser(userId, page = 1, pageSize = 10, viewerUserId = null, viewerRole = "") {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });

  if (viewerUserId) {
    params.set("viewerUserId", viewerUserId);
  }

  if (viewerRole) {
    params.set("viewerRole", viewerRole);
  }

  const response = await fetch(`/feedback-api/api/feedback/user/${userId}?${params}`);

  if (!response.ok) {
    throw new Error("Failed to fetch feedbacks");
  }

  return response.json();
}
