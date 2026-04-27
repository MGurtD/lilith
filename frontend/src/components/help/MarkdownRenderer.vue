<template>
  <article ref="containerRef" class="markdown-content" v-html="renderedHtml"></article>
</template>

<script setup lang="ts">
import DOMPurify from "dompurify";
import MarkdownIt from "markdown-it";
import type { Options } from "markdown-it";
import type Renderer from "markdown-it/lib/renderer.mjs";
import type Token from "markdown-it/lib/token.mjs";
import mermaid from "mermaid";
import { nextTick, ref, watch } from "vue";
import { i18n } from "@/i18n";

interface MermaidBlock {
  id: string;
  code: string;
}

const props = defineProps<{
  markdown: string;
}>();

const containerRef = ref<HTMLElement | null>(null);
const renderedHtml = ref("");

let renderVersion = 0;

mermaid.initialize({
  startOnLoad: false,
  securityLevel: "strict",
});

DOMPurify.addHook("uponSanitizeAttribute", (_node, data) => {
  if (data.attrName.toLowerCase().startsWith("on")) {
    data.keepAttr = false;
  }
});

const createMarkdownRenderer = () => {
  const mermaidBlocks: MermaidBlock[] = [];
  const markdown = new MarkdownIt({
    html: false,
    linkify: true,
    typographer: true,
    breaks: true,
  });

  const defaultFence = markdown.renderer.rules.fence;
  markdown.renderer.rules.fence = (
    tokens: Token[],
    idx: number,
    options: Options,
    env: unknown,
    self: Renderer,
  ) => {
    const token = tokens[idx];
    const info = token.info.trim().toLowerCase();

    if (info === "mermaid") {
      const id = `help-mermaid-${idx}-${crypto.randomUUID()}`;
      mermaidBlocks.push({ id, code: token.content });
      return `<div class="markdown-mermaid" data-mermaid-id="${id}"></div>`;
    }

    if (defaultFence) {
      return defaultFence(tokens, idx, options, env, self);
    }

    return self.renderToken(tokens, idx, options);
  };

  const defaultLinkOpen = markdown.renderer.rules.link_open;
  markdown.renderer.rules.link_open = (
    tokens: Token[],
    idx: number,
    options: Options,
    env: unknown,
    self: Renderer,
  ) => {
    const token = tokens[idx];
    token.attrSet("target", "_blank");
    token.attrSet("rel", "noopener noreferrer");

    if (defaultLinkOpen) {
      return defaultLinkOpen(tokens, idx, options, env, self);
    }

    return self.renderToken(tokens, idx, options);
  };

  return {
    html: markdown.render(props.markdown),
    mermaidBlocks,
  };
};

const sanitizeHtml = (html: string): string => {
  return DOMPurify.sanitize(html, {
    USE_PROFILES: { html: true },
    ADD_ATTR: ["target", "rel", "class", "data-mermaid-id"],
  });
};

const renderMermaidBlocks = async (blocks: MermaidBlock[], version: number) => {
  const container = containerRef.value;
  if (!container) {
    return;
  }

  for (const block of blocks) {
    if (version !== renderVersion) {
      return;
    }

    const target = container.querySelector<HTMLElement>(`[data-mermaid-id="${block.id}"]`);
    if (!target) {
      continue;
    }

    try {
      const { svg, bindFunctions } = await mermaid.render(`svg-${block.id}`, block.code);
      // Mermaid usa SVG + foreignObject/HTML intern per alguns labels.
      // El sanejat posterior del SVG estava eliminant aquesta part i feia desapareixer el text.
      // Confiem en securityLevel "strict" de Mermaid i en el fet que el Markdown provingui del repo.
      target.innerHTML = svg;
      bindFunctions?.(target);
    } catch {
      target.textContent = i18n.global.t("help.messages.mermaidError");
    }
  }
};

const updateContent = async () => {
  renderVersion += 1;
  const currentVersion = renderVersion;
  const { html, mermaidBlocks } = createMarkdownRenderer();

  renderedHtml.value = sanitizeHtml(html);
  await nextTick();
  await renderMermaidBlocks(mermaidBlocks, currentVersion);
};

watch(
  () => props.markdown,
  () => {
    void updateContent();
  },
  { immediate: true },
);
</script>

<style scoped>
.markdown-content {
  color: var(--p-surface-800);
  line-height: 1.65;
}

.markdown-content:deep(h1),
.markdown-content:deep(h2),
.markdown-content:deep(h3) {
  color: var(--p-surface-950);
  line-height: 1.3;
}

.markdown-content:deep(h1) {
  font-size: 1.8rem;
  margin-bottom: 1rem;
}

.markdown-content:deep(h2) {
  margin-top: 1.75rem;
}

.markdown-content:deep(p),
.markdown-content:deep(ul),
.markdown-content:deep(ol),
.markdown-content:deep(pre) {
  margin-bottom: 1rem;
}

.markdown-content:deep(ul),
.markdown-content:deep(ol) {
  padding-left: 1.25rem;
}

.markdown-content:deep(code) {
  background: var(--p-surface-100);
  border-radius: 6px;
  padding: 0.15rem 0.35rem;
  font-size: 0.92em;
}

.markdown-content:deep(pre) {
  background: var(--p-surface-900);
  color: var(--p-surface-0);
  border-radius: 12px;
  padding: 1rem;
  overflow: auto;
}

.markdown-content:deep(pre code) {
  background: transparent;
  color: inherit;
  padding: 0;
}

.markdown-content:deep(blockquote) {
  border-left: 4px solid var(--p-primary-300);
  margin: 1rem 0;
  padding: 0.75rem 1rem;
  background: var(--p-primary-50);
}

.markdown-content:deep(table) {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 1rem;
}

.markdown-content:deep(th),
.markdown-content:deep(td) {
  border: 1px solid var(--p-surface-200);
  padding: 0.6rem;
  text-align: left;
}

.markdown-content:deep(.markdown-mermaid) {
  overflow-x: auto;
  padding: 1rem;
  margin-bottom: 1rem;
  border: 1px solid var(--p-surface-200);
  border-radius: 12px;
  background: #ffffff;
}
</style>
