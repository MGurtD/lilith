import { normalizeColor } from "@/utils/functions";

// The machine status is the strongest signal on a plant screen: a solid
// fill in the status's catalogue colour with ink chosen for contrast
// (frontend/docs/plant-mes-redesign.md). Document statuses keep tinted Tags.

const INK_ON_LIGHT = "var(--p-steel-900)";
const INK_ON_DARK = "#FFFFFF";

/** Hatched band for a workcenter that sends no data. */
export const NO_DATA_BACKGROUND =
  "repeating-linear-gradient(135deg, var(--p-steel-300) 0 6px, var(--p-steel-100) 6px 12px)";

const toLinear = (channel: number): number => {
  const v = channel / 255;
  return v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4);
};

const luminance = (hex: string): number => {
  const value = hex.replace("#", "");
  const [r, g, b] = [0, 2, 4].map((i) => toLinear(parseInt(value.slice(i, i + 2), 16)));
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
};

/** White when it reaches 4.5:1 on the colour, otherwise steel 900. */
export function statusInk(color: string): string {
  const contrastWithWhite = 1.05 / (luminance(normalizeColor(color)) + 0.05);
  return contrastWithWhite >= 4.5 ? INK_ON_DARK : INK_ON_LIGHT;
}

export interface StatusSignal {
  /** Fill and ink for the solid signal (badge, placa, current status key). */
  style: Record<string, string>;
  /** Fill for a thin band or a status light. */
  band: string;
  /** No status: hatched band and an outlined badge. */
  noData: boolean;
}

export function statusSignal(color: string | null | undefined): StatusSignal {
  if (!color) {
    return {
      noData: true,
      band: NO_DATA_BACKGROUND,
      style: {
        background: "var(--p-surface-0)",
        color: "var(--p-steel-600)",
        boxShadow: "inset 0 0 0 1px var(--p-steel-300)",
      },
    };
  }
  const fill = normalizeColor(color);
  return {
    noData: false,
    band: fill,
    style: { background: fill, color: statusInk(fill) },
  };
}
