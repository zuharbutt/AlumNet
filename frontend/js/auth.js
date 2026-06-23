function getToken()  { return localStorage.getItem(TOKEN_KEY); }
function getUser()   { try { return JSON.parse(localStorage.getItem(USER_KEY) || 'null'); } catch { return null; } }
function isLoggedIn(){ return !!getToken(); }

function saveAuth(data) {
    localStorage.setItem(TOKEN_KEY, data.token);
    localStorage.setItem(USER_KEY,  JSON.stringify({ userId: data.userId, role: data.role, fullName: data.fullName, email: data.email }));
}

function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    window.location.href = '/frontend/login.html';
}

function redirectToDashboard(role) {
    const map = { Admin: '/frontend/pages/admin/dashboard.html', Alumni: '/frontend/pages/alumni/dashboard.html', Student: '/frontend/pages/student/dashboard.html' };
    window.location.href = map[role] || '/frontend/login.html';
}
