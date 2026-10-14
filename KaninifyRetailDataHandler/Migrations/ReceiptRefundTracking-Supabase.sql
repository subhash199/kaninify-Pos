-- Apply to the cloud SalesTransactions table before syncing receipt refunds.
ALTER TABLE public."SalesTransactions"
    ADD COLUMN IF NOT EXISTS "Receipt_Refund_Reference" varchar(40),
    ADD COLUMN IF NOT EXISTS "Last_Refunded_At" timestamptz;
