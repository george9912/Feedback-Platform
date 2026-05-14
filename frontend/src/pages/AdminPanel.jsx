import { useEffect, useMemo, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { getAllUsersForPicker, updateUserRole } from "../services/userService";
import "../styles/dashboard.css";

const ROLE_OPTIONS = ["EMPLOYEE", "HR", "ADMIN"];
const ADMIN_PANEL_PASSWORD = import.meta.env.VITE_ADMIN_PANEL_PASSWORD || "admin123";
const PAGE_SIZE = 10;

function AdminPanel() {
  const { instance, accounts } = useMsal();
  const [password, setPassword] = useState("");
  const [isUnlocked, setIsUnlocked] = useState(false);
  const [users, setUsers] = useState([]);
  const [roleDrafts, setRoleDrafts] = useState({});
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  const sortedUsers = useMemo(() => {
    return [...users].sort((a, b) => {
      const aName = a.displayName || `${a.firstName || ""} ${a.lastName || ""}`.trim();
      const bName = b.displayName || `${b.firstName || ""} ${b.lastName || ""}`.trim();
      return aName.localeCompare(bName);
    });
  }, [users]);

  const loadUsers = async () => {
    if (!accounts.length || !isUnlocked) return;

    setIsLoading(true);
    setError("");
    setMessage("");

    try {
      const data = await getAllUsersForPicker(instance, accounts[0]);
      setUsers(data);

      const initialDrafts = {};
      data.forEach((u) => {
        initialDrafts[u.id] = (u.role || "EMPLOYEE").toUpperCase();
      });
      setRoleDrafts(initialDrafts);
      setCurrentPage(1);
    } catch (err) {
      console.error(err);
      setError("Could not load users.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, [instance, accounts, isUnlocked]);

  const changedUsers = useMemo(() => {
    return sortedUsers.filter((u) => {
      const oldRole = (u.role || "EMPLOYEE").toUpperCase();
      const newRole = (roleDrafts[u.id] || "EMPLOYEE").toUpperCase();
      return oldRole !== newRole;
    });
  }, [sortedUsers, roleDrafts]);

  const totalPages = Math.max(1, Math.ceil(sortedUsers.length / PAGE_SIZE));
  const pageUsers = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return sortedUsers.slice(start, start + PAGE_SIZE);
  }, [sortedUsers, currentPage]);

  const startIndex = sortedUsers.length ? (currentPage - 1) * PAGE_SIZE + 1 : 0;
  const endIndex = Math.min(currentPage * PAGE_SIZE, sortedUsers.length);

  const unlockPanel = () => {
    setError("");
    setMessage("");

    if (password === ADMIN_PANEL_PASSWORD) {
      setIsUnlocked(true);
      return;
    }

    setError("Invalid admin password.");
  };

  const saveRoleChanges = async () => {
    if (!changedUsers.length) {
      setMessage("No role changes to save.");
      setError("");
      return;
    }

    const confirmationLines = changedUsers.map((u) => {
      const name = u.displayName || `${u.firstName || ""} ${u.lastName || ""}`.trim() || u.email || "this user";
      const oldRole = (u.role || "EMPLOYEE").toUpperCase();
      const newRole = (roleDrafts[u.id] || "EMPLOYEE").toUpperCase();
      return `Are you sure you want to change the role of \"${name}\" from \"${oldRole}\" to \"${newRole}\"?`;
    });

    const confirmed = window.confirm(confirmationLines.join("\n"));
    if (!confirmed) {
      return;
    }

    setIsLoading(true);
    setError("");
    setMessage("");

    try {
      for (const user of changedUsers) {
        const nextRole = (roleDrafts[user.id] || "EMPLOYEE").toUpperCase();
        await updateUserRole(instance, accounts[0], user, nextRole);
      }

      setMessage(`Successfully updated ${changedUsers.length} role${changedUsers.length > 1 ? "s" : ""}.`);
      await loadUsers();
    } catch (err) {
      console.error(err);
      setError(`Could not update role. ${err?.message || ""}`.trim());
    } finally {
      setIsLoading(false);
    }
  };

  if (!isUnlocked) {
    return (
      <div className="content-card">
        <h1 className="section-title">Admin Panel</h1>
        <p className="section-text">Enter the admin password to manage user roles.</p>

        <div className="admin-lock-row">
          <input
            className="search-input"
            type="password"
            placeholder="Admin password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <button type="button" className="refresh-users-button" onClick={unlockPanel}>
            Unlock
          </button>
        </div>

        {error && <p className="error-text">{error}</p>}
      </div>
    );
  }

  return (
    <div className="content-card">
      <h1 className="section-title">Admin Panel</h1>
      <p className="section-text">Manage roles for existing users. Department and manager are placeholders for now.</p>

      {message && <p className="feedback-success">{message}</p>}
      {error && <p className="error-text">{error}</p>}

      {isLoading && <p className="section-text">Loading...</p>}

      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Department</th>
              <th>Role</th>
              <th>Manager</th>
            </tr>
          </thead>
          <tbody>
            {pageUsers.map((u) => {
              const name = u.displayName || `${u.firstName || ""} ${u.lastName || ""}`.trim() || "-";
              return (
                <tr key={u.id}>
                  <td>{name}</td>
                  <td>{u.email || "-"}</td>
                  <td>-</td>
                  <td>
                    <select
                      className="feedback-select"
                      value={roleDrafts[u.id] || "EMPLOYEE"}
                      onChange={(e) => {
                        const value = e.target.value;
                        setRoleDrafts((prev) => ({ ...prev, [u.id]: value }));
                      }}
                    >
                      {ROLE_OPTIONS.map((role) => (
                        <option key={role} value={role}>
                          {role}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>-</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <div className="pagination-wrap">
        <div className="pagination-info">
          Showing {startIndex}-{endIndex} of {sortedUsers.length} users
        </div>

        <div className="pagination-controls">
          <button
            type="button"
            className="pagination-button"
            onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
            disabled={currentPage === 1 || isLoading}
          >
            Previous
          </button>

          <span className="pagination-page-indicator">
            Page {currentPage} of {totalPages}
          </span>

          <button
            type="button"
            className="pagination-button"
            onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
            disabled={currentPage === totalPages || isLoading}
          >
            Next
          </button>
        </div>
      </div>

      <div className="admin-save-wrap">
        <p className="section-text admin-save-info">
          Pending changes: {changedUsers.length}
        </p>
        <button
          type="button"
          className="refresh-users-button"
          onClick={saveRoleChanges}
          disabled={isLoading || !changedUsers.length}
        >
          {isLoading ? "Saving..." : "Save Changes"}
        </button>
      </div>
    </div>
  );
}

export default AdminPanel;
