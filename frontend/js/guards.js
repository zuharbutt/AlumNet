// Call at top of every protected page
function requireAuth(allowedRoles) {
    if (!isLoggedIn()) { window.location.href = '/frontend/login.html'; return; }
    const user = getUser();
    if (allowedRoles && !allowedRoles.includes(user?.role)) {
        window.location.href = '/frontend/unauthorized.html';
    }
}
