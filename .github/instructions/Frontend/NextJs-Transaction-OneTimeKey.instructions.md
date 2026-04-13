# Next.js Frontend Integration: One-Time Transaction Key View

## Purpose

Use this guide when updating the Next.js frontend to support the backend one-time key reveal behavior for transactions.

## Backend Contract

- Endpoint 1: `GET /api/transactions/{transactionId}`
- Endpoint 2: `GET /api/payments/complete/{transactionId}`
- Success payload: `TransactionViewResponse`
- Key business rule:
  - `LicenseKey` is returned only once for successful payment records.
  - `IsKeyViewed` becomes `true` after the first successful reveal.
  - Subsequent requests return `LicenseKey = null`.

## UI/UX Requirements

- On first successful page load:
  - Render `LicenseKey` prominently.
  - Show one-time warning: "This key is shown only once. Save it now."
  - Provide actions: copy key, download text file, acknowledge saved.
- If `IsKeyViewed` is `true` and `LicenseKey` is null:
  - Do not show empty key placeholders.
  - Show fallback help text for recovery path (support contact or account settings guidance).
- If transaction is not found or not successful (404):
  - Show a clear status page with retry + support options.

## Suggested Client Types

```ts
export type TransactionViewResponse = {
  transactionId: string;
  customerId: string;
  planId: string;
  keyId?: string | null;
  guildId: number;
  amount: string;
  paymentStatus: "Pending" | "Success" | "Failed" | string;
  billCode?: string | null;
  isKeyViewed: boolean;
  licenseKey?: string | null;
};
```

## Implementation Notes

- Fetch once on page load using server action or route handler as preferred by project conventions.
- Avoid automatic background refetching on this page; it can consume the one-time key reveal unexpectedly.
- Keep response handling idempotent:
  - If `licenseKey` exists, persist client-side copy flow immediately.
  - If `licenseKey` is null, present non-destructive fallback state.

## Acceptance Checks

- First view of successful transaction shows key and warning.
- Refresh after first view no longer shows key.
- Failed transaction never shows key.
- Not-found response renders resilient recovery UI.