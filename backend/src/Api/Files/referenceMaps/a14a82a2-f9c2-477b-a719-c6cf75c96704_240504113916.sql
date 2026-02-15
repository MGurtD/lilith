/* MAPEX-16369 - Partir paro en varios paros */

-- Parámetro del configurador para habilitar funcionalidad
EXECUTE sys_p_CreateParam 'xPMDowntime#SplitOnJustificationForm', 'Allow split capability on standard justification screen', 1, 1, 'Downtime', 9, 0, '', 6, '', 'Core', 'Core', 1, 1, 3, 9, 1;

-- Columna tipo check para marcar los paros divididos mediante la nueva operativa
IF NOT EXISTS(SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'his_prod_paro' AND COLUMN_NAME = 'partirParo')
BEGIN
    ALTER TABLE [dbo].[his_prod_paro] ADD [partirParo] BIT NULL CONSTRAINT DF_cfg_tipotrazabilidad_partirParo DEFAULT 0
    EXECUTE sp_addextendedproperty N'MS_Description'
    ,N'Check than indicates if a register has been manipulated from the split downtime operative.'
    ,N'SCHEMA'
    ,N'dbo'
    ,N'TABLE'
    ,N'his_prod_paro'
    ,N'COLUMN'
    ,N'partirParo';
END

-- Traducciones
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#FORMPANELSPLITDOWNTIME', 'Downtime Splitting', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELORIGINALDOWNTIME', 'Original Reason', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELDOWNTIMEDURATION', 'Duration', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELDOWNTIME', 'New Reason', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELNEWDOWNTIMEDURATION', 'New Duration', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELREMAININGDURATION', 'Remaining', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELREMAININGASNOTJUSTIFIED', 'Keep remaining as ''NOT JUSTIFIED''?', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELREMAININGASNOTJUSTIFIED', 'Keep remaining as ''NOT JUSTIFIED''?', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION|INVALID_SPLIT', 'Invalid duration, it has to be lower than the original downtime duration.', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION|CANNOTSPLITCURRENTDOWNTIME', 'The current downtime cannot be splitted with this operative.', 'ENG', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION|MUCHDOWNTIMESSELECTED', 'The splitted operative is not compatible with more than one downtime.', 'ENG', 'MAPEXBP', 0;

EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#FORMPANELSPLITDOWNTIME', 'Partición del Paro', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELORIGINALDOWNTIME', 'Paro Original', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELDOWNTIMEDURATION', 'Duración', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELDOWNTIME', 'Nuevo Paro', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELNEWDOWNTIMEDURATION', 'Nueva Duración', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELREMAININGDURATION', 'Duración Rest.', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION#LABELREMAININGASNOTJUSTIFIED', '¿Dejar restante como ''NO JUSTIFICADO''?', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION|INVALID_SPLIT', 'La duración indicada no es correcta, debe ser menor a la duración del paro original.', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION|CANNOTSPLITCURRENTDOWNTIME', 'No se puede partir el paro actual mediante esta operativa.', 'ES', 'MAPEXBP', 0;
EXEC [dbo].[sys_addOrUpdateTranslation] 'FRMDOWNTIMEJUSTIFICATION|MUCHDOWNTIMESSELECTED', 'La operativa seleccionada no es compatible con múltiples paros.', 'ES', 'MAPEXBP', 0;