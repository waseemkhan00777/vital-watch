"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { caregiverLinksApi } from "@/lib/api";

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "short",
    timeStyle: "short",
  });
}

export default function CaregiversPage() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const queryClient = useQueryClient();

  const { data: links = [], isLoading, error } = useQuery({
    queryKey: ["caregiverlinks"],
    queryFn: () => caregiverLinksApi.list({ activeOnly: true }),
  });

  const inviteMutation = useMutation({
    mutationFn: () => caregiverLinksApi.invite(email),
    onSuccess: () => {
      setEmail("");
      setMessage({ type: "success", text: "Caregiver invited. They will have read-only access to your vitals." });
      queryClient.invalidateQueries({ queryKey: ["caregiverlinks"] });
    },
    onError: (err: unknown) => {
      const msg = err && typeof err === "object" && "message" in err ? String((err as { message: string }).message) : "Failed to invite.";
      setMessage({ type: "error", text: msg });
    },
  });

  const revokeMutation = useMutation({
    mutationFn: (id: string) => caregiverLinksApi.revoke(id),
    onSuccess: () => {
      setMessage({ type: "success", text: "Access revoked." });
      queryClient.invalidateQueries({ queryKey: ["caregiverlinks"] });
    },
    onError: () => {
      setMessage({ type: "error", text: "Failed to revoke." });
    },
  });

  const activeLinks = links.filter((l) => !l.revokedAt);

  return (
    <>
      <PageHeader
        title="Caregiver access"
        description="Grant or revoke consent for caregivers to view your vitals. All actions are logged."
      />
      <Card>
        <CardHeader
          title="Consent-based access"
          description="Caregivers can only view your vitals when you grant access. Revocation takes effect immediately."
        />
        {message && (
          <p
            className={`mb-4 text-sm ${message.type === "success" ? "text-emerald-700" : "text-red-600"}`}
          >
            {message.text}
          </p>
        )}
        <div className="mb-4 flex flex-wrap gap-2">
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="Caregiver email"
            className="input-field max-w-xs flex-1"
          />
          <button
            type="button"
            onClick={() => inviteMutation.mutate()}
            disabled={!email.trim() || inviteMutation.isPending}
            className="btn-primary"
          >
            {inviteMutation.isPending ? "Adding…" : "Add caregiver"}
          </button>
        </div>
        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load links."}
          </p>
        )}
        {!isLoading && !error && (
          <>
            {activeLinks.length === 0 ? (
              <p className="text-sm text-slate-600">
                No caregivers linked. Add a caregiver by email above.
              </p>
            ) : (
              <ul className="space-y-2">
                {activeLinks.map((l) => (
                  <li
                    key={l.id}
                    className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-slate-200 bg-slate-50/50 px-4 py-3"
                  >
                    <span className="font-medium text-slate-900">
                      {l.caregiverName || l.caregiverId}
                    </span>
                    <span className="text-sm text-slate-600">
                      Since {formatDate(l.consentedAt)}
                    </span>
                    <button
                      type="button"
                      onClick={() => revokeMutation.mutate(l.id)}
                      disabled={revokeMutation.isPending}
                      className="btn-secondary text-sm"
                    >
                      Revoke access
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </>
        )}
      </Card>
    </>
  );
}
