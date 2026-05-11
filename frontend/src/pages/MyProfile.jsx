import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { getMyProfile, syncMe } from "../services/userService";

function MyProfile() {
  const { instance, accounts } = useMsal();
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const loadProfile = async () => {
      try {
        if (!accounts.length) return;

        setError("");
        setIsLoading(true);

        await syncMe(instance, accounts[0]);
        const data = await getMyProfile(instance, accounts[0]);

        console.log("My profile:", data);
        setProfile(data);
      } catch (err) {
        console.error(err);
        setError("Could not load profile.");
      } finally {
        setIsLoading(false);
      }
    };

    loadProfile();
  }, [instance, accounts]);

  if (error) {
    return <p className="error-text">{error}</p>;
  }

  if (isLoading || !profile) {
    return <p className="section-text">Loading...</p>;
  }

  const displayName =
    profile.displayName ||
    `${profile.firstName || ""} ${profile.lastName || ""}`.trim() ||
    profile.email ||
    "-";

  const initials = displayName
    .split(" ")
    .filter(Boolean)
    .map((part) => part[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return (
    <div className="content-card">
      <h1 className="section-title">My Profile</h1>
      <p className="section-text">
        View your personal information and employee details.
      </p>

      <div className="profile-card">
        <div className="profile-avatar">{initials}</div>

        <div className="profile-info">
          <div className="profile-row">
            <span className="profile-label">Name</span>
            <span className="profile-value">{displayName}</span>
          </div>

          <div className="profile-row">
            <span className="profile-label">Email</span>
            <span className="profile-value">{profile.email || "-"}</span>
          </div>

          <div className="profile-row">
            <span className="profile-label">Role</span>
            <span className="profile-value">{profile.role || "-"}</span>
          </div>

          <div className="profile-row">
            <span className="profile-label">First Name</span>
            <span className="profile-value">{profile.firstName || "-"}</span>
          </div>

          <div className="profile-row">
            <span className="profile-label">Last Name</span>
            <span className="profile-value">{profile.lastName || "-"}</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default MyProfile;