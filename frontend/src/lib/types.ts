// Role-based access – aligned with PRD §4
export type Role = "admin" | "clinician" | "patient" | "caregiver";

// Vital metrics – PRD §5.1
export type VitalType =
  | "blood_pressure"
  | "heart_rate"
  | "blood_glucose"
  | "oxygen_saturation"
  | "weight";

export interface VitalReading {
  id: string;
  patientId: string;
  type: VitalType;
  value: number;
  valueSecondary?: number; // e.g. diastolic for BP
  unit: string;
  recordedAt: string;
  source: "manual" | "system";
  createdAt: string;
}

// Alert lifecycle – PRD §5.2.2
export type AlertState =
  | "flagged"
  | "acknowledged"
  | "escalated"
  | "resolved"
  | "archived";

export type AlertSeverity = "critical" | "high" | "moderate" | "normal";

export interface Alert {
  id: string;
  patientId: string;
  patientName?: string;
  vitalType: VitalType;
  severity: AlertSeverity;
  state: AlertState;
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
}

// User (simplified for frontend)
export interface User {
  id: string;
  email: string;
  role: Role;
  name: string;
}

// Caregiver link – PRD §5.6
export interface CaregiverLink {
  id: string;
  patientId: string;
  caregiverId: string;
  caregiverName?: string;
  consentedAt: string;
  revokedAt?: string;
}
