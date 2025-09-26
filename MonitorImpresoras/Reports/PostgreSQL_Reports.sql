-- =====================================================
-- REPORTES PRIORITARIOS PARA POSTGRESQL 17
-- Sistema Multi-tenant de Monitor de Impresoras
-- =====================================================

-- =====================================================
-- REPORTE 1: PRINTER USAGE (Uso de Impresoras)
-- Total de páginas por impresora en los últimos 30 días
-- =====================================================
CREATE OR REPLACE VIEW vw_printer_usage_report AS
SELECT
    p.id,
    p.name AS printer_name,
    p.model AS printer_model,
    p.location,
    d.name AS department_name,
    COALESCE(SUM(j.pages), 0) AS total_pages,
    COALESCE(COUNT(*), 0) AS total_jobs,
    COALESCE(SUM(CASE WHEN j.color_pages > 0 THEN 1 ELSE 0 END), 0) AS color_jobs,
    COALESCE(SUM(CASE WHEN j.duplex = true THEN 1 ELSE 0 END), 0) AS duplex_jobs,
    COALESCE(AVG(j.pages), 0) AS avg_pages_per_job,
    MIN(j.created_at) AS first_job_date,
    MAX(j.created_at) AS last_job_date,
    EXTRACT(EPOCH FROM (MAX(j.created_at) - MIN(j.created_at))) / 86400 AS days_active
FROM printers p
LEFT JOIN printjobs j ON p.id = j.printer_id
    AND j.created_at >= (CURRENT_DATE - INTERVAL '30 days')
LEFT JOIN departments d ON p.department_id = d.id
WHERE p.is_active = true
GROUP BY p.id, p.name, p.model, p.location, d.name
ORDER BY total_pages DESC;

-- =====================================================
-- REPORTE 2: CONSUMABLE USAGE (Uso de Consumibles)
-- Estado actual de consumibles (tóner, fusor, kit)
-- =====================================================
CREATE OR REPLACE VIEW vw_consumable_usage_report AS
SELECT
    p.id AS printer_id,
    p.name AS printer_name,
    p.model AS printer_model,
    c.part_type,
    c.part_identifier,
    c.level_percent,
    c.threshold_percent,
    c.status,
    c.last_updated,
    c.install_date,
    CASE
        WHEN c.level_percent <= 5 THEN 'Crítico - Reemplazo inmediato requerido'
        WHEN c.level_percent <= 15 THEN 'Bajo - Programar reemplazo'
        WHEN c.level_percent <= 30 THEN 'Medio - Monitorear'
        ELSE 'Normal - Sin acción requerida'
    END AS recommendation,
    ROUND(
        CASE
            WHEN c.level_percent > 0 THEN (c.pages_remaining::numeric / c.level_percent * 100)
            ELSE c.pages_remaining::numeric
        END, 0
    ) AS estimated_total_pages,
    c.replacement_cost,
    c.supplier_name,
    c.warranty_expires_at
FROM printer_consumable_parts c
JOIN printers p ON p.id = c.printer_id
WHERE c.part_type IN ('Toner', 'Fusor', 'MaintenanceKit', 'Drum', 'Belt')
    AND p.is_active = true
ORDER BY
    CASE
        WHEN c.level_percent <= 5 THEN 1
        WHEN c.level_percent <= 15 THEN 2
        WHEN c.level_percent <= 30 THEN 3
        ELSE 4
    END,
    p.name,
    c.part_type;

-- =====================================================
-- REPORTE 3: COST ANALYSIS (Análisis de Costos)
-- Costo por departamento en el último mes
-- =====================================================
CREATE OR REPLACE VIEW vw_cost_analysis_report AS
WITH monthly_costs AS (
    SELECT
        d.id AS department_id,
        d.name AS department_name,
        DATE_TRUNC('month', j.created_at) AS period,
        COUNT(*) AS total_jobs,
        COALESCE(SUM(j.pages), 0) AS total_pages,
        COALESCE(SUM(j.color_pages), 0) AS color_pages,
        COALESCE(SUM(j.pages - j.color_pages), 0) AS black_pages,
        COALESCE(SUM(j.cost), 0) AS total_cost,
        COALESCE(SUM(j.color_cost), 0) AS color_cost,
        COALESCE(SUM(j.cost - j.color_cost), 0) AS black_cost,
        COUNT(DISTINCT j.printer_id) AS printers_used,
        COUNT(DISTINCT j.user_id) AS users_count
    FROM printjobs j
    JOIN departments d ON j.department_id = d.id
    WHERE j.created_at >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
        AND j.created_at < DATE_TRUNC('month', CURRENT_DATE)
    GROUP BY d.id, d.name, DATE_TRUNC('month', j.created_at)
),
previous_month AS (
    SELECT
        d.id AS department_id,
        d.name AS department_name,
        DATE_TRUNC('month', j.created_at) AS period,
        COALESCE(SUM(j.cost), 0) AS previous_cost
    FROM printjobs j
    JOIN departments d ON j.department_id = d.id
    WHERE j.created_at >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '2 months')
        AND j.created_at < DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
    GROUP BY d.id, d.name, DATE_TRUNC('month', j.created_at)
)
SELECT
    mc.department_id,
    mc.department_name,
    mc.period,
    mc.total_jobs,
    mc.total_pages,
    mc.color_pages,
    mc.black_pages,
    mc.total_cost,
    mc.color_cost,
    mc.black_cost,
    CASE WHEN mc.total_pages > 0 THEN ROUND(mc.total_cost / mc.total_pages, 4) ELSE 0 END AS cost_per_page,
    CASE WHEN mc.total_cost > 0 THEN ROUND(mc.total_pages / mc.total_cost, 2) ELSE 0 END AS pages_per_dollar,
    pm.previous_cost,
    CASE
        WHEN pm.previous_cost > 0 THEN
            ROUND(((mc.total_cost - pm.previous_cost) / pm.previous_cost) * 100, 2)
        ELSE 0
    END AS cost_change_percentage,
    CASE
        WHEN mc.pages_per_dollar >= 50 THEN 'Excelente'
        WHEN mc.pages_per_dollar >= 35 THEN 'Bueno'
        WHEN mc.pages_per_dollar >= 20 THEN 'Regular'
        ELSE 'Requiere optimización'
    END AS efficiency_rating,
    mc.printers_used,
    mc.users_count
FROM monthly_costs mc
LEFT JOIN previous_month pm ON mc.department_id = pm.department_id AND mc.period = pm.period + INTERVAL '1 month'
ORDER BY mc.total_cost DESC;

-- =====================================================
-- REPORTE 4: ALERT SUMMARY (Resumen de Alertas)
-- Alertas por severidad en los últimos 30 días
-- =====================================================
CREATE OR REPLACE VIEW vw_alert_summary_report AS
SELECT
    a.severity,
    a.alert_type,
    COUNT(*) AS total_alerts,
    COUNT(CASE WHEN a.resolved_at IS NULL THEN 1 END) AS unresolved_alerts,
    COUNT(CASE WHEN a.resolved_at IS NOT NULL THEN 1 END) AS resolved_alerts,
    AVG(EXTRACT(EPOCH FROM (COALESCE(a.resolved_at, CURRENT_TIMESTAMP) - a.created_at)) / 3600) AS avg_resolution_hours,
    MIN(a.created_at) AS first_occurrence,
    MAX(a.created_at) AS last_occurrence,
    p.name AS printer_name,
    p.model AS printer_model,
    a.probable_cause,
    a.suggested_solution,
    COUNT(DISTINCT a.printer_id) AS affected_printers,
    SUM(COALESCE(a.cost_impact, 0)) AS total_cost_impact
FROM alerts a
LEFT JOIN printers p ON a.printer_id = p.id
WHERE a.created_at >= (CURRENT_DATE - INTERVAL '30 days')
    AND a.is_active = true
GROUP BY
    a.severity,
    a.alert_type,
    p.name,
    p.model,
    a.probable_cause,
    a.suggested_solution
ORDER BY
    CASE a.severity
        WHEN 'Critical' THEN 1
        WHEN 'Error' THEN 2
        WHEN 'Warning' THEN 3
        ELSE 4
    END,
    total_alerts DESC;

-- =====================================================
-- REPORTE 5: POLICY VIOLATIONS (Violaciones de Políticas)
-- Violaciones en los últimos 30 días
-- =====================================================
CREATE OR REPLACE VIEW vw_policy_violations_report AS
SELECT
    pv.policy_id,
    pol.name AS policy_name,
    pol.description AS policy_description,
    pv.violation_type,
    COUNT(*) AS violation_count,
    COUNT(DISTINCT pv.user_id) AS affected_users,
    COUNT(DISTINCT pv.printer_id) AS affected_printers,
    SUM(pv.cost_impact) AS total_cost_impact,
    AVG(pv.over_limit_percentage) AS avg_over_limit_percentage,
    MIN(pv.occurred_at) AS first_violation,
    MAX(pv.occurred_at) AS last_violation,
    u.username AS top_violator,
    d.name AS department_name,
    p.name AS printer_name,
    CASE
        WHEN COUNT(*) >= 10 THEN 'Crítica - Acción inmediata requerida'
        WHEN COUNT(*) >= 5 THEN 'Alta - Revisión urgente'
        WHEN COUNT(*) >= 2 THEN 'Media - Monitoreo continuo'
        ELSE 'Baja - Monitoreo estándar'
    END AS severity_assessment,
    STRING_AGG(DISTINCT pv.corrective_action, '; ') AS corrective_actions_taken
FROM policy_violations pv
JOIN policies pol ON pv.policy_id = pol.id
LEFT JOIN users u ON pv.user_id = u.id
LEFT JOIN departments d ON pv.department_id = d.id
LEFT JOIN printers p ON pv.printer_id = p.id
WHERE pv.occurred_at >= (CURRENT_DATE - INTERVAL '30 days')
GROUP BY
    pv.policy_id,
    pol.name,
    pol.description,
    pv.violation_type,
    u.username,
    d.name,
    p.name
ORDER BY violation_count DESC, total_cost_impact DESC;

-- =====================================================
-- FUNCIONES AUXILIARES
-- =====================================================

-- Función para obtener datos de un reporte específico
CREATE OR REPLACE FUNCTION get_report_data(
    p_report_type TEXT,
    p_tenant_id UUID DEFAULT NULL,
    p_date_from DATE DEFAULT NULL,
    p_date_to DATE DEFAULT NULL,
    p_limit INTEGER DEFAULT 1000
)
RETURNS TABLE (
    data JSONB
) AS $$
BEGIN
    RETURN QUERY
    SELECT CASE
        WHEN p_report_type = 'PrinterUsage' THEN
            (SELECT jsonb_agg(row_to_json(t)) FROM (
                SELECT * FROM vw_printer_usage_report
                WHERE (p_date_from IS NULL OR first_job_date >= p_date_from)
                  AND (p_date_to IS NULL OR last_job_date <= p_date_to)
                LIMIT p_limit
            ) t)
        WHEN p_report_type = 'ConsumableUsage' THEN
            (SELECT jsonb_agg(row_to_json(t)) FROM (
                SELECT * FROM vw_consumable_usage_report
                WHERE (p_date_from IS NULL OR last_updated >= p_date_from)
                  AND (p_date_to IS NULL OR last_updated <= p_date_to)
                LIMIT p_limit
            ) t)
        WHEN p_report_type = 'CostAnalysis' THEN
            (SELECT jsonb_agg(row_to_json(t)) FROM (
                SELECT * FROM vw_cost_analysis_report
                WHERE (p_date_from IS NULL OR period >= p_date_from)
                  AND (p_date_to IS NULL OR period <= p_date_to)
                LIMIT p_limit
            ) t)
        WHEN p_report_type = 'AlertSummary' THEN
            (SELECT jsonb_agg(row_to_json(t)) FROM (
                SELECT * FROM vw_alert_summary_report
                WHERE (p_date_from IS NULL OR first_occurrence >= p_date_from)
                  AND (p_date_to IS NULL OR last_occurrence <= p_date_to)
                LIMIT p_limit
            ) t)
        WHEN p_report_type = 'PolicyViolations' THEN
            (SELECT jsonb_agg(row_to_json(t)) FROM (
                SELECT * FROM vw_policy_violations_report
                WHERE (p_date_from IS NULL OR first_violation >= p_date_from)
                  AND (p_date_to IS NULL OR last_violation <= p_date_to)
                LIMIT p_limit
            ) t)
        ELSE NULL
    END;
END;
$$ LANGUAGE plpgsql;

-- Función para obtener estadísticas de un reporte
CREATE OR REPLACE FUNCTION get_report_statistics(
    p_report_type TEXT,
    p_tenant_id UUID DEFAULT NULL,
    p_date_from DATE DEFAULT NULL,
    p_date_to DATE DEFAULT NULL
)
RETURNS TABLE (
    total_records BIGINT,
    date_range TEXT,
    summary_stats JSONB
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*)::BIGINT,
        CASE
            WHEN p_date_from IS NOT NULL AND p_date_to IS NOT NULL THEN
                p_date_from::TEXT || ' to ' || p_date_to::TEXT
            WHEN p_date_from IS NOT NULL THEN
                'From ' || p_date_from::TEXT
            WHEN p_date_to IS NOT NULL THEN
                'Until ' || p_date_to::TEXT
            ELSE 'All time'
        END,
        CASE p_report_type
            WHEN 'PrinterUsage' THEN jsonb_build_object(
                'total_printers', (SELECT COUNT(*) FROM vw_printer_usage_report WHERE total_pages > 0),
                'total_pages', (SELECT COALESCE(SUM(total_pages), 0) FROM vw_printer_usage_report),
                'avg_pages_per_printer', (SELECT COALESCE(AVG(total_pages), 0) FROM vw_printer_usage_report WHERE total_pages > 0)
            )
            WHEN 'ConsumableUsage' THEN jsonb_build_object(
                'total_consumables', (SELECT COUNT(*) FROM vw_consumable_usage_report),
                'critical_consumables', (SELECT COUNT(*) FROM vw_consumable_usage_report WHERE level_percent <= 5),
                'low_consumables', (SELECT COUNT(*) FROM vw_consumable_usage_report WHERE level_percent <= 15 AND level_percent > 5)
            )
            WHEN 'CostAnalysis' THEN jsonb_build_object(
                'total_cost', (SELECT COALESCE(SUM(total_cost), 0) FROM vw_cost_analysis_report),
                'departments_count', (SELECT COUNT(DISTINCT department_id) FROM vw_cost_analysis_report),
                'avg_cost_per_department', (SELECT COALESCE(AVG(total_cost), 0) FROM vw_cost_analysis_report WHERE total_cost > 0)
            )
            WHEN 'AlertSummary' THEN jsonb_build_object(
                'total_alerts', (SELECT COUNT(*) FROM vw_alert_summary_report),
                'unresolved_alerts', (SELECT SUM(unresolved_alerts) FROM vw_alert_summary_report),
                'critical_alerts', (SELECT COUNT(*) FROM vw_alert_summary_report WHERE severity = 'Critical')
            )
            WHEN 'PolicyViolations' THEN jsonb_build_object(
                'total_violations', (SELECT SUM(violation_count) FROM vw_policy_violations_report),
                'affected_users', (SELECT SUM(affected_users) FROM vw_policy_violations_report),
                'total_cost_impact', (SELECT COALESCE(SUM(total_cost_impact), 0) FROM vw_policy_violations_report)
            )
            ELSE jsonb_build_object('message', 'No statistics available')
        END
    FROM (
        SELECT 1 as dummy
    ) dummy_table
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- ÍNDICES DE RENDIMIENTO
-- =====================================================

-- Índices para optimizar las consultas de reportes
CREATE INDEX IF NOT EXISTS idx_printjobs_created_at ON printjobs(created_at);
CREATE INDEX IF NOT EXISTS idx_printjobs_printer_id ON printjobs(printer_id);
CREATE INDEX IF NOT EXISTS idx_printjobs_department_id ON printjobs(department_id);
CREATE INDEX IF NOT EXISTS idx_alerts_created_at ON alerts(created_at);
CREATE INDEX IF NOT EXISTS idx_alerts_severity ON alerts(severity);
CREATE INDEX IF NOT EXISTS idx_policy_violations_occurred_at ON policy_violations(occurred_at);
CREATE INDEX IF NOT EXISTS idx_printer_consumable_parts_level ON printer_consumable_parts(level_percent);

-- Índice compuesto para búsquedas por período
CREATE INDEX IF NOT EXISTS idx_printjobs_date_range ON printjobs(created_at, department_id, printer_id);
CREATE INDEX IF NOT EXISTS idx_alerts_date_severity ON alerts(created_at, severity, resolved_at);
