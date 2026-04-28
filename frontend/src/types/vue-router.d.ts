import "vue-router";

declare module "vue-router" {
  interface RouteMeta {
    public?: boolean;
    roles?: string[];
    helpKey?: string;
  }
}

export {};
