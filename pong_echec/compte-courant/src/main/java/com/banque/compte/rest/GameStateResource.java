package com.banque.compte.rest;

import com.banque.compte.dao.GameStateEJB;
import com.banque.compte.dao.GameStateEntity;

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

            GameStateEntity state = gameStateEJB.getGameStateEntity();
            LOGGER.info("✓ État retourné: " + state.getNombrePieces() + " pièces");

            return Response.ok(state).build();

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
    public Response saveGameState(GameStateEntity dto) {
        try {
            LOGGER.info("💾 Sauvegarde état reçue depuis le client");

            if (gameStateEJB == null) {
                return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                        .entity("{\"error\":\"GameStateEJB non injecté\"}")
                        .build();
            }
 
            // Sauvegarde (mémoire + BDD)
            gameStateEJB.updateGameStateEntity(dto);

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

}