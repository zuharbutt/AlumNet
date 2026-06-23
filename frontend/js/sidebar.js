/* sidebar.js — renders the sticky sidebar for Admin / Alumni / Student portals */

const SIDEBAR_NAV = {
  Admin: [
    { label: 'OVERVIEW', items: [
      { icon: '📊', text: 'Dashboard',   href: '/frontend/pages/admin/dashboard.html' },
    ]},
    { label: 'MANAGE', items: [
      { icon: '📅', text: 'Events',      href: '/frontend/pages/admin/events.html' },
      { icon: '📣', text: 'Campaigns',   href: '/frontend/pages/admin/campaigns.html' },
    ]},
    { label: 'USERS', items: [
      { icon: '👥', text: 'All Users',   href: '/frontend/pages/admin/users.html' },
      { icon: '🤝', text: 'Mentorship',  href: '/frontend/pages/admin/mentorship.html' },
      { icon: '💰', text: 'Donations',   href: '/frontend/pages/admin/donations.html' },
    ]},
    { label: 'REPORTS', items: [
      { icon: '📈', text: 'Reports',     href: '/frontend/pages/admin/reports.html' },
    ]},
  ],
  Alumni: [
    { label: 'OVERVIEW', items: [
      { icon: '📊', text: 'Dashboard',    href: '/frontend/pages/alumni/dashboard.html' },
    ]},
    { label: 'PROFILE', items: [
      { icon: '👤', text: 'Edit Profile', href: '/frontend/pages/alumni/profile.html' },
    ]},
    { label: 'MENTORSHIP', items: [
      { icon: '📥', text: 'My Inbox',     href: '/frontend/pages/alumni/inbox.html' },
    ]},
    { label: 'EVENTS & MORE', items: [
      { icon: '📅', text: 'Events',       href: '/frontend/pages/shared/events.html' },
      { icon: '🎫', text: 'My Tickets',   href: '/frontend/pages/shared/my-tickets.html' },
      { icon: '💳', text: 'Donate',       href: '/frontend/pages/shared/donations.html' },
      { icon: '💰', text: 'My Donations', href: '/frontend/pages/alumni/my-donations.html' },
    ]},
  ],
  Student: [
    { label: 'OVERVIEW', items: [
      { icon: '📊', text: 'Dashboard',     href: '/frontend/pages/student/dashboard.html' },
    ]},
    { label: 'PROFILE', items: [
      { icon: '👤', text: 'Edit Profile',  href: '/frontend/pages/student/profile.html' },
    ]},
    { label: 'ALUMNI', items: [
      { icon: '🔍', text: 'Browse Alumni', href: '/frontend/pages/student/alumni-directory.html' },
      { icon: '🤝', text: 'My Requests',  href: '/frontend/pages/student/my-requests.html' },
    ]},
    { label: 'EVENTS', items: [
      { icon: '📅', text: 'Events',        href: '/frontend/pages/shared/events.html' },
      { icon: '🎫', text: 'My Tickets',    href: '/frontend/pages/shared/my-tickets.html' },
    ]},
  ],
};

function renderSidebar(pageTitle) {
  const user = getUser();
  if (!user) return;

  document.body.classList.add('sidebar-layout');

  const sections = SIDEBAR_NAV[user.role] || [];
  const currentPath = window.location.pathname;

  const navHtml = sections.map(sec => `
    <div class="sb-section-label">${sec.label}</div>
    <nav>
      ${sec.items.map(item => `
        <a href="${item.href}" class="${currentPath.includes(item.href.split('/').pop().replace('.html','')) ? 'active' : ''}">
          <span class="sb-icon">${item.icon}</span>
          ${item.text}
        </a>`).join('')}
    </nav>`).join('');

  const initials = (user.fullName || 'U').split(' ').map(w => w[0]).slice(0,2).join('').toUpperCase();

  const sidebarHtml = `
    <a href="${getDashLink(user.role)}" class="sb-brand">
      <div class="brand-dot">A</div>
      AlumniMS
    </a>
    ${navHtml}
    <div class="sb-user">
      <div class="avatar">${initials}</div>
      <div class="user-info">
        <div class="user-name">${esc(user.fullName)}</div>
        <div class="user-role">${user.role}</div>
      </div>
      <button class="logout-btn" onclick="logout()" title="Logout">Logout</button>
    </div>`;

  const topbarHtml = `
    <div style="display:flex; align-items:center; gap:12px;">
      <button id="sidebar-toggle" onclick="toggleSidebar()">☰</button>
      <span class="topbar-title">${pageTitle || ''}</span>
    </div>
    <div class="topbar-user">
      <div class="avatar">${initials}</div>
      <span>${esc(user.fullName)}</span>
    </div>`;

  // Inject structure
  const sidebar = document.getElementById('sidebar');
  const topbar  = document.getElementById('topbar');
  if (sidebar) sidebar.innerHTML = sidebarHtml;
  if (topbar)  topbar.innerHTML  = topbarHtml;

  // Active link fix — match on full href
  document.querySelectorAll('#sidebar nav a').forEach(a => {
    a.classList.toggle('active', a.getAttribute('href') === currentPath ||
      window.location.href.includes(a.getAttribute('href').split('/frontend')[1] || '__none__'));
  });
}

function toggleSidebar() {
  document.getElementById('sidebar')?.classList.toggle('open');
}

function getDashLink(role) {
  return { Admin: '/frontend/pages/admin/dashboard.html', Alumni: '/frontend/pages/alumni/dashboard.html', Student: '/frontend/pages/student/dashboard.html' }[role] || '#';
}

function esc(str) {
  return String(str ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}
