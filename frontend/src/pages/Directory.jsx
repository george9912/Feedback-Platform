import { useCallback, useEffect, useRef, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { searchUsers, syncTenantUsers } from "../services/userService";

const PAGE_SIZE = 12;

function Directory({ onGiveFeedback }) {
  const { instance, accounts } = useMsal();
  const [users, setUsers] = useState([]);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const debounceRef = useRef(null);

  const loadUsers = useCallback(async (query, page) => {
    if (!accounts.length) return;
    try {
      setError("");
      setIsLoading(true);
      const data = await searchUsers(instance, accounts[0], query, page, PAGE_SIZE);
      setUsers(data.users);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
    } catch (err) {
      console.error(err);
      setError("Could not load users.");
    } finally {
      setIsLoading(false);
    }
  }, [instance, accounts]);

  // Initial load
  useEffect(() => {
    loadUsers("", 1);
  }, [loadUsers]);

  // Debounced search
  const handleSearchChange = (e) => {
    const value = e.target.value;
    setSearch(value);
    setCurrentPage(1);
    clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      loadUsers(value, 1);
    }, 350);
  };

  const handlePageChange = (newPage) => {
    setCurrentPage(newPage);
    loadUsers(search, newPage);
  };

  const refreshUsers = async () => {
    if (!accounts.length) return;
    try {
      setError("");
      setIsRefreshing(true);
      await syncTenantUsers(instance, accounts[0]);
      await loadUsers(search, 1);
      setCurrentPage(1);
    } catch (err) {
      console.error(err);
      setError("Could not refresh users.");
    } finally {
      setIsRefreshing(false);
    }
  };

  const startIndex = (currentPage - 1) * PAGE_SIZE + 1;
  const endIndex = Math.min(startIndex + PAGE_SIZE - 1, totalCount);

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
          onChange={handleSearchChange}
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

      {!error && !isLoading && users.length === 0 && (
        <p className="section-text">No users found.</p>
      )}

      <div className="directory-grid">
        {users.map((user) => {
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

                <div className="user-card-actions">
                  <button
                    type="button"
                    className="pagination-button"
                    onClick={() => onGiveFeedback?.(user)}
                  >
                    Give Feedback
                  </button>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {!error && !isLoading && totalCount > 0 && (
        <div className="pagination-wrap">
          <div className="pagination-info">
            Showing {startIndex}-{endIndex} of {totalCount} users
          </div>

          <div className="pagination-controls">
            <button
              type="button"
              className="pagination-button"
              onClick={() => handlePageChange(currentPage - 1)}
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
              onClick={() => handlePageChange(currentPage + 1)}
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