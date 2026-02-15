import { Exercise } from "../modules/shared/types";

export enum BaseInputType {
  NUMERIC = 1,
  TEXT = 2,
  PASSWORD = 3,
  CURRENCY = 4,
}

export enum FormActionMode {
  CREATE,
  EDIT,
}

export interface MenuItem {
  icon: string;
  title: string;
  href?: string;
  backButtonVisible?: boolean;
  child?: Array<MenuItem>;
}

export interface SidebarConfig {
  collapsed: boolean;
  hideToggle: boolean;
  menus: Array<any>;
}

export interface ChartOptions {
  labels: string[];
  datasets?: ChartDataSet[];
}

export interface ChartDataSet {
  label: string;
  data: number[];
  backgroundColor: string[];
  borderColor: string[];
  borderWidth: number;
}

export interface DialogOptions {
  visible: boolean;
  title: string;
  closable: boolean;
  position: string;
  modal: boolean;
  isPdf?: boolean;
  selectedFile?: any;
}

export enum FileType {
  DOCUMENT = 0,
  IMAGE = 1,
}
