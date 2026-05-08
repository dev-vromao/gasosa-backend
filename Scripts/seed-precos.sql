-- ============================================================
-- Script: seed-precos.sql
-- Popula preços de combustível para um posto específico,
-- espalhando datas nos últimos 14 dias para dar histórico
-- ao gráfico e à calculadora.
--
-- USO:
--   psql -h localhost -p 5432 -U postgres -d gasosa \
--        -v posto_id=1 -f Scripts/seed-precos.sql
--
-- Troque "posto_id=1" pelo Id do posto desejado.
-- Para descobrir os Ids existentes:
--   SELECT id, nome FROM postos;
-- ============================================================

\set posto_id_int :posto_id

-- Limpa preços anteriores deste posto (idempotente: pode rodar várias vezes)
DELETE FROM precos_combustiveis WHERE posto_id = :posto_id_int;

-- ------------------------------------------------------------
-- Gasolina comum  — tendência de alta nas últimas semanas
-- ------------------------------------------------------------
INSERT INTO precos_combustiveis (posto_id, tipo_combustivel, preco, data_cadastro) VALUES
  (:posto_id_int, 'Gasolina comum', 5.79, NOW() - INTERVAL '14 days'),
  (:posto_id_int, 'Gasolina comum', 5.85, NOW() - INTERVAL '10 days'),
  (:posto_id_int, 'Gasolina comum', 5.89, NOW() - INTERVAL '7 days'),
  (:posto_id_int, 'Gasolina comum', 5.95, NOW() - INTERVAL '3 days'),
  (:posto_id_int, 'Gasolina comum', 5.99, NOW() - INTERVAL '1 day'),
  (:posto_id_int, 'Gasolina comum', 5.99, NOW());

-- ------------------------------------------------------------
-- Gasolina aditivada
-- ------------------------------------------------------------
INSERT INTO precos_combustiveis (posto_id, tipo_combustivel, preco, data_cadastro) VALUES
  (:posto_id_int, 'Gasolina aditivada', 5.99, NOW() - INTERVAL '14 days'),
  (:posto_id_int, 'Gasolina aditivada', 6.05, NOW() - INTERVAL '10 days'),
  (:posto_id_int, 'Gasolina aditivada', 6.09, NOW() - INTERVAL '7 days'),
  (:posto_id_int, 'Gasolina aditivada', 6.15, NOW() - INTERVAL '3 days'),
  (:posto_id_int, 'Gasolina aditivada', 6.19, NOW());

-- ------------------------------------------------------------
-- Etanol  — preços que dão razão ~66% (Etanol VALE A PENA)
-- ------------------------------------------------------------
INSERT INTO precos_combustiveis (posto_id, tipo_combustivel, preco, data_cadastro) VALUES
  (:posto_id_int, 'Etanol', 3.79, NOW() - INTERVAL '14 days'),
  (:posto_id_int, 'Etanol', 3.85, NOW() - INTERVAL '10 days'),
  (:posto_id_int, 'Etanol', 3.89, NOW() - INTERVAL '7 days'),
  (:posto_id_int, 'Etanol', 3.95, NOW() - INTERVAL '3 days'),
  (:posto_id_int, 'Etanol', 3.99, NOW());

-- ------------------------------------------------------------
-- Diesel S10  — só pra testar o gráfico (não entra na calc)
-- ------------------------------------------------------------
INSERT INTO precos_combustiveis (posto_id, tipo_combustivel, preco, data_cadastro) VALUES
  (:posto_id_int, 'Diesel S10', 5.49, NOW() - INTERVAL '14 days'),
  (:posto_id_int, 'Diesel S10', 5.55, NOW() - INTERVAL '7 days'),
  (:posto_id_int, 'Diesel S10', 5.59, NOW());

-- ------------------------------------------------------------
-- Verificação: resumo do que foi inserido
-- ------------------------------------------------------------
SELECT
  tipo_combustivel,
  COUNT(*) AS qtd_registros,
  MIN(preco) AS preco_min,
  MAX(preco) AS preco_max,
  ROUND(AVG(preco)::numeric, 2) AS preco_medio
FROM precos_combustiveis
WHERE posto_id = :posto_id_int
GROUP BY tipo_combustivel
ORDER BY tipo_combustivel;
