function renderNavbar() {
    const user = getUser();
    if (!user) return;
    const nav = document.getElementById('navbar');
    if (!nav) return;

    const dashLink = {
        Admin:   '/frontend/pages/admin/dashboard.html',
        Alumni:  '/frontend/pages/alumni/dashboard.html',
        Student: '/frontend/pages/student/dashboard.html'
    }[user.role] || '#';

    const roleLinks = {
        Alumni: `
            <a href="/frontend/pages/alumni/profile.html">Edit Profile</a>
            <a href="/frontend/pages/alumni/inbox.html">Mentorship</a>
            <a href="/frontend/pages/shared/events.html">Events</a>
            <a href="/frontend/pages/shared/my-tickets.html">My Tickets</a>
            <a href="/frontend/pages/shared/donations.html">Donate</a>
            <a href="/frontend/pages/alumni/my-donations.html">My Donations</a>`,
        Student: `
            <a href="/frontend/pages/student/alumni-directory.html">Browse Alumni</a>
            <a href="/frontend/pages/student/profile.html">Edit Profile</a>
            <a href="/frontend/pages/student/my-requests.html">My Requests</a>
            <a href="/frontend/pages/shared/events.html">Events</a>
            <a href="/frontend/pages/shared/my-tickets.html">My Tickets</a>`,
        Admin: `
            <a href="/frontend/pages/admin/events.html">Events</a>
            <a href="/frontend/pages/admin/campaigns.html">Campaigns</a>
            <a href="/frontend/pages/admin/donations.html">Donations</a>
            <a href="/frontend/pages/admin/users.html">Users</a>
            <a href="/frontend/pages/admin/mentorship.html">Mentorship</a>`
    }[user.role] || '';

    nav.innerHTML = `
      <div class="nav-brand">AlumniMS</div>
      <div class="nav-links">
        <a href="${dashLink}">Dashboard</a>
        ${roleLinks}
        <span class="nav-user">${user.fullName} (${user.role})</span>
        <button onclick="logout()">Logout</button>
      </div>`;
}
