"use client";

import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { vitalsApi, usersApi } from "@/lib/api";

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

export default function ClinicianVitalsPage() {
  const searchParams = useSearchParams();
  const patientId = searchParams.get("patientId");

  const { data: patient } = useQuery({
    queryKey: ["users", patientId],
    queryFn: () => usersApi.get(patientId!),
    enabled: !!patientId,
  });

  const { data: readings = [], isLoading, error } = useQuery({
    queryKey: ["vitals", "patient", patientId],
    queryFn: () => vitalsApi.list({ patientId: patientId!, limit: 100 }),
    enabled: !!patientId,
  });

  if (!patientId) {
    return (
      <>
        <PageHeader title="Patient vitals" description="Select a patient to view vitals." />
        <Card>
          <p className="text-sm text-slate-600">
            <Link href="/clinician/patients" className="text-primary-600 hover:underline">
              Go to Patients
            </Link>{" "}
            and click &quot;View vitals&quot; for a patient.
          </p>
        </Card>
      </>
    );
  }

  return (
    <>
      <PageHeader
        title={patient ? `Vitals: ${patient.name}` : "Patient vitals"}
        description="Read-only. Role-based access enforced at API."
      />
      <Card>
        <CardHeader
          title="Readings"
          description={patient ? `Chronological vitals for ${patient.name}` : "Loading…"}
          action={
            <Link href="/clinician/patients" className="btn-secondary text-sm">
              Back to patients
            </Link>
          }
        />
        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load vitals."}
          </p>
        )}
        {!isLoading && !error && (
          <>
            {readings.length === 0 ? (
              <p className="text-sm text-slate-600">No readings for this patient.</p>
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
                        <td className="py-3 text-slate-900">{TYPE_LABELS[r.type] ?? r.type}</td>
                        <td className="py-3 font-medium">{formatValue(r)}</td>
                        <td className="py-3 text-slate-600">{formatDate(r.recordedAt)}</td>
                        <td className="py-3 capitalize text-slate-600">{r.source}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </>
        )}
      </Card>
    </>
  );
}
