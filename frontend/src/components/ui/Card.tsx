import React from "react";

interface CardProps {
  children: React.ReactNode;
  className?: string;
  hoverLift?: boolean;
}

export function Card({
  children,
  className = "",
  hoverLift,
}: CardProps & { hoverLift?: boolean }) {
  return (
    <div
      className={`card-clinical p-6 ${hoverLift ? "card-hover-lift" : ""} ${className}`}
    >
      {children}
    </div>
  );
}

export function CardHeader({
  title,
  description,
  action,
}: {
  title: string;
  description?: string;
  action?: React.ReactNode;
}) {
  return (
    <div className="mb-4 flex flex-col gap-1 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <h2 className="text-lg font-medium text-slate-900">{title}</h2>
        {description && (
          <p className="text-sm text-slate-600">{description}</p>
        )}
      </div>
      {action && <div>{action}</div>}
    </div>
  );
}
