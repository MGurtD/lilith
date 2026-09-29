# Expense type management

## What this screen is for

Lists the expense types, the catalog that classifies the company's general expenses recorded in "Expense management", outside purchase invoices. Every expense in "Expense management" has a type, and the "Expense dashboard" groups expenses by type in its chart by type.

## Available actions

- Create a type with the "+" button ("Create new") above the list.
- Open a type by clicking its row to change its name, description or "Disabled".
- Delete a type with the row's trash icon ("Delete"), after confirming.

## Usual flow

1. Open "Expense type management".
2. Check the "Name" and "Description" columns to see whether the type you need already exists.
3. If it does not, press "+", fill in the name and description, and save with "Save".
4. Go to "Expense management" and use the new type in the expenses' "Type" field.

## Important notes

- Deleting an expense type is permanent, but a type that has expenses assigned cannot be deleted, so no recorded expense is lost.
- The type name is what the "Expense dashboard" shows (the "Detail" filter and the chart by type) when the type is "Expense". If you rename it, existing expenses are shown under the new name.
- Marking a type "Disabled" is only an indication in the list: the type is still available in the expenses' "Type" dropdown and in the filters.
- The list has no filters: it always shows every type.

## Common errors

- If deleting shows "The expense type ... could not be deleted", the type has expenses assigned. Change their "Type" in "Expense management" if you want to delete it, or mark it "Disabled".
- If "Entity already exists" appears when creating it, a type with that name already exists.

## Basic process

```mermaid
flowchart TD
    A[Open expense types] --> B{Does the type exist?}
    B -->|No| C[Create the type]
    B -->|Yes| D[Open and review]
    C --> E[Save]
    D --> E
    E --> F[Assign it to expenses]
```
