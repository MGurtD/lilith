import { defineStore } from "pinia";
import Services from "../services";
import { Stock } from "../types";

export const useStockStore = defineStore({
    id:"stock",
    state: () => ({
        stocks: undefined as Array<Stock> | undefined,
    }),
    getters: {},
    actions: {
        async fetchStocks() {
            this.stocks = await Services.Stock.getAll();
        },
    },
});