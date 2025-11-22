const API_BASE_URL = "https://localhost:7067/api";

// Get token from localStorage
function getToken() {
    return localStorage.getItem('token');
}

// Get refresh token from localStorage
function getRefreshToken() {
    return localStorage.getItem('refreshToken');
}

// Save tokens to localStorage
function saveTokens(token, refreshToken) {
    localStorage.setItem('token', token);
    localStorage.setItem('refreshToken', refreshToken);
}

// Clear tokens
function clearTokens() {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
}

// Handle 401 - try refresh token
async function handleUnauthorized() {
    const refreshToken = getRefreshToken();
    if (!refreshToken) {
        clearTokens();
        window.location.href = '/frontend/pages/login.html';
        return null;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/auth/refresh-token`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ refreshToken })
        });

        if (response.ok) {
            const data = await response.json();
            if (data.success && data.data) {
                saveTokens(data.data.token, data.data.refreshToken);
                return data.data.token;
            }
        }
    } catch (error) {
        console.error('Refresh token failed:', error);
    }

    clearTokens();
    window.location.href = '/frontend/pages/login.html';
    return null;
}

// Generic GET request
async function get(url, authRequired = false) {
    const headers = {
        'Content-Type': 'application/json'
    };

    if (authRequired) {
        const token = getToken();
        if (!token) {
            await handleUnauthorized();
            throw new Error('Unauthorized');
        }
        headers['Authorization'] = `Bearer ${token}`;
    }

    try {
        const response = await fetch(`${API_BASE_URL}${url}`, {
            method: 'GET',
            headers: headers
        });

        if (response.status === 401 && authRequired) {
            const newToken = await handleUnauthorized();
            if (newToken) {
                headers['Authorization'] = `Bearer ${newToken}`;
                const retryResponse = await fetch(`${API_BASE_URL}${url}`, {
                    method: 'GET',
                    headers: headers
                });
                if (!retryResponse.ok) {
                    if (retryResponse.status === 403) {
                        throw new Error('Bạn không có quyền thực hiện thao tác này. Vui lòng đăng nhập với tài khoản Admin.');
                    }
                    throw new Error(`HTTP error! status: ${retryResponse.status}`);
                }
                return await retryResponse.json();
            }
        }

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('GET request failed:', error);
        throw error;
    }
}

// Generic POST request
async function post(url, body, authRequired = false) {
    const headers = {
        'Content-Type': 'application/json'
    };

    if (authRequired) {
        const token = getToken();
        if (!token) {
            await handleUnauthorized();
            throw new Error('Unauthorized');
        }
        headers['Authorization'] = `Bearer ${token}`;
    }

    try {
        const response = await fetch(`${API_BASE_URL}${url}`, {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(body)
        });

        if (response.status === 401 && authRequired) {
            const newToken = await handleUnauthorized();
            if (newToken) {
                headers['Authorization'] = `Bearer ${newToken}`;
                const retryResponse = await fetch(`${API_BASE_URL}${url}`, {
                    method: 'POST',
                    headers: headers,
                    body: JSON.stringify(body)
                });
                if (!retryResponse.ok) {
                    if (retryResponse.status === 403) {
                        throw new Error('Bạn không có quyền thực hiện thao tác này. Vui lòng đăng nhập với tài khoản Admin.');
                    }
                    throw new Error(`HTTP error! status: ${retryResponse.status}`);
                }
                return await retryResponse.json();
            }
        }

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            if (response.status === 403) {
                throw new Error('Bạn không có quyền thực hiện thao tác này. Vui lòng đăng nhập với tài khoản Admin.');
            }
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('POST request failed:', error);
        throw error;
    }
}

// Generic PUT request
async function put(url, body, authRequired = false) {
    const headers = {
        'Content-Type': 'application/json'
    };

    if (authRequired) {
        const token = getToken();
        if (!token) {
            await handleUnauthorized();
            throw new Error('Unauthorized');
        }
        headers['Authorization'] = `Bearer ${token}`;
    }

    try {
        const response = await fetch(`${API_BASE_URL}${url}`, {
            method: 'PUT',
            headers: headers,
            body: JSON.stringify(body)
        });

        if (response.status === 401 && authRequired) {
            const newToken = await handleUnauthorized();
            if (newToken) {
                headers['Authorization'] = `Bearer ${newToken}`;
                const retryResponse = await fetch(`${API_BASE_URL}${url}`, {
                    method: 'PUT',
                    headers: headers,
                    body: JSON.stringify(body)
                });
                if (!retryResponse.ok) {
                    if (retryResponse.status === 403) {
                        throw new Error('Bạn không có quyền thực hiện thao tác này. Vui lòng đăng nhập với tài khoản Admin.');
                    }
                    throw new Error(`HTTP error! status: ${retryResponse.status}`);
                }
                return await retryResponse.json();
            }
        }

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            if (response.status === 403) {
                throw new Error('Bạn không có quyền thực hiện thao tác này. Vui lòng đăng nhập với tài khoản Admin.');
            }
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('PUT request failed:', error);
        throw error;
    }
}

// Upload file
async function uploadFile(file, url, authRequired = false) {
    const formData = new FormData();
    formData.append('file', file);

    const headers = {};

    if (authRequired) {
        const token = getToken();
        if (!token) {
            await handleUnauthorized();
            throw new Error('Unauthorized');
        }
        headers['Authorization'] = `Bearer ${token}`;
    }

    try {
        const response = await fetch(`${API_BASE_URL}${url}`, {
            method: 'POST',
            headers: headers,
            body: formData
        });

        if (response.status === 401 && authRequired) {
            const newToken = await handleUnauthorized();
            if (newToken) {
                headers['Authorization'] = `Bearer ${newToken}`;
                const retryResponse = await fetch(`${API_BASE_URL}${url}`, {
                    method: 'POST',
                    headers: headers,
                    body: formData
                });
                if (!retryResponse.ok) {
                    if (retryResponse.status === 403) {
                        throw new Error('Bạn không có quyền thực hiện thao tác này. Vui lòng đăng nhập với tài khoản Admin.');
                    }
                    throw new Error(`HTTP error! status: ${retryResponse.status}`);
                }
                return await retryResponse.json();
            }
        }

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            if (response.status === 403) {
                throw new Error('Bạn không có quyền thực hiện thao tác này. Vui lòng đăng nhập với tài khoản Admin.');
            }
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('Upload file failed:', error);
        throw error;
    }
}

// Generic DELETE request
async function del(url, authRequired = false) {
    const headers = {
        'Content-Type': 'application/json'
    };

    if (authRequired) {
        const token = getToken();
        if (!token) {
            await handleUnauthorized();
            throw new Error('Unauthorized');
        }
        headers['Authorization'] = `Bearer ${token}`;
    }

    try {
        const response = await fetch(`${API_BASE_URL}${url}`, {
            method: 'DELETE',
            headers: headers
        });

        if (response.status === 401 && authRequired) {
            const newToken = await handleUnauthorized();
            if (newToken) {
                headers['Authorization'] = `Bearer ${newToken}`;
                const retryResponse = await fetch(`${API_BASE_URL}${url}`, {
                    method: 'DELETE',
                    headers: headers
                });
                if (!retryResponse.ok) {
                    if (retryResponse.status === 403) {
                        throw new Error('Bạn không có quyền thực hiện thao tác này. Vui lòng đăng nhập với tài khoản Admin.');
                    }
                    throw new Error(`HTTP error! status: ${retryResponse.status}`);
                }
                return await retryResponse.json();
            }
        }

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('DELETE request failed:', error);
        throw error;
    }
}

