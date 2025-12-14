-- Script pour convertir la colonne vies_pieces de JSONB à TEXT
-- À exécuter dans PostgreSQL

-- Convertir la colonne existante
ALTER TABLE game_state 
ALTER COLUMN vies_pieces TYPE TEXT USING vies_pieces::TEXT;

-- Vérifier le changement
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'game_state' AND column_name = 'vies_pieces';

-- Afficher les données pour vérifier
SELECT * FROM game_state;
