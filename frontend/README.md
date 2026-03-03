# VitalWatch – Frontend

HIPAA-aligned Remote Patient Monitoring & Alert Escalation platform. This repo is **frontend-first**: Next.js, TypeScript, Tailwind, React Query, and Zod.

## Design

- **Healthcare-focused**: Teal primary palette, clinical severity colors (critical/high/normal), clear typography (DM Sans), card-based layout.
- **Role-based**: Patient, Clinician, Admin, Caregiver with separate nav and dashboards.
- **Accessibility**: Focus rings, semantic HTML, readable contrast.

## Quick start

```bash
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000). Use **Sign in** and pick a role (Patient, Clinician, Admin, Caregiver) to enter the corresponding portal. No password required for demo.

## Stack

- **Next.js 14** (App Router)
- **TypeScript**
- **Tailwind CSS**
- **React Query** (TanStack Query)
- **Zod** (vitals validation)

## Structure

| Area | Description |
|------|-------------|
| `/` | Landing page |
| `/login` | Sign in + role selector (demo) |
| `/patient/*` | Patient portal: dashboard, submit vitals, history, alerts, caregivers, audit log |
| `/clinician/*` | Clinician: alert queue, SLA indicator, patients |
| `/admin/*` | Admin: users, thresholds, audit logs |
| `/caregiver/*` | Caregiver: view-only vitals (consent-based) |

## Vitals (MVP)

- Blood pressure (systolic/diastolic)
- Heart rate
- Blood glucose
- Oxygen saturation
- Weight

Validation via Zod in `src/lib/vitals-schema.ts`. Submission form in `src/components/vitals/VitalForm.tsx`.

## Auth (demo)

`AuthContext` holds mock login; role determines redirect. Replace with real JWT + refresh flow and 2FA for Admin/Clinician when backend is ready.

## Backend (planned)

- NestJS, PostgreSQL, Redis, BullMQ
- API enforcement: RBAC, patient-level isolation, audit logging

## Compliance

Aligned with HIPAA technical safeguards; audit and consent flows are prepared for backend integration.
