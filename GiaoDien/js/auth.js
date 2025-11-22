// Check if user is authenticated
function isAuthenticated() {
    return !!getToken();
}

// Get current user from localStorage
function getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
}

// Check if current user is admin
function isAdmin() {
    const user = getCurrentUser();
    return user && user.roles && user.roles.includes('Admin');
}

// Save user to localStorage
function saveUser(user) {
    localStorage.setItem('user', JSON.stringify(user));
}

// Register
async function register(username, email, password, fullName, phoneNumber) {
    try {
        const response = await post('/auth/register', {
            username,
            email,
            password,
            fullName,
            phoneNumber
        });

        if (response.success && response.data) {
            saveTokens(response.data.token, response.data.refreshToken);
            saveUser(response.data.user);
            return { success: true, data: response.data };
        }

        return { success: false, message: response.message || 'Registration failed' };
    } catch (error) {
        return { success: false, message: error.message || 'Registration failed' };
    }
}

// Login
async function login(username, password) {
    try {
        const response = await post('/auth/login', {
            username,
            password
        });

        if (response.success && response.data) {
            saveTokens(response.data.token, response.data.refreshToken);
            saveUser(response.data.user);
            return { success: true, data: response.data };
        }

        return { success: false, message: response.message || 'Login failed' };
    } catch (error) {
        return { success: false, message: error.message || 'Login failed' };
    }
}

// Logout
async function logout() {
    try {
        const refreshToken = getRefreshToken();
        if (refreshToken) {
            await post('/auth/logout', { refreshToken }, true);
        }
    } catch (error) {
        console.error('Logout error:', error);
    } finally {
        clearTokens();
        // Redirect to login page - use relative path
        const currentPath = window.location.pathname;
        if (currentPath.includes('/pages/')) {
            window.location.href = 'login.html';
        } else {
            window.location.href = 'pages/login.html';
        }
    }
}

// Forgot password
async function forgotPassword(email) {
    try {
        const response = await post('/auth/forgot-password', { email });
        return { success: response.success, message: response.message };
    } catch (error) {
        return { success: false, message: error.message || 'Request failed' };
    }
}

// Reset password
async function resetPassword(email, token, newPassword) {
    try {
        const response = await post('/auth/reset-password', {
            email,
            token,
            newPassword
        });
        return { success: response.success, message: response.message };
    } catch (error) {
        return { success: false, message: error.message || 'Reset failed' };
    }
}

// Get current user from API
async function getCurrentUserFromApi() {
    try {
        const response = await get('/auth/me', true);
        if (response.success && response.data) {
            saveUser(response.data);
            return response.data;
        }
        return null;
    } catch (error) {
        console.error('Get current user failed:', error);
        return null;
    }
}

