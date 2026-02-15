# Entity Validation Script

## validate_entity.py

Validates that all 11 steps of the entity addition workflow were completed correctly. **Automatically detects** whether the entity uses **Pattern A (Generic Repository)** or **Pattern B (Specific Repository)**.

### Usage

```bash
# From project root
python .skills/adding-backend-entity/scripts/validate_entity.py <EntityName>

# Real examples from this project
python .skills/adding-backend-entity/scripts/validate_entity.py Enterprise    # Pattern A
python .skills/adding-backend-entity/scripts/validate_entity.py Area          # Pattern B
python .skills/adding-backend-entity/scripts/validate_entity.py Operator      # Pattern A
```

### What It Checks

The script validates all 11 steps, **adapting to the detected pattern**:

**Always checked:**
1. ✅ Entity defined in `Domain/Entities/{Module}/`
3. ✅ Property added to `IUnitOfWork` (either `IRepository<T, Guid>` or `IEntityRepository`)
5. ✅ Entity configuration in `Infrastructure/Persistance/EntityConfiguration/{Module}/*Builder.cs`
6. ✅ UnitOfWork implementation updated
7. ✅ Service interface in `Application.Contracts/Services/{Module}/`
8. ✅ Service implementation in `Application/Services/{Module}/`
9. ✅ Service registered in `Api/Setup/ApplicationServicesSetup.cs`
10. ✅ Controller in `Api/Controllers/{Module}/`
11. ✅ Migration created in `Infrastructure/Migrations/`

**Pattern B only (custom repository):**
2. ✅ Repository interface in `Application.Contracts/Persistance/Repositories/{Module}/`
4. ✅ Repository implementation in `Infrastructure/Persistance/Repositories/{Module}/`

**Pattern A (generic repository) - these are marked as N/A:**
2. ℹ️  Repository interface (skipped)
4. ℹ️  Repository implementation (skipped)

**Note**: DbSet step has been removed - project uses `ApplyConfigurationsFromAssembly()` for auto-discovery.

### Pattern Detection

The script detects the pattern by checking `IUnitOfWork.cs`:

- **Pattern A**: `IRepository<Enterprise, Guid> Enterprises { get; }`
- **Pattern B**: `IAreaRepository Areas { get; }`

### Example Output - Pattern A (Generic)

```
======================================================================
Entity Validation Report: Enterprise
Backend root: C:\Users\mgurt\source\personal\lilith\lilith-backend
======================================================================

📋 Detected Pattern: Pattern A (Generic)

✅ 1. Entity defined in Domain
   └─ Found: C:\...\Domain\Entities\Production\Enterprise.cs
ℹ️  2. Repository interface (Application.Contracts) - Pattern B only
   └─ N/A - Pattern A uses generic repository
✅ 3. Added to IUnitOfWork interface
   ├─ Pattern A (Generic)
ℹ️  4. Repository implementation (Infrastructure) - Pattern B only
   └─ N/A - Pattern A uses generic repository
✅ 5. Entity configuration (*Builder.cs in Infrastructure)
   └─ Found: C:\...\EntityConfiguration\Production\EnterpriseBuilder.cs
✅ 6. UnitOfWork implementation updated
   ├─ Pattern A (Generic)
✅ 7. Service interface created (Application.Contracts)
   └─ Found: C:\...\Services\Production\IEnterpriseService.cs
✅ 8. Service implementation (Application)
   └─ Found: C:\...\Services\Production\EnterpriseService.cs
✅ 9. Service registered in DI container (Api)
✅ 10. Controller created (Api)
   └─ Found: C:\...\Controllers\Production\EnterpriseController.cs
❌ 11. Migration created (Infrastructure)

======================================================================
Summary: 7 passed, 1 failed, 2 skipped/N/A
======================================================================

🎉 All required steps completed successfully!
```

### Example Output - Pattern B (Specific)

```
======================================================================
Entity Validation Report: Area
Backend root: C:\Users\mgurt\source\personal\lilith\lilith-backend
======================================================================

📋 Detected Pattern: Pattern B (Specific)

✅ 1. Entity defined in Domain
   └─ Found: C:\...\Domain\Entities\Production\Area.cs
✅ 2. Repository interface (Application.Contracts) - Pattern B only
   ├─ B (Specific)
   └─ Found: C:\...\Repositories\Production\IAreaRepository.cs
✅ 3. Added to IUnitOfWork interface
   ├─ Pattern B (Specific)
✅ 4. Repository implementation (Infrastructure) - Pattern B only
   └─ Found: C:\...\Repositories\Production\AreaRepository.cs
✅ 5. Entity configuration (*Builder.cs in Infrastructure)
   └─ Found: C:\...\EntityConfiguration\Production\AreaBuilder.cs
✅ 6. UnitOfWork implementation updated
   ├─ Pattern B (Specific)
✅ 7. Service interface created (Application.Contracts)
   └─ Found: C:\...\Services\Production\IAreaService.cs
✅ 8. Service implementation (Application)
   └─ Found: C:\...\Services\Production\AreaService.cs
✅ 9. Service registered in DI container (Api)
✅ 10. Controller created (Api)
   └─ Found: C:\...\Controllers\Production\AreaController.cs
❌ 11. Migration created (Infrastructure)

======================================================================
Summary: 10 passed, 1 failed, 0 skipped/N/A
======================================================================
```

### Warnings

The script will show warnings for:

- ⚠️  Found `*Configuration.cs` instead of `*Builder.cs` (Step 5)
- ⚠️  Found DbSet but not needed (Step 6 - project uses auto-discovery)

### Exit Codes

- `0` - All required steps passed (skipped/N/A steps don't count as failures)
- `1` - One or more required steps failed

### Requirements

- Python 3.6+
- Must be run from project root or subdirectory containing `lilith-backend/`

### Real Project Patterns

**80% of entities use Pattern A** (simple CRUD):
- Enterprise, Site, Operator, OperatorType
- ~30 minutes implementation time
- 9 steps (skip Steps 2, 4)

**20% of entities use Pattern B** (custom queries):
- Area, Workcenter, Budget (entities with custom filtering/queries)
- ~45 minutes implementation time
- 11 steps (all steps)
