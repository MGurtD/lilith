import { definePreset } from "@primeuix/themes";
import Aura from "@primeuix/themes/aura";

/**
 * "Taller" preset: steel neutrals for the whole interface; the tenant's
 * branding palette (see store/branding.ts) only drives actions, active
 * states and focus.
 */
export const TallerPreset = definePreset(Aura, {
  primitive: {
    borderRadius: {
      none: "0",
      xs: "2px",
      sm: "3px",
      md: "4px",
      lg: "6px",
      xl: "8px",
    },
    steel: {
      50: "#F2F4F5",
      100: "#E6EAED",
      200: "#DADFE3",
      300: "#C9D0D6",
      400: "#A3ACB5",
      500: "#6B7580",
      600: "#5F6973",
      700: "#3F4852",
      800: "#2C343C",
      900: "#1C2126",
      950: "#12161A",
    },
  },
  semantic: {
    formField: {
      paddingX: "0.75rem",
      paddingY: "0.5rem",
      borderRadius: "{border.radius.md}",
      // size="small" is used widely (filters, tables): keep it legible at 14px,
      // only a little tighter than the default.
      sm: {
        fontSize: "1rem",
        paddingX: "0.625rem",
        paddingY: "0.375rem",
      },
    },
    colorScheme: {
      light: {
        // White text on {primary.600} reaches 4.5:1 for most palettes;
        // branding.ts moves the lighter ones one step darker.
        primary: {
          color: "{primary.600}",
          contrastColor: "#ffffff",
          hoverColor: "{primary.700}",
          activeColor: "{primary.800}",
        },
        surface: {
          0: "#ffffff",
          50: "{steel.50}",
          100: "{steel.100}",
          200: "{steel.200}",
          300: "{steel.300}",
          400: "{steel.400}",
          500: "{steel.500}",
          600: "{steel.600}",
          700: "{steel.700}",
          800: "{steel.800}",
          900: "{steel.900}",
          950: "{steel.950}",
        },
        text: {
          color: "{surface.900}",
          hoverColor: "{surface.950}",
          mutedColor: "{surface.600}",
          hoverMutedColor: "{surface.700}",
        },
        formField: {
          color: "{surface.900}",
          borderColor: "{surface.300}",
          hoverBorderColor: "{surface.400}",
          placeholderColor: "{surface.500}",
        },
      },
    },
  },
  components: {
    card: {
      root: {
        borderRadius: "{border.radius.md}",
        shadow: "0 0 0 1px {surface.200}",
      },
      body: {
        padding: "1rem",
      },
    },
    datatable: {
      headerCell: {
        background: "{surface.50}",
        color: "{text.muted.color}",
        padding: "0.5rem 0.75rem",
        sm: { padding: "0.5rem 0.75rem" },
      },
      bodyCell: {
        padding: "0.5rem 0.75rem",
        sm: { padding: "0.4375rem 0.75rem" },
      },
      columnTitle: {
        fontWeight: "600",
      },
      colorScheme: {
        light: {
          row: {
            stripedBackground: "#FAFBFB",
          },
        },
      },
    },
    tag: {
      root: {
        fontWeight: "500",
        padding: "0.1875rem 0.5rem",
      },
    },
  },
});
