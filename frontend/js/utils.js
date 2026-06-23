function showError(id, msg)  { const el = document.getElementById(id); if (el) { el.textContent = msg; el.style.display = 'block'; } }
function clearError(id)      { const el = document.getElementById(id); if (el) { el.textContent = ''; el.style.display = 'none'; } }
function showToast(msg, type = 'success') {
    const t = document.createElement('div');
    t.className = `toast toast-${type}`;
    t.textContent = msg;
    document.body.appendChild(t);
    setTimeout(() => t.remove(), 3000);
}
