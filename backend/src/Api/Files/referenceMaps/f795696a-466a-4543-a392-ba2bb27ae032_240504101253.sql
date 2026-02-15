UPDATE IoT.cfg_tag_target 
SET frec_ejecucion = IIF(cfg_graf.GrafCambio = 1, 0, cfg_graf.GrafFrec)
FROM IoT.cfg_tag
JOIN cfg_graf ON IoT.cfg_tag.cod_tag = cfg_graf.Cod_graf
JOIN IoT.cfg_tag_target ON IoT.cfg_tag.id_tag = IoT.cfg_tag_target.id_tag AND cfg_tag_target.id_target = 10
WHERE
	cfg_graf.GrafFrec <> IoT.cfg_tag_target.frec_ejecucion