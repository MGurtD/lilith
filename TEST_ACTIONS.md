# Test GitHub Actions

This file was created to test that GitHub Actions workflows are triggered correctly.

**Date**: 2026-02-15
**Branch**: test
**Purpose**: Verify CI/CD pipeline execution

## Expected behavior

When this commit is pushed to the `test` branch:

1. ✅ Backend CI/CD workflow should trigger (if backend files changed)
2. ✅ Frontend CI/CD workflow should trigger (if frontend files changed)
3. ✅ Docker images should be built with tag `test`

## Workflow files

- `.github/workflows/backend-ci.yml` - Backend pipeline
- `.github/workflows/frontend-ci.yml` - Frontend pipeline

Both workflows are configured to trigger on pushes to the `test` branch.
