"use client";

import { useState } from "react";
import {
  vitalSubmissionSchema,
  type VitalSubmission,
} from "@/lib/vitals-schema";

const VITAL_OPTIONS: { type: VitalSubmission["type"]; label: string }[] = [
  { type: "blood_pressure", label: "Blood pressure (mmHg)" },
  { type: "heart_rate", label: "Heart rate (bpm)" },
  { type: "blood_glucose", label: "Blood glucose (mg/dL)" },
  { type: "oxygen_saturation", label: "Oxygen saturation (%)" },
  { type: "weight", label: "Weight (kg)" },
];

export function VitalForm({ onSuccess }: { onSuccess: () => void }) {
  const [type, setType] = useState<VitalSubmission["type"]>("blood_pressure");
  const [systolic, setSystolic] = useState("");
  const [diastolic, setDiastolic] = useState("");
  const [value, setValue] = useState("");
  const [error, setError] = useState<string | null>(null);

  const buildPayload = (): VitalSubmission | null => {
    const recordedAt = new Date().toISOString();
    switch (type) {
      case "blood_pressure": {
        const s = Number(systolic);
        const d = Number(diastolic);
        return {
          type: "blood_pressure" as const,
          systolic: s,
          diastolic: d,
          recordedAt,
        };
      }
      case "heart_rate":
        return { type: "heart_rate", value: Number(value), recordedAt };
      case "blood_glucose":
        return { type: "blood_glucose", value: Number(value), recordedAt };
      case "oxygen_saturation":
        return { type: "oxygen_saturation", value: Number(value), recordedAt };
      case "weight":
        return { type: "weight", value: Number(value), recordedAt };
      default:
        return null;
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    const payload = buildPayload();
    if (!payload) {
      setError("Please fill in valid values.");
      return;
    }
    const parsed = vitalSubmissionSchema.safeParse(payload);
    if (!parsed.success) {
      setError(parsed.error.errors.map((e) => e.message).join(" "));
      return;
    }
    // Demo: no API yet
    onSuccess();
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <div>
        <label className="block text-sm font-medium text-slate-700">
          Vital type
        </label>
        <select
          value={type}
          onChange={(e) => setType(e.target.value as VitalSubmission["type"])}
          className="input-field mt-1"
        >
          {VITAL_OPTIONS.map((opt) => (
            <option key={opt.type} value={opt.type}>
              {opt.label}
            </option>
          ))}
        </select>
      </div>

      {type === "blood_pressure" && (
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label className="block text-sm font-medium text-slate-700">
              Systolic (mmHg)
            </label>
            <input
              type="number"
              min={70}
              max={250}
              value={systolic}
              onChange={(e) => setSystolic(e.target.value)}
              className="input-field mt-1"
              placeholder="120"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700">
              Diastolic (mmHg)
            </label>
            <input
              type="number"
              min={40}
              max={150}
              value={diastolic}
              onChange={(e) => setDiastolic(e.target.value)}
              className="input-field mt-1"
              placeholder="80"
            />
          </div>
        </div>
      )}

      {type !== "blood_pressure" && (
        <div>
          <label className="block text-sm font-medium text-slate-700">
            Value
            {type === "heart_rate" && " (bpm)"}
            {type === "blood_glucose" && " (mg/dL)"}
            {type === "oxygen_saturation" && " (%)"}
            {type === "weight" && " (kg)"}
          </label>
          <input
            type="number"
            step={type === "weight" ? 0.1 : 1}
            min={
              type === "oxygen_saturation"
                ? 70
                : type === "heart_rate"
                  ? 30
                  : type === "blood_glucose"
                    ? 20
                    : 20
            }
            max={
              type === "oxygen_saturation"
                ? 100
                : type === "heart_rate"
                  ? 250
                  : type === "blood_glucose"
                    ? 600
                    : 500
            }
            value={value}
            onChange={(e) => setValue(e.target.value)}
            className="input-field mt-1"
            placeholder={
              type === "heart_rate"
                ? "72"
                : type === "blood_glucose"
                  ? "100"
                  : type === "oxygen_saturation"
                    ? "98"
                    : "70"
            }
          />
        </div>
      )}

      {error && (
        <p className="text-sm text-red-600" role="alert">
          {error}
        </p>
      )}

      <button type="submit" className="btn-primary">
        Submit reading
      </button>
    </form>
  );
}
