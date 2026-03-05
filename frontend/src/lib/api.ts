/**
 * API client for VitalCare backend.
 * Uses credentials: 'include' for cookie-based auth (HIPAA: httpOnly cookie).
 */

const getBaseUrl = (): string => {
  if (typeof window !== "undefined")
    return process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
  return process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
};

export type ApiError = { message?: string; status: number };

async function handleResponse<T>(res: Response): Promise<T> {
  if (res.status === 204) return undefined as T;
  const text = await res.text();
  if (!res.ok) {
    let message = res.statusText;
    try {
      const j = JSON.parse(text) as { message?: string };
      if (j.message) message = j.message;
    } catch {
      if (text) message = text;
    }
    throw { message, status: res.status } as ApiError;
  }
  if (!text) return undefined as T;
  return JSON.parse(text) as T;
}

async function request<T>(
  path: string,
  options: RequestInit & { body?: object } = {}
): Promise<T> {
  const { body, ...rest } = options;
  const url = path.startsWith("http") ? path : `${getBaseUrl()}${path}`;
  const headers: HeadersInit = {
    "Content-Type": "application/json",
    ...(rest.headers as Record<string, string>),
  };
  const res = await fetch(url, {
    ...rest,
    headers,
    credentials: "include",
    body: body !== undefined ? JSON.stringify(body) : rest.body,
  });
  return handleResponse<T>(res);
}

export const api = {
  get: <T>(path: string) => request<T>(path, { method: "GET" }),
  post: <T>(path: string, body?: object) =>
    request<T>(path, { method: "POST", body }),
  put: <T>(path: string, body?: object) =>
    request<T>(path, { method: "PUT", body }),
  patch: <T>(path: string, body?: object) =>
    request<T>(path, { method: "PATCH", body }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};

// Auth
export type LoginResponse = { token: string; user: { id: string; email: string; role: string; name: string } };
export type UserDto = { id: string; email: string; role: string; name: string };

export const authApi = {
  login: (email: string, password: string) =>
    api.post<LoginResponse>("/api/auth/login", { email, password }),
  me: () => api.get<UserDto>("/api/auth/me"),
  logout: () => api.post<undefined>("/api/auth/logout"),
};

// Users (admin/clinician)
export const usersApi = {
  list: (params?: { role?: string; search?: string; limit?: number }) => {
    const q = new URLSearchParams();
    if (params?.role) q.set("role", params.role);
    if (params?.search) q.set("search", params.search);
    if (params?.limit != null) q.set("limit", String(params.limit));
    const query = q.toString();
    return api.get<UserDto[]>(`/api/users${query ? `?${query}` : ""}`);
  },
  get: (id: string) => api.get<UserDto>(`/api/users/${id}`),
};

// Vitals
export type VitalReadingDto = {
  id: string;
  patientId: string;
  type: string;
  value: number;
  valueSecondary?: number;
  unit: string;
  recordedAt: string;
  source: string;
  createdAt: string;
};

export const vitalsApi = {
  list: (params?: {
    patientId?: string;
    type?: string;
    from?: string;
    to?: string;
    limit?: number;
  }) => {
    const q = new URLSearchParams();
    if (params?.patientId) q.set("patientId", params.patientId);
    if (params?.type) q.set("type", params.type);
    if (params?.from) q.set("from", params.from);
    if (params?.to) q.set("to", params.to);
    if (params?.limit != null) q.set("limit", String(params.limit));
    const query = q.toString();
    return api.get<VitalReadingDto[]>(`/api/vitals${query ? `?${query}` : ""}`);
  },
  submit: (body: Record<string, unknown>) =>
    api.post<VitalReadingDto>("/api/vitals", body),
};

// Alerts
export type AlertDto = {
  id: string;
  patientId: string;
  patientName?: string;
  vitalType: string;
  severity: string;
  state: string;
  value: number;
  valueSecondary?: number;
  unit: string;
  ruleId?: string;
  acknowledgedAt?: string;
  acknowledgedBy?: string;
  escalatedAt?: string;
  resolvedAt?: string;
  resolvedBy?: string;
  slaDueAt: string;
  createdAt: string;
  clinicalNote?: string;
};

export const alertsApi = {
  list: (params?: {
    patientId?: string;
    state?: string;
    severity?: string;
    limit?: number;
  }) => {
    const q = new URLSearchParams();
    if (params?.patientId) q.set("patientId", params.patientId);
    if (params?.state) q.set("state", params.state);
    if (params?.severity) q.set("severity", params.severity);
    if (params?.limit != null) q.set("limit", String(params.limit));
    const query = q.toString();
    return api.get<AlertDto[]>(`/api/alerts${query ? `?${query}` : ""}`);
  },
  get: (id: string) => api.get<AlertDto>(`/api/alerts/${id}`),
  update: (id: string, body: { state?: string; clinicalNote?: string }) =>
    api.patch<AlertDto>(`/api/alerts/${id}`, body),
};

// Alert rules (admin)
export type AlertRuleDto = {
  id: string;
  vitalType: string;
  severity: string;
  operator: string;
  thresholdMin?: number;
  thresholdMax?: number;
  isActive: boolean;
};

export const alertRulesApi = {
  list: (params?: { vitalType?: string; activeOnly?: boolean }) => {
    const q = new URLSearchParams();
    if (params?.vitalType) q.set("vitalType", params.vitalType);
    if (params?.activeOnly != null) q.set("activeOnly", String(params.activeOnly));
    const query = q.toString();
    return api.get<AlertRuleDto[]>(`/api/alertrules${query ? `?${query}` : ""}`);
  },
  get: (id: string) => api.get<AlertRuleDto>(`/api/alertrules/${id}`),
  create: (body: {
    vitalType: string;
    severity: string;
    operator: string;
    thresholdMin?: number;
    thresholdMax?: number;
  }) => api.post<AlertRuleDto>("/api/alertrules", body),
  update: (
    id: string,
    body: {
      vitalType: string;
      severity: string;
      operator: string;
      thresholdMin?: number;
      thresholdMax?: number;
    }
  ) => api.put<AlertRuleDto>(`/api/alertrules/${id}`, body),
  delete: (id: string) => api.delete<undefined>(`/api/alertrules/${id}`),
};

// Caregiver links
export type CaregiverLinkDto = {
  id: string;
  patientId: string;
  caregiverId: string;
  caregiverName?: string;
  consentedAt: string;
  revokedAt?: string;
};

export const caregiverLinksApi = {
  list: (params?: { patientId?: string; caregiverId?: string; activeOnly?: boolean }) => {
    const q = new URLSearchParams();
    if (params?.patientId) q.set("patientId", params.patientId);
    if (params?.caregiverId) q.set("caregiverId", params.caregiverId);
    if (params?.activeOnly != null) q.set("activeOnly", String(params.activeOnly));
    const query = q.toString();
    return api.get<CaregiverLinkDto[]>(`/api/caregiverlinks${query ? `?${query}` : ""}`);
  },
  invite: (caregiverEmail: string, patientId?: string) =>
    api.post<CaregiverLinkDto>("/api/caregiverlinks", {
      caregiverEmail,
      patientId: patientId || undefined,
    }),
  revoke: (id: string) =>
    api.post<undefined>(`/api/caregiverlinks/${id}/revoke`, {}),
};

// Audit
export type AuditLogDto = {
  id: string;
  userId?: string;
  userEmail?: string;
  role: string;
  resource: string;
  action: string;
  resourceId?: string;
  details?: string;
  ipAddress?: string;
  timestamp: string;
};

export const auditApi = {
  list: (params?: {
    userId?: string;
    resource?: string;
    resourceId?: string;
    from?: string;
    to?: string;
    limit?: number;
  }) => {
    const q = new URLSearchParams();
    if (params?.userId) q.set("userId", params.userId);
    if (params?.resource) q.set("resource", params.resource);
    if (params?.resourceId) q.set("resourceId", params.resourceId);
    if (params?.from) q.set("from", params.from);
    if (params?.to) q.set("to", params.to);
    if (params?.limit != null) q.set("limit", String(params.limit));
    const query = q.toString();
    return api.get<AuditLogDto[]>(`/api/audit${query ? `?${query}` : ""}`);
  },
};
