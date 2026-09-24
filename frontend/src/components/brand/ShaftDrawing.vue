<!-- Decorative drawing of a stepped shaft with dimensions, inked in the tenant's colour. -->
<template>
  <svg class="shaft-drawing" viewBox="0 0 640 320" aria-hidden="true" focusable="false">
    <defs>
      <marker
        id="shaft-drawing-arrow"
        viewBox="0 0 10 10"
        refX="5"
        refY="5"
        markerWidth="8"
        markerHeight="8"
        orient="auto-start-reverse"
      >
        <path d="M0 1.5L10 5 0 8.5" class="shaft-drawing__arrow" />
      </marker>
    </defs>
    <path class="shaft-drawing__outline" d="M60 102H180V112H430V120H560V180H430V188H180V198H60Z" />
    <path class="shaft-drawing__thin" d="M66 102V198M554 120V180" />
    <rect class="shaft-drawing__thin" x="250" y="143" width="110" height="14" rx="7" />
    <path class="shaft-drawing__axis" d="M36 150H584" />
    <path class="shaft-drawing__thin" d="M60 204V262M560 186V262M180 96V38M430 106V38" />
    <path class="shaft-drawing__dim" d="M64 250H556" />
    <text class="shaft-drawing__text" x="310" y="242" text-anchor="middle">320</text>
    <path class="shaft-drawing__dim" d="M184 50H426" />
    <text class="shaft-drawing__text" x="305" y="42" text-anchor="middle">160</text>
    <path class="shaft-drawing__dim" d="M120 106V194" />
    <text class="shaft-drawing__text" x="128" y="92">Ø40</text>
    <path class="shaft-drawing__dim" d="M395 116V184" />
    <text class="shaft-drawing__text" x="360" y="104">Ø32</text>
    <path class="shaft-drawing__dim" d="M500 124V176" />
    <text class="shaft-drawing__text" x="486" y="112">Ø25</text>
  </svg>
</template>

<style scoped>
.shaft-drawing {
  width: 100%;
  height: auto;
}

.shaft-drawing__outline {
  fill: none;
  stroke: var(--p-primary-color);
  stroke-width: 1.6;
  stroke-linejoin: round;
}

.shaft-drawing__thin {
  fill: none;
  stroke: var(--p-primary-color);
  stroke-width: 1;
  opacity: 0.55;
}

.shaft-drawing__axis {
  fill: none;
  stroke: var(--p-primary-color);
  stroke-width: 1.2;
  stroke-dasharray: 22 5 3 5;
}

.shaft-drawing__dim {
  fill: none;
  stroke: var(--p-primary-color);
  stroke-width: 1;
  opacity: 0.8;
  marker-start: url(#shaft-drawing-arrow);
  marker-end: url(#shaft-drawing-arrow);
}

.shaft-drawing__arrow {
  fill: var(--p-primary-color);
}

.shaft-drawing__text {
  font-family: var(--font-condensed);
  font-size: 15px;
  font-weight: 500;
  fill: var(--p-surface-700);
  font-variant-numeric: tabular-nums;
}
</style>
