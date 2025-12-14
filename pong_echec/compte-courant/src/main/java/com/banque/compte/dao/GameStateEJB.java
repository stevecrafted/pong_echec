package com.banque.compte.dao;

import jakarta.ejb.Singleton;
import jakarta.ejb.Startup;
import jakarta.annotation.PostConstruct;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;

import java.util.HashMap; 
import java.util.logging.Logger;

// import com.banque.compte.entity.GameStateEntity;

@Singleton
@Startup
public class GameStateEJB {

    private static final Logger LOGGER = Logger.getLogger(GameStateEJB.class.getName());

    @PersistenceContext(unitName = "banquePU")
    private EntityManager em;

    // État du jeu en mémoire (envoyé aux clients)
    private GameStateEntity currentState;

    // ==========================
    // INIT
    // ==========================
    @PostConstruct
    public void init() {
        LOGGER.info("→ Initialisation GameStateEntityEJB (chargement BDD)");

        GameStateEntity entity = getFirstStateFromDB();

        if (entity == null) {
            LOGGER.warning("⚠ Aucun GameStateEntity en BDD, création état par défaut");
        } else {
            currentState = entityToGameStateEntity(entity);
            LOGGER.info("✓ GameStateEntity chargé depuis la BDD");
        }
    }

    // ==========================
    // GET / UPDATE (API EXISTANTE)
    // ==========================
    public GameStateEntity getGameStateEntity() {
        return currentState;
    }

    public void updateGameStateEntity(GameStateEntity newState) {
        this.currentState = newState;
        saveToDatabase();
    }

    public void updateBallPosition(int x, int y, int speedX, int speedY) {
        currentState.setBallX(x);
        currentState.setBallY(y);
        currentState.setBallSpeedX(speedX);
        currentState.setBallSpeedY(speedY);
        saveToDatabase();
    }

    public void updateRaquettePosition(int joueur, int x, int y) {
        if (joueur == 1) {
            currentState.setRaquette1X(x);
            currentState.setRaquette1Y(y);
        } else if (joueur == 2) {
            currentState.setRaquette2X(x);
            currentState.setRaquette2Y(y);
        }
        saveToDatabase();
    }

    public void lancerBalle(int speedX, int speedY) {
        currentState.setBalleLancee(true);
        currentState.setBallSpeedX(speedX);
        currentState.setBallSpeedY(speedY);
        saveToDatabase();
    }

    public void resetGame(int nombrePieces) {
        currentState = createDefaultState();
        currentState.setNombrePieces(nombrePieces);
        saveToDatabase();
    }

    // ==========================
    // BDD
    // ==========================
    private GameStateEntity getFirstStateFromDB() {
        return em.createQuery(
                "SELECT g FROM GameStateEntity g ORDER BY g.id ASC",
                GameStateEntity.class).setMaxResults(1)
                .getResultStream()
                .findFirst()
                .orElse(null);
    }

    private void saveToDatabase() {
        GameStateEntity entity = getFirstStateFromDB();

        if (entity == null) {
            entity = new GameStateEntity();
            gameStateEGameStateEntityToEntity(currentState, entity);
            em.persist(entity);
        } else {
            gameStateEGameStateEntityToEntity(currentState, entity);
            em.merge(entity);
        }
    }

    // ==========================
    // MAPPING
    // ==========================
    private GameStateEntity entityToGameStateEntity(GameStateEntity e) {
        GameStateEntity s = new GameStateEntity();

        s.setNombrePieces(e.getNombrePieces());
        s.setBallX(e.getBallX());
        s.setBallY(e.getBallY());
        s.setBallSpeedX(e.getBallSpeedX());
        s.setBallSpeedY(e.getBallSpeedY());
        s.setBalleLancee(e.isBalleLancee());

        s.setRaquette1X(e.getRaquette1X());
        s.setRaquette1Y(e.getRaquette1Y());
        s.setRaquette2X(e.getRaquette2X());
        s.setRaquette2Y(e.getRaquette2Y());

        s.setViesPiecesJoueur1(e.getViesPiecesJoueur1());
        s.setViesPiecesJoueur2(e.getViesPiecesJoueur2());

        return s;
    }

    private void gameStateEGameStateEntityToEntity(GameStateEntity s, GameStateEntity e) {
        e.setNombrePieces(s.getNombrePieces());
        e.setBallX(s.getBallX());
        e.setBallY(s.getBallY());
        e.setBallSpeedX(s.getBallSpeedX());
        e.setBallSpeedY(s.getBallSpeedY());
        e.setBalleLancee(s.isBalleLancee());

        e.setRaquette1X(s.getRaquette1X());
        e.setRaquette1Y(s.getRaquette1Y());
        e.setRaquette2X(s.getRaquette2X());
        e.setRaquette2Y(s.getRaquette2Y());

        e.setViesPiecesJoueur1(s.getViesPiecesJoueur1());
        e.setViesPiecesJoueur2(s.getViesPiecesJoueur2());
    }

    // ==========================
    // ÉTAT PAR DÉFAUT
    // ==========================
    private GameStateEntity createDefaultState() {
        GameStateEntity s = new GameStateEntity();

        s.setNombrePieces(6);
        s.setBallX(400);
        s.setBallY(300);
        s.setBallSpeedX(0);
        s.setBallSpeedY(0);
        s.setBalleLancee(false);

        s.setRaquette1X(400);
        s.setRaquette1Y(50);
        s.setRaquette2X(400);
        s.setRaquette2Y(850);

        // Vies des pièces pour le joueur 1
        var viesJoueur1 = new HashMap<String, Integer>();
        viesJoueur1.put("Roi", 5);
        viesJoueur1.put("Dame", 4);
        viesJoueur1.put("Tour", 3);
        viesJoueur1.put("Fou", 3);
        viesJoueur1.put("Cavalier", 2);
        viesJoueur1.put("Pion", 1);

        // Vies des pièces pour le joueur 2
        var viesJoueur2 = new HashMap<String, Integer>();
        viesJoueur2.put("Roi", 5);
        viesJoueur2.put("Dame", 4);
        viesJoueur2.put("Tour", 3);
        viesJoueur2.put("Fou", 3);
        viesJoueur2.put("Cavalier", 2);
        viesJoueur2.put("Pion", 1);

        s.setViesPiecesJoueur1(viesJoueur1);
        s.setViesPiecesJoueur2(viesJoueur2);
        return s;
    }

    
}
