"use client";

import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { vitalsApi, caregiverLinksApi } from "@/lib/api";

const TYPE_LABELS: Record<string, string> = {
  blood_pressure: "Blood pressure",
  heart_rate: "Heart rate",
  blood_glucose: "Blood glucose",
  oxygen_saturation: "Oxygen saturation",
  weight: "Weight",
};

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "short",
    timeStyle: "short",
  });
}

function formatValue(r: { type: string; value: number; valueSecondary?: number | null; unit: string }) {
  if (r.type === "blood_pressure" && r.valueSecondary != null)
    return `${r.value}/${r.valueSecondary} ${r.unit}`;
  return `${r.value} ${r.unit}`;
}

export default function CaregiverVitalsPage() {
  const searchParams = useSearchParams();
  const patientId = searchParams.get("patientId");

  const { data: links = [] } = useQuery({
    queryKey: ["caregiverlinks"],
    queryFn: () => caregiverLinksApi.list({ activeOnly: true }),
  });

  const link = patientId ? links.find((l) => l.patientId === patientId) : null;

  const { data: readings = [], isLoading, error } = useQuery({
    queryKey: ["vitals", "caregiver", patientId],
    queryFn: () => vitalsApi.list({ patientId: patientId!, limit: 100 }),
    enabled: !!patientId,
  });

  return (
    <>
      <PageHeader
        title="Vitals (view only)"
        description="Read-only. No modifications or alert actions."
      />
      <Card>
        <CardHeader
          title="Patient vitals"
          description="Consent-based. Caregiver actions logged separately."
          action={
            <Link href="/caregiver" className="btn-secondary text-sm">
              Back to dashboard
            </Link>
          }
        />
        {!patientId ? (
          <p className="text-sm text-slate-600">
            Select a patient from the{" "}
            <Link href="/caregiver" className="text-primary-600 hover:underline">
              dashboard
            </Link>
            .
          </p>
        ) : (
          <>
            {link && (
              <p className="mb-4 text-sm text-slate-700">
                Viewing vitals for patient (consented access).
              </p>
            )}
            {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
            {error && (
              <p className="text-sm text-red-600">
                {error instanceof Error ? error.message : "Failed to load vitals."}
              </p>
            )}
            {!isLoading && !error && (
              <>
                {readings.length === 0 ? (
                  <p className="text-sm text-slate-600">
                    No vitals recorded for this patient yet.
                  </p>
                ) : (
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b border-slate-200 text-left text-slate-600">
                          <th className="pb-3 font-medium">Type</th>
                          <th className="pb-3 font-medium">Value</th>
                          <th className="pb-3 font-medium">Recorded</th>
                          <th className="pb-3 font-medium">Source</th>
                        </tr>
                      </thead>
                      <tbody>
                        {readings.map((r) => (
                          <tr
                            key={r.id}
                            className="border-b border-slate-100 transition-colors hover:bg-slate-50/80"
                          >
                            <td className="py-3 text-slate-900">
                              {TYPE_LABELS[r.type] ?? r.type}
                            </td>
                            <td className="py-3 font-medium">{formatValue(r)}</td>
                            <td className="py-3 text-slate-600">
                              {formatDate(r.recordedAt)}
                            </td>
                            <td className="py-3 capitalize text-slate-600">
                              {r.source}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </>
            )}
          </>
        )}
      </Card>
    </>
  );
}
