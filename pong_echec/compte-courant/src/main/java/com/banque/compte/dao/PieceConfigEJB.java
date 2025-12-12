package com.banque.compte.dao;

import com.banque.compte.entity.PieceConfig;
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

}
