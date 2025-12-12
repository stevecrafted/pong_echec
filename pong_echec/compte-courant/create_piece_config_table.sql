-- Script de création de la table piece_config pour PostgreSQL
-- Base de données: banque

CREATE TABLE IF NOT EXISTS piece_config (
    id BIGSERIAL PRIMARY KEY,
    type_piece VARCHAR(50) NOT NULL UNIQUE,
    vie INTEGER NOT NULL
);

-- Insertion des données par défaut
INSERT INTO piece_config (type_piece, vie) VALUES 
    ('Pion', 0),
    ('Tour', 1),
    ('Cavalier', 1),
    ('Fou', 1),
    ('Reine', 1),
    ('Roi', 1)
ON CONFLICT (type_piece) DO NOTHING;

-- Vérification des données
SELECT * FROM piece_config ORDER BY type_piece;