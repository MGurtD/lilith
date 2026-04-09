# Migration Commands Reference

## Create Migration

```bash
dotnet ef migrations add MigrationName --project src/Infrastructure/
```

**Naming conventions:**
- Use descriptive names: `AddWorkOrderPhases`, `AddCustomerTypeColumn`
- PascalCase, no spaces
- Be specific about what the migration does

## Apply Migration

```bash
# Apply all pending migrations
dotnet ef database update --project src/Infrastructure/

# Apply to specific migration
dotnet ef database update SpecificMigrationName --project src/Infrastructure/
```

## Rollback Migration

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName --project src/Infrastructure/

# Rollback all migrations
dotnet ef database update 0 --project src/Infrastructure/
```

## View Migration SQL

```bash
# Generate SQL without applying
dotnet ef migrations script --project src/Infrastructure/

# Generate SQL for specific range
dotnet ef migrations script FromMigration ToMigration --project src/Infrastructure/
```

## Remove Migration

```bash
# Remove last migration (only if not applied to database)
dotnet ef migrations remove --project src/Infrastructure/
```

## List Migrations

```bash
# Show all migrations and their status
dotnet ef migrations list --project src/Infrastructure/
```

## Common Issues

**Error: "No migrations configuration type was found"**
- Ensure you're specifying `--project src/Infrastructure/`

**Error: "A connection was not established"**
- Check PostgreSQL is running
- Verify connection string in `appsettings.Development.json`

**Error: "The migration has already been applied"**
- Cannot remove applied migrations
- Create a new migration to revert changes

## Best Practices

1. **Review generated code** before applying
2. **Test locally** before committing
3. **Include rollback logic** in `Down()` method
4. **Backup database** before applying to production
5. **Use descriptive names** for easy identification
