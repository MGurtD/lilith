import { CARD_META_MAX, ColumnType, type CardLayout, type Column } from "./types";

// Columns a card cannot show as text.
function fitsInCard(col: Column): boolean {
  return col.columnType !== ColumnType.ProgressBar;
}

/**
 * The card built from the visible columns when neither the view nor the
 * screen declares one: the status as badge, the first amount as trailing,
 * the first remaining column as title, a lookup as subtitle and the next
 * columns as meta.
 */
export function deriveCardLayout(visibleColumns: Column[]): CardLayout {
  const candidates = visibleColumns.filter(fitsInCard);
  const used = new Set<string>();
  const take = (col: Column | undefined) => {
    if (!col) return undefined;
    used.add(col.field);
    return col.field;
  };
  const free = () => candidates.filter((c) => !used.has(c.field));

  const badge = take(candidates.find((c) => c.columnType === ColumnType.Status));
  const trailing = take(
    candidates.find((c) => c.columnType === ColumnType.Currency),
  );
  const title = take(free()[0]);
  const subtitle = take(free().find((c) => c.columnType === ColumnType.Lookup));
  const meta = free()
    .slice(0, CARD_META_MAX)
    .map((c) => c.field);

  return { title, subtitle, badge, trailing, meta };
}

/**
 * The card a table shows: the view's card, else the screen's, else one
 * derived from the columns. Slots pointing at hidden or removed columns
 * are dropped, so a column hidden in the view is hidden in the card too.
 */
export function resolveCardLayout(
  viewCard: CardLayout | null | undefined,
  screenCard: CardLayout | null | undefined,
  visibleColumns: Column[],
): CardLayout {
  const source =
    viewCard ?? screenCard ?? deriveCardLayout(visibleColumns);
  const visible = new Map(
    visibleColumns.filter(fitsInCard).map((c) => [c.field, c]),
  );
  const used = new Set<string>();
  const keep = (field: string | undefined) => {
    if (!field || !visible.has(field) || used.has(field)) return undefined;
    used.add(field);
    return field;
  };

  let title = keep(source.title);
  const subtitle = keep(source.subtitle);
  const badge = keep(source.badge);
  const trailing = keep(source.trailing);
  // Every card needs a title: fall back to the first column still free.
  title ??= keep(
    visibleColumns.find((c) => visible.has(c.field) && !used.has(c.field))
      ?.field,
  );
  const metaFields = new Set(source.meta ?? []);
  // Meta follows the view's column order, not the order it was picked in.
  const meta = visibleColumns
    .filter((c) => metaFields.has(c.field))
    .map((c) => keep(c.field))
    .filter((field): field is string => !!field)
    .slice(0, CARD_META_MAX);

  return { title, subtitle, badge, trailing, meta };
}
