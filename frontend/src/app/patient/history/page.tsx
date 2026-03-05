"use client";

import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { vitalsApi } from "@/lib/api";

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

export default function PatientHistoryPage() {
  const { data: readings = [], isLoading, error } = useQuery({
    queryKey: ["vitals", "patient"],
    queryFn: () => vitalsApi.list({ limit: 100 }),
  });

  return (
    <>
      <PageHeader
        title="Vitals history"
        description="Your recorded vital signs. Export available for your records."
      />
      <Card>
        <CardHeader
          title="Readings"
          description="Chronological list of submitted vitals."
        />
        {isLoading && (
          <p className="text-sm text-slate-600">Loading…</p>
        )}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load readings."}
          </p>
        )}
        {!isLoading && !error && (
          <>
            {readings.length === 0 ? (
              <p className="text-sm text-slate-600">
                No readings yet. Submit vitals from the Submit vitals page.
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
                        <td className="py-3 font-medium">
                          {formatValue(r)}
                        </td>
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
      </Card>
    </>
  );
}
