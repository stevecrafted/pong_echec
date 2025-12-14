package com.banque.compte.rest;

import com.banque.compte.dao.PieceConfigEJB;
import jakarta.ejb.EJB;
import jakarta.ws.rs.GET;
import jakarta.ws.rs.Path;
import jakarta.ws.rs.Produces;
import jakarta.ws.rs.core.MediaType;
import jakarta.ws.rs.core.Response;
import java.util.Map;
import java.util.logging.Logger;

@Path("/config")
public class PieceConfigResource {

    private static final Logger LOGGER = Logger.getLogger(PieceConfigResource.class.getName());

    @EJB
    private PieceConfigEJB configEJB;

    /**
     * Endpoint de test pour vérifier que l'API fonctionne
     * GET /api/config/test
     */
    @GET
    @Path("/test")
    @Produces(MediaType.TEXT_PLAIN)
    public String test() {
        LOGGER.info("→ Endpoint /test appelé");
        return "API fonctionnelle ! Timestamp: " + System.currentTimeMillis();
    }

    /**
     * Récupère la configuration des pièces
     * GET /api/config
     */
    @GET
    @Produces(MediaType.APPLICATION_JSON)
    public Response getConfig() {
        try {
            LOGGER.info("→ Endpoint /config appelé");
            
            if (configEJB == null) {
                LOGGER.severe("✗ EJB non injecté !");
                return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                        .entity("{\"error\": \"EJB non disponible\"}")
                        .build();
            }
            
            Map<String, Integer> config = configEJB.getConfig();
            LOGGER.info("✓ Configuration retournée: " + config.size() + " éléments");
            
            return Response.ok(config).build();
            
        } catch (Exception e) {
            LOGGER.severe("✗ Erreur lors de la récupération de la config: " + e.getMessage());
            e.printStackTrace();
            
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\": \"" + e.getMessage() + "\"}")
                    .build();
        }
    }

    /**
     * Endpoint pour vérifier le statut de l'EJB
     * GET /api/config/status
     */
    @GET
    @Path("/status")
    @Produces(MediaType.APPLICATION_JSON)
    public Response getStatus() {
        LOGGER.info("→ Endpoint /status appelé");
        
        Map<String, Object> status = Map.of(
            "ejbInjected", configEJB != null,
            "timestamp", System.currentTimeMillis(),
            "status", "OK"
        );
        
        return Response.ok(status).build();
    }

    /**
     * Récupère toutes les données statiques du jeu
     * GET /api/config/gamedata
     */
    @GET
    @Path("/gamedata")
    @Produces(MediaType.APPLICATION_JSON)
    public Response getGameData() {
        try {
            LOGGER.info("→ Endpoint /gamedata appelé");
            
            if (configEJB == null) {
                LOGGER.severe("✗ EJB non injecté !");
                return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                        .entity("{\"error\": \"EJB non disponible\"}")
                        .build();
            }
            
            var gameData = configEJB.getGameData();
            
            if (gameData == null) {
                LOGGER.severe("✗ Impossible de récupérer les données de jeu");
                return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                        .entity("{\"error\": \"Données de jeu non disponibles\"}")
                        .build();
            }
            
            LOGGER.info("✓ Données de jeu retournées avec succès");
            return Response.ok(gameData).build();
            
        } catch (Exception e) {
            LOGGER.severe("✗ Erreur lors de la récupération des données de jeu: " + e.getMessage());
            e.printStackTrace();
            
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR)
                    .entity("{\"error\": \"" + e.getMessage() + "\"}")
                    .build();
        }
    }
}
