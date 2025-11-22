// Common utility functions for all pages

// Initialize user menu with admin link check
async function initUserMenu() {
    const user = getCurrentUser();
    if (!user) {
        return;
    }

    // Update user name if element exists
    const userNameEl = document.getElementById('user-name');
    if (userNameEl) {
        userNameEl.textContent = user.fullName || user.username;
    }

    // Check admin role and show admin link
    const adminLinkEl = document.getElementById('admin-link');
    if (adminLinkEl) {
        try {
            // Refresh user info from API to get latest roles
            const currentUser = await getCurrentUserFromApi();
            if (currentUser && currentUser.roles && currentUser.roles.includes('Admin')) {
                adminLinkEl.classList.remove('hidden');
            } else if (user.roles && user.roles.includes('Admin')) {
                // Fallback: check roles from localStorage
                adminLinkEl.classList.remove('hidden');
            }
        } catch (error) {
            console.error('Error loading user info:', error);
            // Fallback: check roles from localStorage
            if (user.roles && user.roles.includes('Admin')) {
                adminLinkEl.classList.remove('hidden');
            }
        }
    }

    // Check staff role and show staff link
    const staffLinkEl = document.getElementById('staff-link');
    if (staffLinkEl) {
        try {
            // Refresh user info from API to get latest roles
            const currentUser = await getCurrentUserFromApi();
            if (currentUser && currentUser.roles && (currentUser.roles.includes('Staff') || currentUser.roles.includes('Admin'))) {
                staffLinkEl.classList.remove('hidden');
            } else if (user.roles && (user.roles.includes('Staff') || user.roles.includes('Admin'))) {
                // Fallback: check roles from localStorage
                staffLinkEl.classList.remove('hidden');
            }
        } catch (error) {
            console.error('Error loading user info:', error);
            // Fallback: check roles from localStorage
            if (user.roles && (user.roles.includes('Staff') || user.roles.includes('Admin'))) {
                staffLinkEl.classList.remove('hidden');
            }
        }
    }
}

