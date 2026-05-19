import { useEffect, useMemo, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { createFeedback, getFeedbacksByUser } from "../services/feedbackService";
import { getAllUsersForPicker, getMyProfile, syncMe } from "../services/userService";
import "../styles/dashboard.css";

const FEEDBACK_TAGS = [
  "Collaboration",
  "Communication",
  "Leadership",
  "Ownership",
  "Innovation",
  "Quality",
  "Mentorship",
  "Reliability",
];

function Feedbacks({ preselectedRecipient = null, onPreselectedRecipientConsumed }) {
  const { instance, accounts } = useMsal();

  const [myProfile, setMyProfile] = useState(null);
  const [directory, setDirectory] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState("");
  const [prefilledRecipientSummary, setPrefilledRecipientSummary] = useState("");
  const [rating, setRating] = useState(5);
  const [visibility, setVisibility] = useState("Public");
  const [selectedTags, setSelectedTags] = useState([]);
  const [comment, setComment] = useState("");

  const [feedbacks, setFeedbacks] = useState([]);
  const [feedbackPage, setFeedbackPage] = useState(1);
  const [isBusy, setIsBusy] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [viewAsEnabled, setViewAsEnabled] = useState(true);
  const [viewAsUserId, setViewAsUserId] = useState("");
  const [viewAsRole, setViewAsRole] = useState("EMPLOYEE");

  const selectedUser = useMemo(
    () => directory.find((u) => u.id === selectedUserId) || null,
    [directory, selectedUserId]
  );

  const selectedDisplayName = selectedUser
    ? selectedUser.displayName || `${selectedUser.firstName || ""} ${selectedUser.lastName || ""}`.trim()
    : prefilledRecipientSummary;

  const effectiveViewerUserId = viewAsEnabled ? (viewAsUserId || myProfile?.id || null) : myProfile?.id;
  const effectiveViewerRole = viewAsEnabled ? (viewAsRole || myProfile?.role || "EMPLOYEE") : (myProfile?.role || "EMPLOYEE");

  const loadDirectoryUsers = async () => {
    if (!accounts.length) return;
    const users = await getAllUsersForPicker(instance, accounts[0]);
    setDirectory(users || []);
    return users || [];
  };

  const loadBaseData = async () => {
    if (!accounts.length) return;

    setError("");
    setIsBusy(true);

    try {
      await syncMe(instance, accounts[0]);
      const profile = await getMyProfile(instance, accounts[0]);

      setMyProfile(profile);

      if (!viewAsUserId) {
        setViewAsUserId(profile.id);
      }

      if ((profile.role || "").trim()) {
        setViewAsRole((profile.role || "EMPLOYEE").toUpperCase());
      }

    } catch (err) {
      console.error(err);
      setError("Could not load users for feedback.");
    } finally {
      setIsBusy(false);
    }
  };

  const loadFeedbacks = async (targetUserId, page = 1) => {
    if (!targetUserId || !myProfile?.id) {
      setFeedbacks([]);
      return;
    }

    setError("");
    setIsBusy(true);

    try {
      const data = await getFeedbacksByUser(
        targetUserId,
        page,
        10,
        effectiveViewerUserId,
        effectiveViewerRole
      );

      setFeedbacks(data);
      setFeedbackPage(page);
    } catch (err) {
      console.error(err);
      setError("Could not load feedback history.");
    } finally {
      setIsBusy(false);
    }
  };

  useEffect(() => {
    loadBaseData();
  }, [instance, accounts]);

  useEffect(() => {
    if (preselectedRecipient?.id) {
      const name = preselectedRecipient.displayName
        || `${preselectedRecipient.firstName || ""} ${preselectedRecipient.lastName || ""}`.trim()
        || preselectedRecipient.email
        || "Selected user";

      setSelectedUserId(preselectedRecipient.id);
      setPrefilledRecipientSummary(`${name}${preselectedRecipient.email ? ` (${preselectedRecipient.email})` : ""}`);
      onPreselectedRecipientConsumed?.();
    }
  }, [preselectedRecipient, onPreselectedRecipientConsumed]);

  useEffect(() => {
    if (selectedUserId && myProfile?.id) {
      loadFeedbacks(selectedUserId, 1);
    }
  }, [selectedUserId, myProfile?.id, viewAsEnabled, viewAsUserId, viewAsRole]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage("");
    setError("");

    if (!selectedUserId) {
      setError("Please select a user from Directory first.");
      return;
    }

    setIsBusy(true);
    try {
      await createFeedback({
        userId: selectedUserId,
        submittedByUserId: myProfile?.id || null,
        rating: Number(rating),
        comment,
        visibility,
        tags: selectedTags,
      });

      setComment("");
      setVisibility("Public");
      setRating(5);
      setSelectedTags([]);
      setMessage("Feedback submitted successfully.");
      await loadFeedbacks(selectedUserId, 1);
    } catch (err) {
      console.error(err);
      setError("Could not submit feedback.");
    } finally {
      setIsBusy(false);
    }
  };

  const toggleTag = (tag) => {
    setSelectedTags((prev) => {
      if (prev.includes(tag)) {
        return prev.filter((t) => t !== tag);
      }

      if (prev.length >= 5) {
        return prev;
      }

      return [...prev, tag];
    });
  };

  return (
    <div className="content-card">
      <div className="page-header-block">
        <h1 className="section-title">Give Feedback</h1>
        <p className="section-text">Create feedback for a colleague and review existing entries.</p>
      </div>


      <form className="feedback-form" onSubmit={handleSubmit}>
        <div className="feedback-grid">
          <label className="feedback-field">
            <span className="feedback-label">Recipient</span>
            <input
              className="feedback-select"
              value={prefilledRecipientSummary || "Select a user in Directory and click Give Feedback."}
              disabled
            />
          </label>

          <label className="feedback-field">
            <span className="feedback-label">Rating</span>
            <select
              className="feedback-select"
              value={rating}
              onChange={(e) => setRating(Number(e.target.value))}
              disabled={isBusy}
            >
              <option value={1}>1 - Needs improvement</option>
              <option value={2}>2 - Fair</option>
              <option value={3}>3 - Good</option>
              <option value={4}>4 - Very good</option>
              <option value={5}>5 - Excellent</option>
            </select>
          </label>

          <label className="feedback-field">
            <span className="feedback-label">Visibility</span>
            <select
              className="feedback-select"
              value={visibility}
              onChange={(e) => setVisibility(e.target.value)}
              disabled={isBusy}
            >
              <option value="Public">Public</option>
              <option value="Private">Private</option>
              <option value="HROnly">HROnly</option>
            </select>
          </label>
        </div>

        <label className="feedback-field">
          <span className="feedback-label">Comment</span>
          <textarea
            className="feedback-textarea"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            maxLength={2000}
            rows={5}
            placeholder="Write your feedback..."
            disabled={isBusy}
          />
        </label>

        <div className="feedback-field">
          <span className="feedback-label">Tags (up to 5)</span>
          <div className="tag-picker-wrap">
            {FEEDBACK_TAGS.map((tag) => {
              const isActive = selectedTags.includes(tag);
              const reachedLimit = selectedTags.length >= 5 && !isActive;
              return (
                <button
                  key={tag}
                  type="button"
                  className={`tag-chip ${isActive ? "active" : ""}`}
                  onClick={() => toggleTag(tag)}
                  disabled={isBusy || reachedLimit}
                >
                  {tag}
                </button>
              );
            })}
          </div>
        </div>

        <div className="feedback-actions">
          <button type="submit" className="refresh-users-button" disabled={isBusy || !comment.trim() || !selectedUserId}>
            {isBusy ? "Saving..." : "Submit Feedback"}
          </button>
          <span className="section-text">{comment.length}/2000</span>
        </div>
      </form>

      {message && <p className="feedback-success">{message}</p>}
      {error && <p className="error-text">{error}</p>}

      <div className="feedback-history-wrap">
        <h2 className="section-title feedback-history-title">
          Feedback History {selectedDisplayName ? `for ${selectedDisplayName}` : ""}
        </h2>

        {!isBusy && feedbacks.length === 0 && (
          <p className="section-text">No feedback found for selected user.</p>
        )}

        <div className="directory-grid">
          {feedbacks.map((item) => (
            <div className="user-card" key={item.id}>
              <div className="user-card-content">
                <h3 className="user-card-name">Rating: {item.rating}/5</h3>
                <p className="user-card-email">Visibility: {item.visibility}</p>
                {item.tags?.length > 0 && (
                  <div className="feedback-tags-wrap">
                    {item.tags.map((tag) => (
                      <span className="feedback-tag-badge" key={`${item.id}-${tag}`}>
                        {tag}
                      </span>
                    ))}
                  </div>
                )}
                <p className="user-card-meta">{item.comment}</p>
                <span className="user-role-badge">
                  {new Date(item.createdAt).toLocaleString()}
                </span>
              </div>
            </div>
          ))}
        </div>

        <div className="pagination-wrap">
          <div className="pagination-controls">
            <button
              type="button"
              className="pagination-button"
              disabled={feedbackPage === 1 || !selectedUserId || isBusy}
              onClick={() => loadFeedbacks(selectedUserId, feedbackPage - 1)}
            >
              Previous
            </button>

            <span className="pagination-page-indicator">Page {feedbackPage}</span>

            <button
              type="button"
              className="pagination-button"
              disabled={!selectedUserId || isBusy || feedbacks.length < 10}
              onClick={() => loadFeedbacks(selectedUserId, feedbackPage + 1)}
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Feedbacks;