import { getMyProfile } from "./userService";

export async function getMyNotifications(instance, account, page = 1, pageSize = 20) {
  const profile = await getMyProfile(instance, account);

  if (!profile?.id) {
    throw new Error("Current user profile is missing id.");
  }

  const response = await fetch(
    `/feedback-api/api/notifications/user/${profile.id}?page=${page}&pageSize=${pageSize}`
  );

  if (!response.ok) {
    throw new Error("Failed to fetch notifications.");
  }

  const data = await response.json();
  return { ...data, profile };
}

export async function markNotificationAsRead(notificationId, userId) {
  const response = await fetch(`/feedback-api/api/notifications/${notificationId}/read`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ userId }),
  });

  if (!response.ok) {
    throw new Error("Failed to mark notification as read.");
  }
}
