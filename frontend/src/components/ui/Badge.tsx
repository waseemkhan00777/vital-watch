import React from "react";

type Severity = "critical" | "high" | "moderate" | "normal" | "neutral";

const styles: Record<Severity, string> = {
  critical: "bg-red-100 text-red-800",
  high: "bg-amber-100 text-amber-800",
  moderate: "bg-yellow-100 text-yellow-800",
  normal: "bg-emerald-100 text-emerald-800",
  neutral: "bg-slate-100 text-slate-700",
};

interface BadgeProps {
  children: React.ReactNode;
  severity?: Severity;
  className?: string;
}

export function Badge({
  children,
  severity = "neutral",
  className = "",
}: BadgeProps) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium transition-transform duration-200 hover:scale-105 ${styles[severity]} ${className}`}
    >
      {children}
    </span>
  );
}
