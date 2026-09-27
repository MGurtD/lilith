import logoMain from "../assets/images/logo.jpg";
import logoSidebar from "../assets/images/logo-header-white.png";
import reportWatermark from "../assets/images/report-watermark.png";

export const DEFAULT_BRAND_NAME = "Temges";
export const DEFAULT_MAIN_LOGO = logoMain;
export const DEFAULT_SIDEBAR_LOGO = logoSidebar;
// Same image the backend prints on PDF reports when no watermark is uploaded.
export const DEFAULT_REPORT_WATERMARK = reportWatermark;

export const getBrandMonogram = (brandName: string): string =>
  brandName.trim().charAt(0).toUpperCase() || "T";
