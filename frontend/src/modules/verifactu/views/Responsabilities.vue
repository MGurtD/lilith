<template>
  <div class="responsabilities-container">
    <Card class="legal-declaration-card">
      <template #title>
        <div class="declaration-header">
          <h1 class="declaration-title">
            {{ $t("verifactu.responsabilities.title") }}
          </h1>
        </div>
      </template>

      <template #content>
        <div class="declaration-content">
          <!-- Location and Date -->
          <section class="declaration-section">
            <p class="declaration-meta">
              {{ $t("verifactu.responsabilities.locationDate") }}
            </p>
            <p class="declaration-meta">
              {{ $t("verifactu.responsabilities.declarant") }}
            </p>
          </section>

          <Divider />

          <!-- Main Declaration -->
          <section class="declaration-section">
            <h2 class="section-title">
              {{ $t("verifactu.responsabilities.declares") }}
            </h2>
            <p class="declaration-text">
              {{ $t("verifactu.responsabilities.softwareCompliance") }}
            </p>
          </section>

          <Divider />

          <!-- Specific Compliance Points -->
          <section class="declaration-section">
            <h3 class="subsection-title">
              {{ $t("verifactu.responsabilities.specificsIntro") }}
            </h3>
            <ul class="compliance-list">
              <li class="compliance-item">
                <i class="pi pi-check-circle text-green-600"></i>
                <span>{{
                  $t("verifactu.responsabilities.specifics.integrity")
                }}</span>
              </li>
              <li class="compliance-item">
                <i class="pi pi-check-circle text-green-600"></i>
                <span>{{
                  $t("verifactu.responsabilities.specifics.hash")
                }}</span>
              </li>
              <li class="compliance-item">
                <i class="pi pi-check-circle text-green-600"></i>
                <span>{{
                  $t("verifactu.responsabilities.specifics.noModification")
                }}</span>
              </li>
              <li class="compliance-item">
                <i class="pi pi-check-circle text-green-600"></i>
                <span>{{
                  $t("verifactu.responsabilities.specifics.transmission")
                }}</span>
              </li>
              <li class="compliance-item">
                <i class="pi pi-check-circle text-green-600"></i>
                <span>{{
                  $t("verifactu.responsabilities.specifics.retention")
                }}</span>
              </li>
              <li class="compliance-item">
                <i class="pi pi-check-circle text-green-600"></i>
                <span>{{
                  $t("verifactu.responsabilities.specifics.uniqueId")
                }}</span>
              </li>
            </ul>
          </section>

          <Divider />

          <!-- Liability Notice -->
          <section class="declaration-section liability-section">
            <Message severity="warn" :closable="false">
              <div class="flex align-items-start">
                <i class="pi pi-exclamation-triangle text-xl mr-3 mt-1"></i>
                <p class="liability-text m-0">
                  {{ $t("verifactu.responsabilities.liability") }}
                </p>
              </div>
            </Message>
          </section>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import Card from "primevue/card";
import Divider from "primevue/divider";
import Message from "primevue/message";
</script>

<style scoped>
.responsabilities-container {
  max-width: 900px;
  margin: 1rem auto;
  padding: 1rem;
}

.legal-declaration-card {
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  border: 1px solid var(--surface-border);
}

.declaration-header {
  text-align: center;
  padding: 0.75rem 0 0.5rem;
  border-bottom: 2px solid var(--primary-color);
}

.declaration-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--text-color);
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.declaration-content {
  font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
  line-height: 1.6;
}

.declaration-section {
  margin: 1rem 0;
}

.declaration-meta {
  color: var(--text-color-secondary);
  font-size: 0.95rem;
  margin: 0.25rem 0;
  font-style: italic;
}

.section-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--text-color);
  margin: 0 0 0.5rem 0;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.subsection-title {
  font-size: 1rem;
  font-weight: 600;
  color: var(--text-color);
  margin: 0 0 0.5rem 0;
}

.declaration-text {
  color: var(--text-color);
  font-size: 1rem;
  margin: 0;
  text-align: justify;
}

.compliance-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.compliance-item {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  padding: 0.5rem;
  margin: 0.35rem 0;
  background-color: var(--surface-50);
  border-left: 3px solid var(--green-500);
  border-radius: 4px;
  transition: background-color 0.2s;
}

.compliance-item:hover {
  background-color: var(--surface-100);
}

.compliance-item i {
  margin-top: 0.15rem;
  flex-shrink: 0;
}

.compliance-item span {
  color: var(--text-color);
  font-size: 0.95rem;
  flex: 1;
}

.liability-section {
  margin-top: 1rem;
}

.liability-text {
  color: var(--text-color);
  font-size: 0.95rem;
  font-weight: 500;
  line-height: 1.5;
}

/* Print styles for formal documents */
@media print {
  .responsabilities-container {
    max-width: 100%;
    margin: 0;
    padding: 0;
  }

  .legal-declaration-card {
    box-shadow: none;
    border: 2px solid #000;
  }

  .declaration-header {
    border-bottom: 2px solid #000;
  }

  .compliance-item {
    break-inside: avoid;
  }
}
</style>
