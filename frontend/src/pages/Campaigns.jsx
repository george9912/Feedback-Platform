import { useEffect, useMemo, useState } from "react";
import { useMsal } from "@azure/msal-react";
import {
  activateCampaign,
  closeCampaign,
  createCampaign,
  getCampaignById,
  getCampaignOverallProgress,
  getCampaignReport,
  getCampaignUserProgress,
  listActiveCampaignsForUser,
  listCampaigns,
  submitCampaignFeedback,
  updateCampaign,
} from "../services/campaignService";
import { getAllUsersForPicker, getMyProfile, syncMe } from "../services/userService";
import "../styles/dashboard.css";

const FEEDBACK_TAGS = ["Collaboration", "Communication", "Leadership", "Ownership", "Quality", "Reliability"];

function Campaigns() {
  const { instance, accounts } = useMsal();

  const [myProfile, setMyProfile] = useState(null);
  const [users, setUsers] = useState([]);
  const [campaigns, setCampaigns] = useState([]);
  const [activeCampaigns, setActiveCampaigns] = useState([]);

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [startDateUtc, setStartDateUtc] = useState("");
  const [endDateUtc, setEndDateUtc] = useState("");
  const [minimumRequiredSubmissions, setMinimumRequiredSubmissions] = useState(3);
  const [isAnonymous, setIsAnonymous] = useState(false);

  const [audienceType, setAudienceType] = useState("AllUsers");
  const [selectedRoles, setSelectedRoles] = useState([]);
  const [selectedDepartments, setSelectedDepartments] = useState([]);
  const [selectedUserIds, setSelectedUserIds] = useState([]);

  const [editingCampaignId, setEditingCampaignId] = useState("");
  const [selectedCampaignId, setSelectedCampaignId] = useState("");
  const [selectedCampaignDetails, setSelectedCampaignDetails] = useState(null);
  const [userProgress, setUserProgress] = useState(null);
  const [overallProgress, setOverallProgress] = useState(null);
  const [report, setReport] = useState(null);

  const [recipientUserId, setRecipientUserId] = useState("");
  const [rating, setRating] = useState(5);
  const [visibility, setVisibility] = useState("Public");
  const [comment, setComment] = useState("");
  const [selectedTags, setSelectedTags] = useState([]);

  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [reportDialog, setReportDialog] = useState({
    open: false,
    campaign: null,
    report: null,
    overall: null,
  });

  const selectedCampaignStatus = (selectedCampaignDetails?.status || "").toUpperCase();
  const isSelectedCampaignClosed = selectedCampaignStatus === "CLOSED";

  const isAdmin = useMemo(() => {
    const role = (myProfile?.role || "").toUpperCase();
    return role === "ADMIN" || role === "HR";
  }, [myProfile]);

  const roleOptions = useMemo(() => {
    return Array.from(new Set(users.map((u) => (u.role || "").toUpperCase()).filter(Boolean))).sort();
  }, [users]);

  const departmentOptions = useMemo(() => {
    return Array.from(new Set(users.map((u) => (u.department || "").trim()).filter(Boolean))).sort();
  }, [users]);

  const resolvedParticipantIds = useMemo(() => {
    if (audienceType === "AllUsers") {
      return users.map((u) => u.id);
    }

    if (audienceType === "Roles") {
      return users
        .filter((u) => selectedRoles.includes((u.role || "").toUpperCase()))
        .map((u) => u.id);
    }

    if (audienceType === "Departments") {
      return users
        .filter((u) => selectedDepartments.includes((u.department || "").trim()))
        .map((u) => u.id);
    }

    return selectedUserIds;
  }, [audienceType, users, selectedRoles, selectedDepartments, selectedUserIds]);

  const campaignParticipants = useMemo(() => {
    if (!selectedCampaignDetails?.participantUserIds?.length) {
      return [];
    }

    const participantsMap = new Map(users.map((u) => [u.id, u]));
    return selectedCampaignDetails.participantUserIds
      .map((id) => participantsMap.get(id))
      .filter(Boolean);
  }, [selectedCampaignDetails, users]);

  const loadData = async () => {
    if (!accounts.length) {
      return;
    }

    setBusy(true);
    setError("");

    try {
      await syncMe(instance, accounts[0]);
      const profile = await getMyProfile(instance, accounts[0]);
      setMyProfile(profile);

      const usersData = await getAllUsersForPicker(instance, accounts[0]);
      setUsers(usersData || []);

      const campaignsData = await listCampaigns();
      setCampaigns(campaignsData || []);

      if (profile?.id) {
        const active = await listActiveCampaignsForUser(profile.id);
        setActiveCampaigns(active || []);
      }
    } catch (err) {
      setError("Could not load campaigns data.");
      console.error(err);
    } finally {
      setBusy(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [accounts, instance]);

  useEffect(() => {
    const loadCampaignDetails = async () => {
      if (!selectedCampaignId || !myProfile?.id) {
        setSelectedCampaignDetails(null);
        setUserProgress(null);
        setOverallProgress(null);
        setReport(null);
        return;
      }

      setBusy(true);
      setError("");
      setReport(null);

      try {
        const details = await getCampaignById(selectedCampaignId);
        setSelectedCampaignDetails(details);
        setRecipientUserId(details.participantUserIds?.[0] || "");

        try {
          const progressData = await getCampaignUserProgress(selectedCampaignId, myProfile.id);
          setUserProgress(progressData);
        } catch {
          setUserProgress(null);
        }

        try {
          const overallData = await getCampaignOverallProgress(selectedCampaignId);
          setOverallProgress(overallData);
        } catch {
          setOverallProgress(null);
        }

        if ((details.status || "").toUpperCase() === "CLOSED") {
          try {
            const reportData = await getCampaignReport(selectedCampaignId);
            setReport(reportData);
          } catch {
            setReport(null);
            setError("Campaign is closed but final report could not be loaded yet.");
          }
        }
      } catch (err) {
        console.error(err);
        setError("Could not load campaign details.");
      } finally {
        setBusy(false);
      }
    };

    loadCampaignDetails();
  }, [selectedCampaignId, myProfile?.id]);

  const resetCampaignForm = () => {
    setEditingCampaignId("");
    setName("");
    setDescription("");
    setStartDateUtc("");
    setEndDateUtc("");
    setMinimumRequiredSubmissions(3);
    setIsAnonymous(false);
    setAudienceType("AllUsers");
    setSelectedRoles([]);
    setSelectedDepartments([]);
    setSelectedUserIds([]);
  };

  const buildAudienceTargets = () => {
    if (audienceType === "AllUsers") {
      return [{ type: "AllUsers", values: [] }];
    }

    if (audienceType === "Roles") {
      return [{ type: "Roles", values: selectedRoles }];
    }

    if (audienceType === "Departments") {
      return [{ type: "Departments", values: selectedDepartments }];
    }

    return [{ type: "SelectedUsers", values: selectedUserIds }];
  };

  const handleCreateOrUpdateCampaign = async (event) => {
    event.preventDefault();
    setError("");
    setMessage("");

    if (!myProfile?.id) {
      setError("Current profile is required.");
      return;
    }

    if (!name.trim()) {
      setError("Campaign name is required.");
      return;
    }

    if (!startDateUtc || !endDateUtc) {
      setError("Start and end dates are required.");
      return;
    }

    if (!resolvedParticipantIds.length) {
      setError("Audience selection has no participants.");
      return;
    }

    const payload = {
      name: name.trim(),
      description: description.trim(),
      startDateUtc: new Date(startDateUtc).toISOString(),
      endDateUtc: new Date(endDateUtc).toISOString(),
      minimumRequiredSubmissions: Number(minimumRequiredSubmissions),
      isAnonymous,
      createdByAdminId: myProfile.id,
      audienceTargets: buildAudienceTargets(),
      resolvedParticipantUserIds: resolvedParticipantIds,
    };

    setBusy(true);

    try {
      if (editingCampaignId) {
        await updateCampaign(editingCampaignId, payload);
        setMessage("Campaign updated.");
      } else {
        await createCampaign(payload);
        setMessage("Campaign created.");
      }

      resetCampaignForm();
      await loadData();
    } catch (err) {
      console.error(err);
      setError("Could not save campaign.");
    } finally {
      setBusy(false);
    }
  };

  const startEditingCampaign = async (campaignId) => {
    setBusy(true);
    setError("");
    setMessage("");

    try {
      const details = await getCampaignById(campaignId);
      setEditingCampaignId(details.id);
      setName(details.name || "");
      setDescription(details.description || "");
      setStartDateUtc(details.startDateUtc ? details.startDateUtc.slice(0, 16) : "");
      setEndDateUtc(details.endDateUtc ? details.endDateUtc.slice(0, 16) : "");
      setMinimumRequiredSubmissions(details.minimumRequiredSubmissions || 1);
      setIsAnonymous(Boolean(details.isAnonymous));

      const firstTarget = details.audienceTargets?.[0];
      const type = firstTarget?.type || "AllUsers";
      setAudienceType(type);

      if (type === "Roles") {
        setSelectedRoles(firstTarget.values || []);
        setSelectedDepartments([]);
        setSelectedUserIds([]);
      } else if (type === "Departments") {
        setSelectedDepartments(firstTarget.values || []);
        setSelectedRoles([]);
        setSelectedUserIds([]);
      } else if (type === "SelectedUsers") {
        setSelectedUserIds(details.participantUserIds || []);
        setSelectedRoles([]);
        setSelectedDepartments([]);
      } else {
        setSelectedRoles([]);
        setSelectedDepartments([]);
        setSelectedUserIds([]);
      }
    } catch (err) {
      console.error(err);
      setError("Could not load campaign for editing.");
    } finally {
      setBusy(false);
    }
  };

  const handleActivateCampaign = async (campaignId) => {
    setBusy(true);
    setError("");
    setMessage("");

    try {
      await activateCampaign(campaignId);
      setMessage("Campaign activated and reminders scheduled.");
      await loadData();
    } catch (err) {
      console.error(err);
      setError("Could not activate campaign.");
    } finally {
      setBusy(false);
    }
  };

  const handleCloseCampaign = async (campaignId) => {
    setBusy(true);
    setError("");
    setMessage("");

    try {
      await closeCampaign(campaignId);
      setMessage("Campaign closed and final report generated.");
      await loadData();
    } catch (err) {
      console.error(err);
      setError("Could not close campaign.");
    } finally {
      setBusy(false);
    }
  };

  const openCampaignDialog = async (campaign) => {
    const status = (campaign?.status || "").toUpperCase();

    if (status !== "CLOSED") {
      setSelectedCampaignId(campaign.id);
      setMessage(`Opened campaign: ${campaign.name}`);
      setError("");
      return;
    }

    setBusy(true);
    setError("");

    try {
      const [details, finalReport, overall] = await Promise.all([
        getCampaignById(campaign.id),
        getCampaignReport(campaign.id),
        getCampaignOverallProgress(campaign.id),
      ]);

      setReportDialog({
        open: true,
        campaign: details,
        report: finalReport,
        overall,
      });

      setMessage(`Opened final report: ${campaign.name}`);
      setError("");
    } catch (err) {
      console.error(err);
      setError("Could not open report dialog.");
    } finally {
      setBusy(false);
    }
  };

  const toggleSelection = (setFn, value) => {
    setFn((prev) => (prev.includes(value) ? prev.filter((item) => item !== value) : [...prev, value]));
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

  const handleSubmitCampaignFeedback = async (event) => {
    event.preventDefault();
    setError("");
    setMessage("");

    if (!selectedCampaignId) {
      setError("Select an active campaign first.");
      return;
    }

    if (!recipientUserId) {
      setError("Select a recipient user.");
      return;
    }

    if (!myProfile?.id) {
      setError("Current profile not available.");
      return;
    }

    setBusy(true);

    try {
      await submitCampaignFeedback(selectedCampaignId, {
        recipientUserId,
        submittedByUserId: myProfile.id,
        rating: Number(rating),
        comment,
        visibility,
        tags: selectedTags,
      });

      setComment("");
      setRating(5);
      setVisibility("Public");
      setSelectedTags([]);
      setMessage("Campaign feedback submitted.");

      const progressData = await getCampaignUserProgress(selectedCampaignId, myProfile.id);
      const overallData = await getCampaignOverallProgress(selectedCampaignId);
      setUserProgress(progressData);
      setOverallProgress(overallData);
    } catch (err) {
      console.error(err);
      setError("Could not submit campaign feedback.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="content-card">
      <div className="page-header-block">
        <h1 className="section-title">Feedback Campaigns</h1>
        <p className="section-text">
          Create structured campaigns, track participation progress, and submit campaign feedback.
        </p>
      </div>

      {message && <p className="feedback-success">{message}</p>}
      {error && <p className="error-text">{error}</p>}

      {isAdmin && (
        <div className="feedback-history-wrap">
          <h2 className="section-title feedback-history-title">Admin Campaign Management</h2>

          <form className="feedback-form" onSubmit={handleCreateOrUpdateCampaign}>
            <div className="feedback-grid">
              <label className="feedback-field">
                <span className="feedback-label">Name</span>
                <input
                  className="feedback-select"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  disabled={busy}
                />
              </label>

              <label className="feedback-field">
                <span className="feedback-label">Minimum Required Submissions</span>
                <input
                  className="feedback-select"
                  type="number"
                  min={1}
                  value={minimumRequiredSubmissions}
                  onChange={(e) => setMinimumRequiredSubmissions(Number(e.target.value))}
                  disabled={busy}
                />
              </label>

              <label className="feedback-field">
                <span className="feedback-label">Audience Type</span>
                <select
                  className="feedback-select"
                  value={audienceType}
                  onChange={(e) => setAudienceType(e.target.value)}
                  disabled={busy}
                >
                  <option value="AllUsers">All Users</option>
                  <option value="Roles">Specific Roles</option>
                  <option value="Departments">Departments</option>
                  <option value="SelectedUsers">Selected Users</option>
                </select>
              </label>
            </div>

            <label className="feedback-field">
              <span className="feedback-label">Description</span>
              <textarea
                className="feedback-textarea"
                rows={4}
                maxLength={2000}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                disabled={busy}
              />
            </label>

            <div className="feedback-grid">
              <label className="feedback-field">
                <span className="feedback-label">Start Date (UTC)</span>
                <input
                  className="feedback-select"
                  type="datetime-local"
                  value={startDateUtc}
                  onChange={(e) => setStartDateUtc(e.target.value)}
                  disabled={busy}
                />
              </label>

              <label className="feedback-field">
                <span className="feedback-label">End Date (UTC)</span>
                <input
                  className="feedback-select"
                  type="datetime-local"
                  value={endDateUtc}
                  onChange={(e) => setEndDateUtc(e.target.value)}
                  disabled={busy}
                />
              </label>

              <label className="view-as-toggle" style={{ marginTop: "2rem" }}>
                <input
                  type="checkbox"
                  checked={isAnonymous}
                  onChange={(e) => setIsAnonymous(e.target.checked)}
                  disabled={busy}
                />
                <span>Anonymous submissions</span>
              </label>
            </div>

            {audienceType === "Roles" && (
              <div className="tag-picker-wrap">
                {roleOptions.map((role) => (
                  <button
                    key={role}
                    type="button"
                    className={`tag-chip ${selectedRoles.includes(role) ? "active" : ""}`}
                    onClick={() => toggleSelection(setSelectedRoles, role)}
                    disabled={busy}
                  >
                    {role}
                  </button>
                ))}
              </div>
            )}

            {audienceType === "Departments" && (
              <div className="tag-picker-wrap">
                {departmentOptions.map((department) => (
                  <button
                    key={department}
                    type="button"
                    className={`tag-chip ${selectedDepartments.includes(department) ? "active" : ""}`}
                    onClick={() => toggleSelection(setSelectedDepartments, department)}
                    disabled={busy}
                  >
                    {department}
                  </button>
                ))}
              </div>
            )}

            {audienceType === "SelectedUsers" && (
              <label className="feedback-field">
                <span className="feedback-label">Selected Users</span>
                <select
                  className="feedback-select"
                  multiple
                  value={selectedUserIds}
                  onChange={(e) => {
                    const values = Array.from(e.target.selectedOptions).map((opt) => opt.value);
                    setSelectedUserIds(values);
                  }}
                  disabled={busy}
                  style={{ minHeight: "180px" }}
                >
                  {users.map((user) => {
                    const nameLabel = user.displayName || `${user.firstName || ""} ${user.lastName || ""}`.trim() || user.email;
                    return (
                      <option key={user.id} value={user.id}>
                        {nameLabel}
                      </option>
                    );
                  })}
                </select>
              </label>
            )}

            <p className="section-text">Resolved participants: {resolvedParticipantIds.length}</p>

            <div className="feedback-actions">
              <button className="refresh-users-button" type="submit" disabled={busy}>
                {editingCampaignId ? "Save Campaign" : "Create Campaign"}
              </button>

              {editingCampaignId && (
                <button className="pagination-button" type="button" onClick={resetCampaignForm} disabled={busy}>
                  Cancel Edit
                </button>
              )}
            </div>
          </form>

          <div className="directory-grid">
            {campaigns.map((campaign) => (
              <div className="user-card" key={campaign.id}>
                <div className="user-card-content">
                  <h3 className="user-card-name">{campaign.name}</h3>
                  <p className="user-card-email">Status: {campaign.status}</p>
                  <p className="user-card-meta">{campaign.description || "No description"}</p>
                  <span className="user-role-badge">{new Date(campaign.endDateUtc).toLocaleString()}</span>

                  <div className="feedback-actions" style={{ marginTop: "0.75rem" }}>
                    <button
                      className="pagination-button"
                      type="button"
                      onClick={() => openCampaignDialog(campaign)}
                      disabled={busy}
                    >
                      Open
                    </button>

                    {campaign.status === "Draft" && (
                      <>
                        <button
                          className="pagination-button"
                          type="button"
                          onClick={() => startEditingCampaign(campaign.id)}
                          disabled={busy}
                        >
                          Edit
                        </button>
                        <button
                          className="pagination-button"
                          type="button"
                          onClick={() => handleActivateCampaign(campaign.id)}
                          disabled={busy}
                        >
                          Activate
                        </button>
                      </>
                    )}

                    {campaign.status === "Active" && (
                      <button
                        className="pagination-button"
                        type="button"
                        onClick={() => handleCloseCampaign(campaign.id)}
                        disabled={busy}
                      >
                        Close
                      </button>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="feedback-history-wrap">
        <h2 className="section-title feedback-history-title">Campaign Participation</h2>

        <label className="feedback-field">
          <span className="feedback-label">Active Campaign</span>
          <select
            className="feedback-select"
            value={selectedCampaignId}
            onChange={(e) => setSelectedCampaignId(e.target.value)}
            disabled={busy}
          >
            <option value="">Select campaign</option>
            {selectedCampaignDetails && !activeCampaigns.some((campaign) => campaign.id === selectedCampaignDetails.id) && (
              <option value={selectedCampaignDetails.id}>
                {selectedCampaignDetails.name} ({selectedCampaignDetails.status})
              </option>
            )}
            {activeCampaigns.map((campaign) => (
              <option key={campaign.id} value={campaign.id}>
                {campaign.name}
              </option>
            ))}
          </select>
        </label>

        {selectedCampaignDetails && (
          <p className="section-text">
            Viewing campaign: {selectedCampaignDetails.name} ({selectedCampaignDetails.status})
          </p>
        )}

        {userProgress && (
          <p className="section-text">
            My progress: {userProgress.status} ({userProgress.submittedSubmissions}/{userProgress.requiredSubmissions})
          </p>
        )}

        {overallProgress && isAdmin && (
          <p className="section-text">
            Overall progress: Completed {overallProgress.completed}, In Progress {overallProgress.inProgress}, Not Started {overallProgress.notStarted}
          </p>
        )}

        {isSelectedCampaignClosed && report && (
          <div className="view-as-wrap" style={{ marginBottom: "1rem" }}>
            <h3 className="section-title">Final Report</h3>
            <p className="section-text">Total participants: {report.totalParticipants}</p>
            <p className="section-text">Completed: {report.completedParticipants}</p>
            <p className="section-text">In progress: {report.inProgressParticipants}</p>
            <p className="section-text">Not started: {report.notStartedParticipants}</p>
            <p className="section-text">Total submissions: {report.totalSubmissions}</p>
            <p className="section-text">Generated: {new Date(report.generatedAtUtc).toLocaleString()}</p>
          </div>
        )}

        {isSelectedCampaignClosed && (
          <p className="section-text">This campaign is closed. New submissions are disabled.</p>
        )}

        <form className="feedback-form" onSubmit={handleSubmitCampaignFeedback}>
          <div className="feedback-grid">
            <label className="feedback-field">
              <span className="feedback-label">Recipient</span>
              <select
                className="feedback-select"
                value={recipientUserId}
                onChange={(e) => setRecipientUserId(e.target.value)}
                disabled={busy || !campaignParticipants.length}
              >
                <option value="">Select recipient</option>
                {campaignParticipants.map((user) => {
                  const nameLabel = user.displayName || `${user.firstName || ""} ${user.lastName || ""}`.trim() || user.email;
                  return (
                    <option key={user.id} value={user.id}>
                      {nameLabel}
                    </option>
                  );
                })}
              </select>
            </label>

            <label className="feedback-field">
              <span className="feedback-label">Rating</span>
              <select
                className="feedback-select"
                value={rating}
                onChange={(e) => setRating(Number(e.target.value))}
                disabled={busy}
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
                disabled={busy}
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
              rows={5}
              maxLength={2000}
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              disabled={busy}
            />
          </label>

          <div className="feedback-field">
            <span className="feedback-label">Tags (up to 5)</span>
            <div className="tag-picker-wrap">
              {FEEDBACK_TAGS.map((tag) => {
                const active = selectedTags.includes(tag);
                const reachedLimit = selectedTags.length >= 5 && !active;
                return (
                  <button
                    key={tag}
                    type="button"
                    className={`tag-chip ${active ? "active" : ""}`}
                    onClick={() => toggleTag(tag)}
                    disabled={busy || reachedLimit}
                  >
                    {tag}
                  </button>
                );
              })}
            </div>
          </div>

          <div className="feedback-actions">
            <button
              className="refresh-users-button"
              type="submit"
              disabled={busy || isSelectedCampaignClosed || !selectedCampaignId || !comment.trim() || !recipientUserId}
            >
              Submit Campaign Feedback
            </button>
          </div>
        </form>
      </div>

      {reportDialog.open && reportDialog.campaign && reportDialog.report && (
        <div
          role="dialog"
          aria-modal="true"
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(15, 23, 42, 0.5)",
            display: "grid",
            placeItems: "center",
            zIndex: 3000,
            padding: "1rem",
          }}
        >
          <div
            className="content-card"
            style={{
              width: "min(860px, 100%)",
              maxHeight: "90vh",
              overflowY: "auto",
              borderRadius: "14px",
            }}
          >
            <div className="page-header-block" style={{ marginBottom: "0.75rem" }}>
              <h2 className="section-title">{reportDialog.campaign.name} - Final Report</h2>
              <p className="section-text">{reportDialog.campaign.description || "No description"}</p>
              <p className="section-text">
                Period: {new Date(reportDialog.campaign.startDateUtc).toLocaleString()} - {new Date(reportDialog.campaign.endDateUtc).toLocaleString()}
              </p>
            </div>

            <div className="view-as-wrap">
              <p className="section-text">Total participants: {reportDialog.report.totalParticipants}</p>
              <p className="section-text">Completed: {reportDialog.report.completedParticipants}</p>
              <p className="section-text">In progress: {reportDialog.report.inProgressParticipants}</p>
              <p className="section-text">Not started: {reportDialog.report.notStartedParticipants}</p>
              <p className="section-text">Total submissions: {reportDialog.report.totalSubmissions}</p>
              <p className="section-text">
                Completion rate: {reportDialog.report.totalParticipants > 0
                  ? Math.round((reportDialog.report.completedParticipants / reportDialog.report.totalParticipants) * 100)
                  : 0}%
              </p>
              <p className="section-text">Generated: {new Date(reportDialog.report.generatedAtUtc).toLocaleString()}</p>
            </div>

            {reportDialog.overall && (
              <div className="view-as-wrap" style={{ marginTop: "0.75rem" }}>
                <h3 className="section-title">Aggregate Snapshot</h3>
                <p className="section-text">Completed: {reportDialog.overall.completed}</p>
                <p className="section-text">In progress: {reportDialog.overall.inProgress}</p>
                <p className="section-text">Not started: {reportDialog.overall.notStarted}</p>
              </div>
            )}

            <div className="feedback-actions" style={{ marginTop: "1rem" }}>
              <button
                className="pagination-button"
                type="button"
                onClick={() => setReportDialog({ open: false, campaign: null, report: null, overall: null })}
              >
                Close Report
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default Campaigns;
