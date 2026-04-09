import { defineStore } from "pinia";
import Services from "../services";
import { StockListItem } from "../types";

export const useStockStore = defineStore({
    id: "stock",
    state: () => ({
        stocks: undefined as Array<StockListItem> | undefined,
    }),
    getters: {
        availableReferences: (state) => {
            const references = new Map<string, { id: string; code: string; description: string }>();

            state.stocks?.forEach((stock) => {
                if (!references.has(stock.referenceId)) {
                    references.set(stock.referenceId, {
                        id: stock.referenceId,
                        code: stock.referenceCode,
                        description: stock.referenceDescription,
                    });
                }
            });

            return Array.from(references.values()).sort((left, right) =>
                `${left.code} - ${left.description}`.localeCompare(
                    `${right.code} - ${right.description}`,
                ),
            );
        },
    },
    actions: {
        async fetchStocks() {
            this.stocks = await Services.Stock.getAll();
        },
    },
});