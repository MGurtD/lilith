import { defineComponent, shallowRef, type Slot } from "vue";

/**
 * Renders one template block in two places of the same component, like
 * VueUse's createReusableTemplate. `Define` captures its default slot and
 * renders nothing; `Reuse` renders the captured slot. The block keeps the
 * scope of the component that declares it. Place `Define` before any
 * `Reuse` in the template.
 */
export function createReusableTemplate() {
  const render = shallowRef<Slot | undefined>();

  const Define = defineComponent({
    setup(_, { slots }) {
      return () => {
        render.value = slots.default;
        return null;
      };
    },
  });

  const Reuse = defineComponent({
    setup() {
      return () => render.value?.();
    },
  });

  return [Define, Reuse] as const;
}
