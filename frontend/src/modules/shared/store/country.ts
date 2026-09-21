import { defineStore } from "pinia";
import { Country } from "../types";

export const useCountryStore = defineStore({
  id: "country",
  state: () => ({
    countries: [
      { name: "Alemanya", code: "DE" },
      { name: "Andorra", code: "AD" },
      { name: "Aràbia Saudita", code: "SA" },
      { name: "Argentina", code: "AR" },
      { name: "Austràlia", code: "AU" },
      { name: "Àustria", code: "AT" },
      { name: "Bèlgica", code: "BE" },
      { name: "Brasil", code: "BR" },
      { name: "Bulgària", code: "BG" },
      { name: "Canadà", code: "CA" },
      { name: "Croàcia", code: "HR" },
      { name: "Dinamarca", code: "DK" },
      { name: "Emirats Àrabs Units", code: "AE" },
      { name: "Eslovàquia", code: "SK" },
      { name: "Eslovènia", code: "SI" },
      { name: "Espanya", code: "ES" },
      { name: "Estats Units", code: "US" },
      { name: "Estònia", code: "EE" },
      { name: "Finlàndia", code: "FI" },
      { name: "França", code: "FR" },
      { name: "Grècia", code: "GR" },
      { name: "Hongria", code: "HU" },
      { name: "Índia", code: "IN" },
      { name: "Irlanda", code: "IE" },
      { name: "Itàlia", code: "IT" },
      { name: "Japó", code: "JP" },
      { name: "Letònia", code: "LV" },
      { name: "Lituània", code: "LT" },
      { name: "Luxemburg", code: "LU" },
      { name: "Malta", code: "MT" },
      { name: "Marroc", code: "MA" },
      { name: "Mèxic", code: "MX" },
      { name: "Noruega", code: "NO" },
      { name: "Països Baixos", code: "NL" },
      { name: "Polònia", code: "PL" },
      { name: "Portugal", code: "PT" },
      { name: "Regne Unit", code: "GB" },
      { name: "República Txeca", code: "CZ" },
      { name: "Romania", code: "RO" },
      { name: "Suècia", code: "SE" },
      { name: "Suïssa", code: "CH" },
      { name: "Turquia", code: "TR" },
      { name: "Xina", code: "CN" },
      { name: "Xipre", code: "CY" },
    ] as Country[],
  }),
  getters: {},
  actions: {
    getCountryByCode(code: string) {
      return this.countries.find((country) => country.code === code);
    },
    getFlagUrl(code: string, width: number = 20) {
      return `https://flagcdn.com/w${width}/${code.toLowerCase()}.png`;
    },
  },
});
