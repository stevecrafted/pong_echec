package com.banque.compte.dao;

import com.banque.compte.entity.PieceConfig;
import com.banque.compte.entity.GameDataConfig;
import jakarta.annotation.PostConstruct;
import jakarta.ejb.Stateless;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import jakarta.persistence.TypedQuery;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.logging.Logger;

@Stateless
public class PieceConfigEJB {

    private static final Logger LOGGER = Logger.getLogger(PieceConfigEJB.class.getName());
    
    @PersistenceContext(unitName = "banquePU")
    private EntityManager em;

    @PostConstruct
    public void init() {
        LOGGER.info("✓ PieceConfigEJB déployé avec succès");
        initializeDefaultData();
    }

    private void initializeDefaultData() {
        try {
            long count = em.createQuery("SELECT COUNT(p) FROM PieceConfig p", Long.class).getSingleResult();
            
            if (count == 0) {
                LOGGER.info("→ Initialisation des données par défaut");
                
                em.persist(new PieceConfig("Pion", 0));
                em.persist(new PieceConfig("Tour", 1));
                em.persist(new PieceConfig("Cavalier", 1));
                em.persist(new PieceConfig("Fou", 1));
                em.persist(new PieceConfig("Reine", 1));
                em.persist(new PieceConfig("Roi", 1));
                
                LOGGER.info("✓ Données par défaut initialisées");
            } else {
                LOGGER.info("✓ Données déjà présentes en base (" + count + " configurations)");
            }
        } catch (Exception e) {
            LOGGER.severe("✗ Erreur lors de l'initialisation: " + e.getMessage());
        }
    }

    public Map<String, Integer> getConfig() {
        LOGGER.info("→ getConfig() appelé - récupération depuis la base");
        
        try {
            TypedQuery<PieceConfig> query = em.createQuery("SELECT p FROM PieceConfig p", PieceConfig.class);
            List<PieceConfig> configs = query.getResultList();
            
            Map<String, Integer> result = new HashMap<>();
            for (PieceConfig config : configs) {
                result.put(config.getTypePiece(), config.getVie());
            }
            
            LOGGER.info("✓ Configuration récupérée: " + result.size() + " éléments");
            return result;
            
        } catch (Exception e) {
            LOGGER.severe("✗ Erreur lors de la récupération: " + e.getMessage());
            return new HashMap<>();
        }
    }

    /**
     * Récupère toutes les données statiques du jeu (pièces, balle, raquettes, terrain)
     */
    public GameDataConfig getGameData() {
        LOGGER.info("→ getGameData() appelé - récupération de toutes les données statiques");
        
        try {
            GameDataConfig gameData = new GameDataConfig();
            
            // 1. Configuration des pièces
            gameData.setPieceConfig(getConfig());
            
            // 2. Configuration de la balle (valeurs par défaut)
            GameDataConfig.BallInitialConfig ballConfig = new GameDataConfig.BallInitialConfig(
                400,  // posX
                300,  // posY
                5,    // speedX
                5,    // speedY
                10    // radius
            );
            gameData.setBallConfig(ballConfig);
            
            // 3. Configuration des raquettes (positions initiales)
            GameDataConfig.RaquetteInitialConfig raquette1 = new GameDataConfig.RaquetteInitialConfig(
                1,      // joueurId
                400.0f, // posX (centre horizontal)
                50.0f   // posY (haut du terrain)
            );
            gameData.setRaquette1Config(raquette1);
            
            GameDataConfig.RaquetteInitialConfig raquette2 = new GameDataConfig.RaquetteInitialConfig(
                2,      // joueurId
                400.0f, // posX (centre horizontal)
                850.0f  // posY (bas du terrain)
            );
            gameData.setRaquette2Config(raquette2);
            
            // 4. Configuration du terrain
            GameDataConfig.TerrainConfig terrainConfig = new GameDataConfig.TerrainConfig(
                900,  // width
                900   // height
            );
            gameData.setTerrainConfig(terrainConfig);
            
            LOGGER.info("✓ GameData complet retourné avec succès");
            return gameData;
            
        } catch (Exception e) {
            LOGGER.severe("✗ Erreur lors de la récupération des données de jeu: " + e.getMessage());
            e.printStackTrace();
            return null;
        }
    }

}
