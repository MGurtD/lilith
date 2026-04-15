export interface Country {
  /** Display name in Catalan */
  name: string;
  /** ISO 3166-1 alpha-2 code */
  code: string;
}

/**
 * EU-27 + common trade partners, sorted alphabetically by Catalan name.
 * Flag icons available at https://flagcdn.com/w20/{code.toLowerCase()}.png
 */
export const COUNTRIES: Country[] = [
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
];

/**
 * Find a country by its ISO alpha-2 code.
 */
export function getCountryByCode(code: string): Country | undefined {
  return COUNTRIES.find((c) => c.code === code);
}

/**
 * Returns the flag image URL for a given ISO alpha-2 code.
 */
export function getFlagUrl(code: string, width: number = 20): string {
  return `https://flagcdn.com/w${width}/${code.toLowerCase()}.png`;
}
