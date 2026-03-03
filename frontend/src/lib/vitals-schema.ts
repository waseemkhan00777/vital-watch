import { z } from "zod";

const vitalTypes = [
  "blood_pressure",
  "heart_rate",
  "blood_glucose",
  "oxygen_saturation",
  "weight",
] as const;

export const vitalTypeSchema = z.enum(vitalTypes);

// Blood pressure: systolic / diastolic
export const bloodPressureSchema = z.object({
  type: z.literal("blood_pressure"),
  systolic: z.number().min(70).max(250),
  diastolic: z.number().min(40).max(150),
  recordedAt: z.string().datetime().optional(),
});

// Heart rate bpm
export const heartRateSchema = z.object({
  type: z.literal("heart_rate"),
  value: z.number().min(30).max(250),
  recordedAt: z.string().datetime().optional(),
});

// Blood glucose mg/dL
export const bloodGlucoseSchema = z.object({
  type: z.literal("blood_glucose"),
  value: z.number().min(20).max(600),
  recordedAt: z.string().datetime().optional(),
});

// SpO2 %
export const oxygenSaturationSchema = z.object({
  type: z.literal("oxygen_saturation"),
  value: z.number().min(70).max(100),
  recordedAt: z.string().datetime().optional(),
});

// Weight kg
export const weightSchema = z.object({
  type: z.literal("weight"),
  value: z.number().min(20).max(500),
  recordedAt: z.string().datetime().optional(),
});

export const vitalSubmissionSchema = z.discriminatedUnion("type", [
  bloodPressureSchema,
  heartRateSchema,
  bloodGlucoseSchema,
  oxygenSaturationSchema,
  weightSchema,
]);

export type VitalSubmission = z.infer<typeof vitalSubmissionSchema>;
