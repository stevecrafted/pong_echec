package com.banque.compte.rest;

import jakarta.ws.rs.ApplicationPath;
import jakarta.ws.rs.core.Application;

/**
 * Configuration JAX-RS
 * Cette classe active JAX-RS et définit le chemin de base pour tous les endpoints REST
 */
@ApplicationPath("/api")
public class ApplicationConfig extends Application {
    // Pas besoin de surcharger getClasses() ou getSingletons()
    // Le serveur détectera automatiquement les ressources annotées avec @Path
}