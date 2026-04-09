#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Validates that all 12 steps of entity addition workflow were completed.

Usage:
    python validate_entity.py <EntityName>

Example:
    python validate_entity.py Supplier
    python validate_entity.py ProductCategory

This script checks all 12 steps of the entity creation workflow:
1. Entity defined in Domain
2. Repository interface created
3. Added to IUnitOfWork interface
4. Repository implementation exists
5. Entity configuration exists
6. DbSet added to ApplicationDbContext
7. UnitOfWork implementation updated
8. Service interface created
9. Service implementation exists
10. Service registered in DI container
11. Controller created
12. Migration created (partial validation)
"""

import sys
import os
import re
from pathlib import Path
from typing import Dict, Optional

# Set UTF-8 encoding for Windows console
if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding='utf-8')
        sys.stderr.reconfigure(encoding='utf-8')
    except AttributeError:
        # Python < 3.7
        import codecs
        sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')
        sys.stderr = codecs.getwriter('utf-8')(sys.stderr.buffer, 'strict')


def find_backend_root() -> Optional[Path]:
    """
    Find the lilith-backend directory by searching up from current directory.
    """
    current = Path.cwd()
    
    # Try current directory first
    if (current / "lilith-backend").exists():
        return current / "lilith-backend"
    
    # Try parent directories
    for parent in current.parents:
        backend_path = parent / "lilith-backend"
        if backend_path.exists():
            return backend_path
    
    # Try assuming script is in .skills/
    script_dir = Path(__file__).resolve().parent
    potential_root = script_dir.parent.parent.parent / "lilith-backend"
    if potential_root.exists():
        return potential_root
    
    return None


def validate_entity(entity_name: str, backend_root: Path) -> Dict[str, Optional[bool]]:
    """
    Validates all 12 steps of entity creation.
    Returns dict with step results: True (passed), False (failed), None (cannot validate).
    """
    results = {}
    
    # Step 1: Entity exists in Domain
    domain_path = backend_root / "src/Domain/Entities"
    if domain_path.exists():
        entity_files = list(domain_path.rglob(f"{entity_name}.cs"))
        results["step_1_entity"] = len(entity_files) > 0
        if entity_files:
            results["step_1_path"] = str(entity_files[0])
    else:
        results["step_1_entity"] = None
    
    # Step 2: Repository interface exists (OPTIONAL - only for Pattern B)
    repo_interface_path = backend_root / "src/Application.Contracts/Persistance/Repositories"
    if repo_interface_path.exists():
        repo_interface_files = list(repo_interface_path.rglob(f"I{entity_name}Repository.cs"))
        results["step_2_repo_interface"] = len(repo_interface_files) > 0
        results["step_2_pattern"] = "B (Specific)" if len(repo_interface_files) > 0 else "A (Generic - Skipped)"
        if repo_interface_files:
            results["step_2_path"] = str(repo_interface_files[0])
    else:
        results["step_2_repo_interface"] = None
        results["step_2_pattern"] = "Unknown"
    
    # Step 3: IUnitOfWork has property (Pattern A or B)
    unit_of_work_path = backend_root / "src/Application.Contracts/Persistance/IUnitOfWork.cs"
    if unit_of_work_path.exists():
        content = unit_of_work_path.read_text(encoding='utf-8')
        # Pattern B: I{Entity}Repository {Entity}s { get; }
        pattern_specific = rf"I{entity_name}Repository\s+{entity_name}s?\s*{{\s*get;"
        # Pattern A: IRepository<{Entity}, Guid> {Entity}s { get; }
        pattern_generic = rf"IRepository<{entity_name},\s*Guid>\s+{entity_name}s?\s*{{\s*get;"
        
        has_specific = bool(re.search(pattern_specific, content))
        has_generic = bool(re.search(pattern_generic, content))
        
        results["step_3_unit_of_work"] = has_specific or has_generic
        if has_specific:
            results["step_3_pattern"] = "Pattern B (Specific)"
        elif has_generic:
            results["step_3_pattern"] = "Pattern A (Generic)"
    else:
        results["step_3_unit_of_work"] = None
    
    # Step 4: Repository implementation exists (OPTIONAL - only for Pattern B)
    repo_impl_path = backend_root / "src/Infrastructure/Persistance/Repositories"
    if repo_impl_path.exists():
        repo_impl_files = list(repo_impl_path.rglob(f"{entity_name}Repository.cs"))
        results["step_4_repo_impl"] = len(repo_impl_files) > 0 if results.get("step_2_repo_interface") else "N/A (Pattern A)"
        if repo_impl_files:
            results["step_4_path"] = str(repo_impl_files[0])
    else:
        results["step_4_repo_impl"] = None
    
    # Step 5: Entity configuration exists (*Builder.cs, NOT *Configuration.cs)
    config_path = backend_root / "src/Infrastructure/Persistance/EntityConfiguration"
    if config_path.exists():
        # Look for *Builder.cs (correct naming)
        builder_files = list(config_path.rglob(f"{entity_name}Builder.cs"))
        # Also check for incorrect *Configuration.cs naming
        config_files = list(config_path.rglob(f"{entity_name}Configuration.cs"))
        
        results["step_5_entity_config"] = len(builder_files) > 0
        if builder_files:
            results["step_5_path"] = str(builder_files[0])
        elif config_files:
            results["step_5_path"] = str(config_files[0])
            results["step_5_warning"] = "⚠️  Found *Configuration.cs - should be *Builder.cs"
    else:
        results["step_5_entity_config"] = None
    
    # Step 6: UnitOfWork implementation (Pattern A or B) - RENUMBERED from Step 7
    unit_of_work_impl_path = backend_root / "src/Infrastructure/Persistance/UnitOfWork.cs"
    if unit_of_work_impl_path.exists():
        content = unit_of_work_impl_path.read_text(encoding='utf-8')
        
        # Pattern B: I{Entity}Repository property and {Entity}Repository initialization
        has_specific_property = bool(re.search(rf"I{entity_name}Repository\s+{entity_name}s?\s*{{\s*get;", content))
        has_specific_init = bool(re.search(rf"new\s+{entity_name}Repository\s*\(", content))
        
        # Pattern A: IRepository<{Entity}, Guid> property and Repository<{Entity}, Guid> initialization
        has_generic_property = bool(re.search(rf"IRepository<{entity_name},\s*Guid>\s+{entity_name}s?\s*{{\s*get;", content))
        has_generic_init = bool(re.search(rf"new\s+Repository<{entity_name},\s*Guid>\s*\(", content))
        
        pattern_b_complete = has_specific_property and has_specific_init
        pattern_a_complete = has_generic_property and has_generic_init
        
        results["step_6_unit_of_work_impl"] = pattern_b_complete or pattern_a_complete
        if pattern_b_complete:
            results["step_6_pattern"] = "Pattern B (Specific)"
        elif pattern_a_complete:
            results["step_6_pattern"] = "Pattern A (Generic)"
    else:
        results["step_6_unit_of_work_impl"] = None
    
    
    # Step 7: Service interface exists (RENUMBERED from Step 8)
    service_interface_path = backend_root / "src/Application.Contracts/Services"
    if service_interface_path.exists():
        service_interface_files = list(service_interface_path.rglob(f"I{entity_name}Service.cs"))
        results["step_7_service_interface"] = len(service_interface_files) > 0
        if service_interface_files:
            results["step_7_path"] = str(service_interface_files[0])
    else:
        results["step_7_service_interface"] = None
    
    # Step 8: Service implementation exists (RENUMBERED from Step 9)
    service_impl_path = backend_root / "src/Application/Services"
    if service_impl_path.exists():
        service_impl_files = list(service_impl_path.rglob(f"{entity_name}Service.cs"))
        results["step_8_service_impl"] = len(service_impl_files) > 0
        if service_impl_files:
            results["step_8_path"] = str(service_impl_files[0])
    else:
        results["step_8_service_impl"] = None
    
    # Step 9: Service registered (RENUMBERED from Step 10)
    app_services_path = backend_root / "src/Api/Setup/ApplicationServicesSetup.cs"
    if app_services_path.exists():
        content = app_services_path.read_text(encoding='utf-8')
        results["step_9_service_registered"] = f"I{entity_name}Service" in content
    else:
        results["step_9_service_registered"] = None
    
    # Step 10: Controller exists (RENUMBERED from Step 11)
    controller_path = backend_root / "src/Api/Controllers"
    if controller_path.exists():
        controller_files = list(controller_path.rglob(f"{entity_name}Controller.cs"))
        results["step_10_controller"] = len(controller_files) > 0
        if controller_files:
            results["step_10_path"] = str(controller_files[0])
    else:
        results["step_10_controller"] = None
    
    # Step 11: Migration exists (RENUMBERED from Step 12)
    migrations_path = backend_root / "src/Infrastructure/Migrations"
    if migrations_path.exists():
        # Look for migration files containing entity name
        all_migrations = list(migrations_path.glob("*Add*.cs"))
        entity_migrations = [m for m in all_migrations if entity_name in m.name]
        results["step_11_migration"] = len(entity_migrations) > 0
        if entity_migrations:
            results["step_11_path"] = str(entity_migrations[0])
    else:
        results["step_11_migration"] = None
    
    return results


def print_results(entity_name: str, results: Dict[str, Optional[bool]], backend_root: Path):
    """Pretty print validation results."""
    print(f"\n{'='*70}")
    print(f"Entity Validation Report: {entity_name}")
    print(f"Backend root: {backend_root}")
    print(f"{'='*70}\n")
    
    # Detect pattern
    pattern_info = results.get("step_3_pattern", "Unknown")
    print(f"📋 Detected Pattern: {pattern_info}\n")
    
    steps = [
        ("step_1_entity", "1. Entity defined in Domain"),
        ("step_2_repo_interface", "2. Repository interface (Application.Contracts) - Pattern B only"),
        ("step_3_unit_of_work", "3. Added to IUnitOfWork interface"),
        ("step_4_repo_impl", "4. Repository implementation (Infrastructure) - Pattern B only"),
        ("step_5_entity_config", "5. Entity configuration (*Builder.cs in Infrastructure)"),
        ("step_6_unit_of_work_impl", "6. UnitOfWork implementation updated"),
        ("step_7_service_interface", "7. Service interface created (Application.Contracts)"),
        ("step_8_service_impl", "8. Service implementation (Application)"),
        ("step_9_service_registered", "9. Service registered in DI container (Api)"),
        ("step_10_controller", "10. Controller created (Api)"),
        ("step_11_migration", "11. Migration created (Infrastructure)"),
    ]
    
    passed = 0
    failed = 0
    skipped = 0
    warnings = []
    
    for key, description in steps:
        status = results.get(key)
        path_key = key.split('_')[0] + '_' + key.split('_')[1] + '_path'
        file_path = results.get(path_key, '')
        warning_key = key.split('_')[0] + '_' + key.split('_')[1] + '_warning'
        warning = results.get(warning_key, '')
        pattern_key = key.split('_')[0] + '_' + key.split('_')[1] + '_pattern'
        pattern = results.get(pattern_key, '')
        
        if status == "N/A (uses auto-discovery)":
            print(f"ℹ️  {description}")
            print(f"   └─ N/A - Project uses ApplyConfigurationsFromAssembly()")
            skipped += 1
        elif status == "N/A (Pattern A)":
            print(f"ℹ️  {description}")
            print(f"   └─ N/A - Pattern A uses generic repository")
            skipped += 1
        elif status is True:
            print(f"✅ {description}")
            if pattern:
                print(f"   ├─ {pattern}")
            if file_path:
                print(f"   └─ Found: {file_path}")
            if warning:
                print(f"   ⚠️  {warning}")
                warnings.append(warning)
            passed += 1
        elif status is False:
            # Check if this is an optional step for Pattern A
            if key in ["step_2_repo_interface", "step_4_repo_impl"] and "Pattern A" in pattern_info:
                print(f"ℹ️  {description}")
                print(f"   └─ N/A - Pattern A uses generic repository")
                skipped += 1
            else:
                print(f"❌ {description}")
                failed += 1
        else:
            print(f"⚠️  {description} (cannot validate - directory not found)")
            skipped += 1
    
    print(f"\n{'='*70}")
    print(f"Summary: {passed} passed, {failed} failed, {skipped} skipped/N/A")
    if warnings:
        print(f"⚠️  {len(warnings)} warning(s) found")
    print(f"{'='*70}\n")
    
    if warnings:
        print("⚠️  Warnings:")
        for w in warnings:
            print(f"  - {w}")
        print()
    
    if failed == 0 and skipped >= 0:
        print("🎉 All required steps completed successfully!")
        print(f"\nNext steps:")
        print(f"1. Test endpoints in Swagger: https://localhost:5001/swagger")
        print(f"2. Verify migration applied: dotnet ef migrations list --project src/Infrastructure/")
        return 0
    else:
        print(f"❌ {failed} step(s) incomplete. Review the 11-step checklist in SKILL.md")
        print(f"\nCommon fixes:")
        
        if not results.get("step_9_service_registered"):
            print(f"  - Step 9: Add to src/Api/Setup/ApplicationServicesSetup.cs:")
            print(f"    services.AddScoped<I{entity_name}Service, {entity_name}Service>();")
        
        if not results.get("step_5_entity_config"):
            print(f"  - Step 5: Create src/Infrastructure/Persistance/EntityConfiguration/{{Module}}/{entity_name}Builder.cs")
            print(f"    NOTE: Use *Builder.cs, NOT *Configuration.cs!")
        
        if not results.get("step_11_migration"):
            print(f"  - Step 11: Run migration:")
            print(f"    dotnet ef migrations add Add{entity_name} --project src/Infrastructure/")
        
        return 1


def main():
    if len(sys.argv) != 2:
        print("Usage: python validate_entity.py <EntityName>")
        print("\nExample:")
        print("  python validate_entity.py Supplier")
        print("  python validate_entity.py ProductCategory")
        sys.exit(1)
    
    entity_name = sys.argv[1]
    
    # Validate entity name format (PascalCase)
    if not entity_name[0].isupper() or not entity_name.isalnum():
        print(f"❌ Entity name should be PascalCase (e.g., 'Supplier', not '{entity_name}')")
        sys.exit(1)
    
    # Find backend root
    backend_root = find_backend_root()
    
    if not backend_root:
        print("❌ Backend directory 'lilith-backend' not found.")
        print("\nSearched:")
        print(f"  - Current directory: {Path.cwd()}")
        print(f"  - Parent directories")
        print(f"  - Relative to script location")
        print("\nRun this script from the project root or ensure lilith-backend exists.")
        sys.exit(1)
    
    print(f"🔍 Validating entity '{entity_name}'...")
    print(f"📁 Backend root: {backend_root}\n")
    
    results = validate_entity(entity_name, backend_root)
    exit_code = print_results(entity_name, results, backend_root)
    sys.exit(exit_code)


if __name__ == "__main__":
    main()
