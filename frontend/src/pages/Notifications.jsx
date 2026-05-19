import "../styles/dashboard.css";
import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { getMyNotifications, markNotificationAsRead } from "../services/notificationService";
import { getFeedbackById } from "../services/feedbackService";

function Notifications() {
  const { instance, accounts } = useMsal();

  const [notifications, setNotifications] = useState([]);
  const [profile, setProfile] = useState(null);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isBusy, setIsBusy] = useState(false);
  const [isModalLoading, setIsModalLoading] = useState(false);
  const [error, setError] = useState("");
  const [selectedFeedback, setSelectedFeedback] = useState(null);
  const [selectedNotification, setSelectedNotification] = useState(null);
  const [feedbackViewError, setFeedbackViewError] = useState("");

  const loadNotifications = async () => {
    if (!accounts.length) {
      return;
    }

    setIsBusy(true);
    setError("");

    try {
      const data = await getMyNotifications(instance, accounts[0]);
      setNotifications(data.items || []);
      setUnreadCount(data.unreadCount || 0);
      setProfile(data.profile || null);
    } catch (err) {
      console.error(err);
      setError("Could not load notifications.");
    } finally {
      setIsBusy(false);
    }
  };

  useEffect(() => {
    loadNotifications();
  }, [instance, accounts]);

  const handleMarkAsRead = async (notificationId) => {
    if (!profile?.id) {
      return;
    }

    try {
      await markNotificationAsRead(notificationId, profile.id);

      setNotifications((prev) =>
        prev.map((item) =>
          item.id === notificationId
            ? {
                ...item,
                isRead: true,
                readAtUtc: new Date().toISOString(),
              }
            : item
        )
      );

      setUnreadCount((prev) => (prev > 0 ? prev - 1 : 0));
    } catch (err) {
      console.error(err);
      setError("Could not update notification state.");
    }
  };

  const handleViewFeedback = async (item) => {
    setFeedbackViewError("");
    setIsModalLoading(true);

    try {
      const feedback = await getFeedbackById(item.feedbackId);
      setSelectedFeedback(feedback);
      setSelectedNotification(item);

      if (!item.isRead && profile?.id) {
        await handleMarkAsRead(item.id);
      }
    } catch (err) {
      console.error(err);
      setFeedbackViewError("Could not load feedback details.");
      setSelectedFeedback(null);
      setSelectedNotification(item);
    } finally {
      setIsModalLoading(false);
    }
  };

  const closeFeedbackModal = () => {
    setSelectedFeedback(null);
    setSelectedNotification(null);
    setFeedbackViewError("");
  };

  return (
    <div className="content-card">
      <h2 className="section-title">Notifications</h2>

      <div className="notifications-toolbar">
        <span className="unread-badge">Unread: {unreadCount}</span>
        <button className="refresh-users-button" onClick={loadNotifications} disabled={isBusy}>
          {isBusy ? "Refreshing..." : "Refresh"}
        </button>
      </div>

      {error && <p className="error-text">{error}</p>}

      {!isBusy && notifications.length === 0 && (
        <p className="section-text">No notifications yet.</p>
      )}

      <div className="directory-grid">
        {notifications.map((item) => (
          <div className="user-card" key={item.id}>
            <div className="user-card-content">
              <h3 className="user-card-name">{item.message}</h3>
              <p className="user-card-email">
                Created: {new Date(item.createdAtUtc).toLocaleString()}
              </p>
              <span className={`user-role-badge ${item.isRead ? "read-badge" : "unread-pill"}`}>
                {item.isRead ? "Read" : "Unread"}
              </span>

              <div className="notification-actions notification-actions-row">
                <button
                  className="pagination-button"
                  type="button"
                  onClick={() => handleViewFeedback(item)}
                >
                  View feedback
                </button>

                {!item.isRead && (
                  <button
                    className="pagination-button"
                    type="button"
                    onClick={() => handleMarkAsRead(item.id)}
                  >
                    Mark as read
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>

      {selectedNotification && (
        <div className="modal-backdrop" onClick={closeFeedbackModal}>
          <div className="feedback-modal" onClick={(e) => e.stopPropagation()}>
            <div className="feedback-modal-header">
              <h3 className="section-title modal-title">Received Feedback</h3>
              <button className="icon-button" type="button" onClick={closeFeedbackModal}>
                ✕
              </button>
            </div>

            {isModalLoading && <p className="section-text">Loading feedback...</p>}

            {!isModalLoading && feedbackViewError && (
              <p className="error-text">{feedbackViewError}</p>
            )}

            {!isModalLoading && !feedbackViewError && selectedFeedback && (
              <div className="feedback-modal-content">
                <p><strong>Rating:</strong> {selectedFeedback.rating}/5</p>
                <p><strong>Visibility:</strong> {selectedFeedback.visibility}</p>
                <p><strong>Created:</strong> {new Date(selectedFeedback.createdAt).toLocaleString()}</p>
                <p><strong>Comment:</strong></p>
                <p className="feedback-modal-comment">{selectedFeedback.comment}</p>
                {selectedFeedback.tags?.length > 0 && (
                  <div className="feedback-tags-wrap">
                    {selectedFeedback.tags.map((tag) => (
                      <span className="feedback-tag-badge" key={`${selectedFeedback.id}-${tag}`}>
                        {tag}
                      </span>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default Notifications;