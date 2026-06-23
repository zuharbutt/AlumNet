async function apiFetch(endpoint, options = {}) {
    const token = getToken();
    const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const res = await fetch(`${API_BASE}${endpoint}`, { ...options, headers });
    const body = await res.json().catch(() => ({}));

    if (!res.ok) throw new Error(body.message || `HTTP ${res.status}`);
    return body;
}
