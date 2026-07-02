-- Migration: Add SAP priority code/text columns to LYNXCORE_AVISO
-- Preserva el código de prioridad de SAP (PRIOK) y su texto (PRIOKX) enviados desde el móvil.
-- Ejecutar una sola vez contra la base Oracle antes de desplegar el cambio.
--
-- Nota: la tabla viva es LYNXCORE_AVISO (mapeada por NoticeConfiguration).
-- Scripts antiguos (create_tables.py / add_notice_approval_columns.sql) referencian
-- el nombre anterior LYNX_PM_AVISO; usar el nombre real del entorno destino.

ALTER TABLE LYNXCORE_AVISO ADD (
    PRIORITY_CODE VARCHAR2(10 CHAR),
    PRIORITY_TEXT VARCHAR2(100 CHAR)
);

-- Sin backfill: columnas nuevas quedan NULL en avisos existentes (compatibles con el int PRIORITY previo).
