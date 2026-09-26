import type { MenuItem } from "@/types/component";

/** A menu screen the searcher can open: a sidebar entry with a route. */
export interface MenuSearchEntry {
  href: string;
  title: string;
  icon?: string;
  /** Titles of the entries above it, module first ("Compres", "Comandes"). */
  trail: string[];
  /** Position in the sidebar, used to break ranking ties. */
  order: number;
}

export interface MenuSearchResult {
  entry: MenuSearchEntry;
  score: number;
  /** Matched ranges in `entry.title` as [start, end) indices. */
  ranges: Array<[number, number]>;
}

export interface TitleSegment {
  text: string;
  match: boolean;
}

interface Normalized {
  text: string;
  /** Index in the source string of each normalized character. */
  map: number[];
}

// Case, accents and the Catalan middle dot are ignored: "col·lecció",
// "colleccio" and "col.leccio" all match each other.
const normalizeChar = (char: string): string =>
  char
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[\u00b7.]/g, "");

const normalize = (source: string): Normalized => {
  let text = "";
  const map: number[] = [];
  for (let index = 0; index < source.length; index++) {
    for (const char of normalizeChar(source[index])) {
      text += char;
      map.push(index);
    }
  }
  return { text, map };
};

const isWordStart = (text: string, index: number): boolean =>
  index === 0 || /[\s'/\-(]/.test(text[index - 1]);

/** True when the route belongs to the entry: its own path or a detail below it. */
export const ownsRoute = (entry: MenuItem, path: string): boolean =>
  (!!entry.href &&
    (path === entry.href || path.startsWith(`${entry.href}/`))) ||
  (entry.child ?? []).some((child) => ownsRoute(child, path));

/** Flattens the sidebar tree into the screens it can open, in sidebar order. */
export const flattenMenu = (menus: MenuItem[]): MenuSearchEntry[] => {
  const entries: MenuSearchEntry[] = [];
  const walk = (items: MenuItem[], trail: string[]) => {
    for (const item of items) {
      if (item.href) {
        entries.push({
          href: item.href,
          title: item.title,
          icon: item.icon || undefined,
          trail,
          order: entries.length,
        });
      }
      if (item.child?.length) walk(item.child, [...trail, item.title]);
    }
  };
  walk(menus, []);
  return entries;
};

/** The screen a route belongs to; detail routes resolve to their list entry. */
export const findOwningEntry = (
  entries: MenuSearchEntry[],
  path: string,
): MenuSearchEntry | undefined =>
  entries.find((entry) => entry.href === path) ??
  entries
    .filter((entry) => path.startsWith(`${entry.href}/`))
    .sort((a, b) => b.href.length - a.href.length)[0];

interface IndexedEntry {
  entry: MenuSearchEntry;
  title: Normalized;
  trail: string;
}

interface TokenMatch {
  score: number;
  ranges: Array<[number, number]>;
}

// Ranking tiers: exact > prefix > word start > substring > letters in order
// > parent path. Letters in order must each start a word or follow the
// previous one, so "ofab" finds "Ordres de fabricació" without noise.
const matchToken = (
  token: string,
  indexed: IndexedEntry,
): TokenMatch | null => {
  const title = indexed.title.text;
  if (title === token) return { score: 100, ranges: [[0, token.length]] };
  if (title.startsWith(token))
    return { score: 80, ranges: [[0, token.length]] };

  let first = -1;
  for (
    let at = title.indexOf(token);
    at !== -1;
    at = title.indexOf(token, at + 1)
  ) {
    if (first < 0) first = at;
    if (isWordStart(title, at))
      return { score: 60, ranges: [[at, at + token.length]] };
  }
  if (first >= 0) return { score: 40, ranges: [[first, first + token.length]] };

  if (token.length >= 2) {
    const ranges: Array<[number, number]> = [];
    let from = 0;
    for (let k = 0; k < token.length; k++) {
      let found = -1;
      for (let x = from; x < title.length; x++) {
        if (
          title[x] === token[k] &&
          (isWordStart(title, x) || (k > 0 && x === from))
        ) {
          found = x;
          break;
        }
      }
      if (found < 0) break;
      ranges.push([found, found + 1]);
      from = found + 1;
    }
    if (ranges.length === token.length) return { score: 20, ranges };
  }

  if (indexed.trail.includes(token)) return { score: 10, ranges: [] };
  return null;
};

/** Builds a reusable search over the entries for one menu and language. */
export const createMenuSearch = (entries: MenuSearchEntry[]) => {
  const index: IndexedEntry[] = entries.map((entry) => ({
    entry,
    title: normalize(entry.title),
    trail: normalize(entry.trail.join(" ")).text,
  }));

  const search = (
    query: string,
    recents: string[] = [],
  ): MenuSearchResult[] => {
    const tokens = normalize(query.trim()).text.split(/\s+/).filter(Boolean);
    if (!tokens.length) return [];

    const results: MenuSearchResult[] = [];
    for (const indexed of index) {
      let score = 0;
      const ranges: Array<[number, number]> = [];
      let matched = true;
      for (const token of tokens) {
        const match = matchToken(token, indexed);
        if (!match) {
          matched = false;
          break;
        }
        score += match.score;
        for (const [start, end] of match.ranges) {
          ranges.push([
            indexed.title.map[start],
            indexed.title.map[end - 1] + 1,
          ]);
        }
      }
      if (!matched) continue;

      // Recently opened screens win ties and near-ties.
      const recent = recents.indexOf(indexed.entry.href);
      if (recent >= 0) score += 15 - recent * 2;
      results.push({ entry: indexed.entry, score, ranges });
    }

    return results.sort(
      (a, b) => b.score - a.score || a.entry.order - b.entry.order,
    );
  };

  return { search };
};

/** Splits a title into plain and matched parts for highlighting. */
export const highlightTitle = (
  title: string,
  ranges: Array<[number, number]>,
): TitleSegment[] => {
  const marked = new Array<boolean>(title.length).fill(false);
  for (const [start, end] of ranges) {
    for (let index = start; index < end; index++) marked[index] = true;
  }
  const segments: TitleSegment[] = [];
  for (let index = 0; index < title.length; index++) {
    const last = segments[segments.length - 1];
    if (last && last.match === marked[index]) last.text += title[index];
    else segments.push({ text: title[index], match: marked[index] });
  }
  return segments;
};
