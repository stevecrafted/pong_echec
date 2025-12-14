package com.banque.compte.dao;

import jakarta.ejb.Singleton;
import jakarta.ejb.Startup;
import jakarta.annotation.PostConstruct;
import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;

import java.util.HashMap;
import java.util.Map;
import java.util.logging.Logger;

@Singleton
@Startup
public class GameStateEJB {

    private static final Logger LOGGER = Logger.getLogger(GameStateEJB.class.getName());

    @PersistenceContext(unitName = "banquePU")
    private EntityManager em;

    // État du jeu en mémoire (envoyé aux clients)
    private GameState currentState;

    // ==========================
    // INIT
    // ==========================
    @PostConstruct
    public void init() {
        LOGGER.info("→ Initialisation GameStateEJB (chargement BDD)");

        GameStateEntity entity = getFirstStateFromDB();

        if (entity == null) {
            LOGGER.warning("⚠ Aucun GameState en BDD, création état par défaut");
            currentState = createDefaultState();
            saveToDatabase();
        } else {
            currentState = entityToGameState(entity);
            LOGGER.info("✓ GameState chargé depuis la BDD");
        }
    }

    // ==========================
    // GET / UPDATE (API EXISTANTE)
    // ==========================
    public GameState getGameState() {
        return currentState;
    }

    public void updateGameState(GameState newState) {
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
            gameStateToEntity(currentState, entity);
            em.persist(entity);
        } else {
            gameStateToEntity(currentState, entity);
            em.merge(entity);
        }
    }

    // ==========================
    // MAPPING
    // ==========================
    private GameState entityToGameState(GameStateEntity e) {
        GameState s = new GameState();

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

        s.setViesPieces(e.getViesPieces());

        return s;
    }

    private void gameStateToEntity(GameState s, GameStateEntity e) {
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

        e.setViesPieces(s.getViesPieces());
    }

    // ==========================
    // ÉTAT PAR DÉFAUT
    // ==========================
    private GameState createDefaultState() {
        GameState s = new GameState();

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

        var vies = new HashMap<String, Integer>();
        vies.put("Roi", 5);
        vies.put("Dame", 4);
        vies.put("Tour", 3);
        vies.put("Fou", 3);
        vies.put("Cavalier", 2);
        vies.put("Pion", 1);

        s.setViesPieces(vies);
        return s;
    }

    // ==========================
    // CLASSE INTERNE (INCHANGÉE)
    // ==========================
    public static class GameState {
        private int nombrePieces;
        private int ballX;
        private int ballY;
        private int ballSpeedX;
        private int ballSpeedY;
        private boolean balleLancee;
        private int raquette1X;
        private int raquette1Y;
        private int raquette2X;
        private int raquette2Y;
        private java.util.Map<String, Integer> viesPieces;

        public Map<String, Integer> getViesPiecess() {
            return viesPieces;
        }

        public void setViesPiecess(Map<String, Integer> vie) {
            viesPieces = vie;
        }

        // Getters et Setters
        public int getNombrePieces() {
            return nombrePieces;
        }

        public void setNombrePieces(int nombrePieces) {
            this.nombrePieces = nombrePieces;
        }

        public int getBallX() {
            return ballX;
        }

        public void setBallX(int ballX) {
            this.ballX = ballX;
        }

        public int getBallY() {
            return ballY;
        }

        public void setBallY(int ballY) {
            this.ballY = ballY;
        }

        public int getBallSpeedX() {
            return ballSpeedX;
        }

        public void setBallSpeedX(int ballSpeedX) {
            this.ballSpeedX = ballSpeedX;
        }

        public int getBallSpeedY() {
            return ballSpeedY;
        }

        public void setBallSpeedY(int ballSpeedY) {
            this.ballSpeedY = ballSpeedY;
        }

        public boolean isBalleLancee() {
            return balleLancee;
        }

        public void setBalleLancee(boolean balleLancee) {
            this.balleLancee = balleLancee;
        }

        public int getRaquette1X() {
            return raquette1X;
        }

        public void setRaquette1X(int raquette1X) {
            this.raquette1X = raquette1X;
        }

        public int getRaquette1Y() {
            return raquette1Y;
        }

        public void setRaquette1Y(int raquette1Y) {
            this.raquette1Y = raquette1Y;
        }

        public int getRaquette2X() {
            return raquette2X;
        }

        public void setRaquette2X(int raquette2X) {
            this.raquette2X = raquette2X;
        }

        public int getRaquette2Y() {
            return raquette2Y;
        }

        public void setRaquette2Y(int raquette2Y) {
            this.raquette2Y = raquette2Y;
        }

        public java.util.Map<String, Integer> getViesPieces() {
            return viesPieces;
        }

        public void setViesPieces(java.util.Map<String, Integer> viesPieces) {
            this.viesPieces = viesPieces;
        }
         
    }
}
