-- Migration: Séparer les vies des pièces par joueur
-- Date: $(date)

-- Ajouter les nouvelles colonnes pour chaque joueur
ALTER TABLE game_state 
ADD COLUMN vies_pieces_joueur1 TEXT,
ADD COLUMN vies_pieces_joueur2 TEXT;

-- Copier les données existantes dans les deux nouvelles colonnes
UPDATE game_state 
SET vies_pieces_joueur1 = vies_pieces,
    vies_pieces_joueur2 = vies_pieces
WHERE vies_pieces IS NOT NULL;

-- Optionnel: Supprimer l'ancienne colonne (décommenter si nécessaire)
-- ALTER TABLE game_state DROP COLUMN vies_pieces;

-- Vérifier les résultats
SELECT id, vies_pieces_joueur1, vies_pieces_joueur2 FROM game_state;
