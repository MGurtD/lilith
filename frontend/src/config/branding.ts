import logoMain from "../assets/images/logo.jpg";
import logoSidebar from "../assets/images/logo-header-white.png";

export const DEFAULT_BRAND_NAME = "Temges";
export const DEFAULT_MAIN_LOGO = logoMain;
export const DEFAULT_SIDEBAR_LOGO = logoSidebar;

export const getBrandMonogram = (brandName: string): string =>
  brandName.trim().charAt(0).toUpperCase() || "T";
