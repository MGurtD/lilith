# Data scripts

One-off PostgreSQL scripts that change application data (not schema) and must be applied by hand to each environment. Schema changes and data every deployment needs belong in EF migrations.

Every script here must:

- be idempotent, so running it again changes nothing;
- report what it changed, so the output can be kept as a record;
- skip rows that an environment has customized, and list them for manual review.

Run a script with psql, DBeaver or pgAdmin against the environment's application database (the `Default` connection string), and keep the output.

| Script | Purpose | Applied to |
|---|---|---|
| `menu-translations-es-en.sql` | Spanish and English titles for the standard menu items (#144). | staging (temges), 2026-09-26: 129 translations updated |
