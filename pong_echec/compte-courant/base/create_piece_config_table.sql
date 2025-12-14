-- Script de création de la table piece_config pour PostgreSQL
-- Base de données: banque
CREATE TABLE
    IF NOT EXISTS piece_config (
        id BIGSERIAL PRIMARY KEY,
        type_piece VARCHAR(50) NOT NULL UNIQUE,
        vie INTEGER NOT NULL
    );

-- Insertion des données par défaut
INSERT INTO
    piece_config (type_piece, vie)
VALUES
    ('Pion', 0),
    ('Tour', 1),
    ('Cavalier', 1),
    ('Fou', 1),
    ('Reine', 1),
    ('Roi', 1) ON CONFLICT (type_piece) DO NOTHING;

-- Vérification des données
SELECT
    *
FROM
    piece_config
ORDER BY
    type_piece;

CREATE TABLE
    game_state (
        id SERIAL PRIMARY KEY,
        -- Configuration
        nombre_pieces INT NOT NULL,
        -- Balle
        ball_x INT NOT NULL,
        ball_y INT NOT NULL,
        ball_speed_x INT NOT NULL,
        ball_speed_y INT NOT NULL,
        balle_lancee BOOLEAN NOT NULL,
        -- Raquettes
        raquette1_x INT NOT NULL,
        raquette1_y INT NOT NULL,
        raquette2_x INT NOT NULL,
        raquette2_y INT NOT NULL,
        -- Vies des pièces (JSONB)
        vies_pieces JSONB NOT NULL
    );

INSERT INTO
    game_state (
        nombre_pieces,
        ball_x,
        ball_y,
        ball_speed_x,
        ball_speed_y,
        balle_lancee,
        raquette1_x,
        raquette1_y,
        raquette2_x,
        raquette2_y,
        vies_pieces
    )
VALUES
    (
        6,
        100,
        100,
        0,
        0,
        false,
        400,
        50,
        400,
        850,
        '{
        "Roi": 5,
        "Dame": 4,
        "Tour": 3,
        "Fou": 3,
        "Cavalier": 2,
        "Pion": 0
        }'
    );