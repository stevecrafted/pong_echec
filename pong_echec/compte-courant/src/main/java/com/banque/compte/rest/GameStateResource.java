package com.banque.compte.rest;

import com.banque.compte.dao.GameStateEJB;
import com.banque.compte.dao.GameStateEJB.GameState;
import com.banque.compte.dto.GameStateDTO;

import jakarta.ejb.EJB;
import jakarta.ws.rs.*;
import jakarta.ws.rs.core.MediaType;
import jakarta.ws.rs.core.Response;
import java.util.logging.Logger;

/**
 * API REST pour la gestion de l'état du jeu
 */
@Path("/game")
public class GameStateResource {

    private static final Logger LOGGER = Logger.getLogger(GameStateResource.class.getName());

    @EJB
    private GameStateEJB gameStateEJB;

    /**
     * Récupère l'état actuel du jeu
     * GET /api/game/state
     */
    @GET
    @Path("/state")
    @Produces(MediaType.APPLICATION_JSON)
    public Response getGameState() {
        try {
            LOGGER.info("→ GET /game/state");

            if (gameStateEJB == null) {
                LOGGER.severe("✗ EJB non injecté !");
                return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                        .entity("{\"error\": \"EJB non disponible\"}")
                        .build();
            }

            GameState state = gameStateEJB.getGameState();
            LOGGER.info("✓ État retourné: " + state.getNombrePieces() + " pièces");

            return Response.ok(state).build();

        } catch (Exception e) {
            LOGGER.severe("✗ Erreur: " + e.getMessage());
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\": \"" + e.getMessage() + "\"}")
                    .build();
        }
    }

    /**
     * Met à jour l'état complet du jeu
     * PUT /api/game/state
     */
    @PUT
    @Path("/state")
    @Consumes(MediaType.APPLICATION_JSON)
    @Produces(MediaType.APPLICATION_JSON)
    public Response updateGameState(GameState newState) {
        try {
            LOGGER.info("→ PUT /game/state");

            if (newState == null) {
                return Response.status(Response.Status.BAD_REQUEST)
                        .entity("{\"error\": \"État du jeu manquant\"}")
                        .build();
            }

            gameStateEJB.updateGameState(newState);
            LOGGER.info("✓ État mis à jour");

            return Response.ok("{\"message\": \"État mis à jour avec succès\"}").build();

        } catch (Exception e) {
            LOGGER.severe("✗ Erreur: " + e.getMessage());
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\": \"" + e.getMessage() + "\"}")
                    .build();
        }
    }

    /**
     * Réinitialise le jeu
     * POST /api/game/reset?pieces=4
     */
    @POST
    @Path("/reset")
    @Produces(MediaType.APPLICATION_JSON)
    public Response resetGame(@QueryParam("pieces") @DefaultValue("4") int nombrePieces) {
        try {
            LOGGER.info("→ POST /game/reset?pieces=" + nombrePieces);

            if (nombrePieces < 1 || nombrePieces > 8) {
                return Response.status(Response.Status.BAD_REQUEST)
                        .entity("{\"error\": \"Le nombre de pièces doit être entre 1 et 8\"}")
                        .build();
            }

            gameStateEJB.resetGame(nombrePieces);
            LOGGER.info("✓ Jeu réinitialisé");

            return Response.ok("{\"message\": \"Jeu réinitialisé avec " + nombrePieces + " pièces\"}").build();

        } catch (Exception e) {
            LOGGER.severe("✗ Erreur: " + e.getMessage());
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\": \"" + e.getMessage() + "\"}")
                    .build();
        }
    }

    /**
     * Met à jour la position de la balle
     * PUT /api/game/ball
     */
    @PUT
    @Path("/ball")
    @Consumes(MediaType.APPLICATION_JSON)
    @Produces(MediaType.APPLICATION_JSON)
    public Response updateBallPosition(BallUpdate update) {
        try {
            LOGGER.info("→ PUT /game/ball");

            gameStateEJB.updateBallPosition(
                    update.getX(),
                    update.getY(),
                    update.getSpeedX(),
                    update.getSpeedY());

            return Response.ok("{\"message\": \"Position balle mise à jour\"}").build();

        } catch (Exception e) {
            LOGGER.severe("✗ Erreur: " + e.getMessage());
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\": \"" + e.getMessage() + "\"}")
                    .build();
        }
    }

    /**
     * Met à jour la position d'une raquette
     * PUT /api/game/raquette/{joueur}
     */
    @PUT
    @Path("/raquette/{joueur}")
    @Consumes(MediaType.APPLICATION_JSON)
    @Produces(MediaType.APPLICATION_JSON)
    public Response updateRaquettePosition(
            @PathParam("joueur") int joueur,
            RaquetteUpdate update) {
        try {
            LOGGER.info("→ PUT /game/raquette/" + joueur);

            if (joueur != 1 && joueur != 2) {
                return Response.status(Response.Status.BAD_REQUEST)
                        .entity("{\"error\": \"Le joueur doit être 1 ou 2\"}")
                        .build();
            }

            gameStateEJB.updateRaquettePosition(joueur, update.getX(), update.getY());

            return Response.ok("{\"message\": \"Position raquette mise à jour\"}").build();

        } catch (Exception e) {
            LOGGER.severe("✗ Erreur: " + e.getMessage());
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\": \"" + e.getMessage() + "\"}")
                    .build();
        }
    }

    /**
     * Lance la balle
     * POST /api/game/launch
     */
    @POST
    @Path("/launch")
    @Consumes(MediaType.APPLICATION_JSON)
    @Produces(MediaType.APPLICATION_JSON)
    public Response lancerBalle(LaunchBall launch) {
        try {
            LOGGER.info("→ POST /game/launch");

            gameStateEJB.lancerBalle(launch.getSpeedX(), launch.getSpeedY());

            return Response.ok("{\"message\": \"Balle lancée\"}").build();

        } catch (Exception e) {
            LOGGER.severe("✗ Erreur: " + e.getMessage());
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\": \"" + e.getMessage() + "\"}")
                    .build();
        }
    }

    @PUT 
    @Path("/save")
    @Consumes(MediaType.APPLICATION_JSON)
    @Produces(MediaType.APPLICATION_JSON)
    public Response saveGameState(GameStateDTO dto) {
        try {
            LOGGER.info("💾 Sauvegarde état reçue depuis le client");

            if (gameStateEJB == null) {
                return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                        .entity("{\"error\":\"GameStateEJB non injecté\"}")
                        .build();
            }

            // DTO -> GameState
            GameStateEJB.GameState state = dtoToGameState(dto);

            // Sauvegarde (mémoire + BDD)
            gameStateEJB.updateGameState(state);

            return Response.ok()
                    .entity("{\"status\":\"saved\"}")
                    .build();

        } catch (Exception e) {
            e.printStackTrace();
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\":\"" + e.getMessage() + "\"}")
                    .build();
        }
    }

    // =========================
    // MAPPING
    // =========================
    private GameStateEJB.GameState dtoToGameState(GameStateDTO d) {
        GameStateEJB.GameState s = new GameStateEJB.GameState();

        s.setNombrePieces(d.nombrePieces);
        s.setBallX(d.ballX);
        s.setBallY(d.ballY);
        s.setBallSpeedX(d.ballSpeedX);
        s.setBallSpeedY(d.ballSpeedY);
        s.setBalleLancee(d.balleLancee);

        s.setRaquette1X(d.raquette1X);
        s.setRaquette1Y(d.raquette1Y);
        s.setRaquette2X(d.raquette2X);
        s.setRaquette2Y(d.raquette2Y);

        s.setViesPiecess(d.viesPieces);

        return s;
    }

    // ===== Classes pour les requêtes JSON =====

    public static class BallUpdate {
        private int x;
        private int y;
        private int speedX;
        private int speedY;

        public int getX() {
            return x;
        }

        public void setX(int x) {
            this.x = x;
        }

        public int getY() {
            return y;
        }

        public void setY(int y) {
            this.y = y;
        }

        public int getSpeedX() {
            return speedX;
        }

        public void setSpeedX(int speedX) {
            this.speedX = speedX;
        }

        public int getSpeedY() {
            return speedY;
        }

        public void setSpeedY(int speedY) {
            this.speedY = speedY;
        }
    }

    public static class RaquetteUpdate {
        private int x;
        private int y;

        public int getX() {
            return x;
        }

        public void setX(int x) {
            this.x = x;
        }

        public int getY() {
            return y;
        }

        public void setY(int y) {
            this.y = y;
        }
    }

    public static class LaunchBall {
        private int speedX;
        private int speedY;

        public int getSpeedX() {
            return speedX;
        }

        public void setSpeedX(int speedX) {
            this.speedX = speedX;
        }

        public int getSpeedY() {
            return speedY;
        }

        public void setSpeedY(int speedY) {
            this.speedY = speedY;
        }
    }

}