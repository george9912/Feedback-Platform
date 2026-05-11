import { useEffect, useMemo, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { getUsers, syncTenantUsers } from "../services/userService";

const USERS_PER_PAGE = 12;

function Directory() {
  const { instance, accounts } = useMsal();
  const [users, setUsers] = useState([]);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);

  const loadUsers = async () => {
    try {
      if (!accounts.length) return;

      setError("");
      setIsLoading(true);

      const data = await getUsers(instance, accounts[0]);
      setUsers(data);
    } catch (err) {
      console.error(err);
      setError("Could not load users.");
    } finally {
      setIsLoading(false);
    }
  };

  const refreshUsers = async () => {
    try {
      if (!accounts.length) return;

      setError("");
      setIsRefreshing(true);

      await syncTenantUsers(instance, accounts[0]);
      const data = await getUsers(instance, accounts[0]);
      setUsers(data);
      setCurrentPage(1);
    } catch (err) {
      console.error(err);
      setError("Could not refresh users.");
    } finally {
      setIsRefreshing(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, [instance, accounts]);

  const filteredUsers = useMemo(() => {
    return users.filter((user) => {
      const displayName =
        user.displayName ||
        `${user.firstName || ""} ${user.lastName || ""}`.trim();

      const email = user.email || "";

      return (
        displayName.toLowerCase().includes(search.toLowerCase()) ||
        email.toLowerCase().includes(search.toLowerCase())
      );
    });
  }, [users, search]);

  useEffect(() => {
    setCurrentPage(1);
  }, [search]);

  const totalPages = Math.max(1, Math.ceil(filteredUsers.length / USERS_PER_PAGE));
  const startIndex = (currentPage - 1) * USERS_PER_PAGE;
  const paginatedUsers = filteredUsers.slice(startIndex, startIndex + USERS_PER_PAGE);

  const goToPreviousPage = () => {
    setCurrentPage((prev) => Math.max(1, prev - 1));
  };

  const goToNextPage = () => {
    setCurrentPage((prev) => Math.min(totalPages, prev + 1));
  };

  return (
    <div className="content-card">
      <div className="page-header-block">
        <h1 className="section-title">Employee Directory</h1>
        <p className="section-text">
          Browse and search employees across the organization.
        </p>
      </div>

      <div className="directory-toolbar">
        <input
          type="text"
          className="search-input"
          placeholder="Search by name or email..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <button
          type="button"
          className="refresh-users-button"
          onClick={refreshUsers}
          disabled={isRefreshing || isLoading || !accounts.length}
        >
          {isRefreshing ? "Refreshing..." : "Refresh Users"}
        </button>
      </div>

      {error && <p className="error-text">{error}</p>}

      {isLoading && <p className="section-text">Loading users...</p>}

      {!error && !isLoading && filteredUsers.length === 0 && (
        <p className="section-text">No users found.</p>
      )}

      <div className="directory-grid">
        {paginatedUsers.map((user) => {
          const displayName =
            user.displayName ||
            `${user.firstName || ""} ${user.lastName || ""}`.trim() ||
            "Unknown User";

          const initials = displayName
            .split(" ")
            .filter(Boolean)
            .map((part) => part[0])
            .join("")
            .slice(0, 2)
            .toUpperCase();

          return (
            <div className="user-card" key={user.id}>
              <div className="user-card-avatar">{initials}</div>

              <div className="user-card-content">
                <h3 className="user-card-name">{displayName}</h3>
                <p className="user-card-email">{user.email || "-"}</p>

                {user.role && (
                  <span className="user-role-badge">{user.role}</span>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {!error && !isLoading && filteredUsers.length > 0 && (
        <div className="pagination-wrap">
          <div className="pagination-info">
            Showing {startIndex + 1}-{Math.min(startIndex + USERS_PER_PAGE, filteredUsers.length)} of{" "}
            {filteredUsers.length} users
          </div>

          <div className="pagination-controls">
            <button
              type="button"
              className="pagination-button"
              onClick={goToPreviousPage}
              disabled={currentPage === 1}
            >
              Previous
            </button>

            <span className="pagination-page-indicator">
              Page {currentPage} of {totalPages}
            </span>

            <button
              type="button"
              className="pagination-button"
              onClick={goToNextPage}
              disabled={currentPage === totalPages}
            >
              Next
            </button>
          </div>
        </div>
      )}

      {isRefreshing && (
  <div className="spinner-overlay">
    <div className="spinner-container">
      <div className="fh-logo">FH</div>
      <div className="spinner-ring"></div>
      <p className="spinner-text">Syncing users...</p>
    </div>
  </div>
)}
    </div>
  );
}

export default Directory;