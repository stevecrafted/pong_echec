package com.banque.compte.entity;

import java.util.Map;

/**
 * Classe qui contient toutes les données statiques du jeu
 */
public class GameDataConfig {
    
    // Configuration des pièces (type -> vie)
    private Map<String, Integer> pieceConfig;
    
    // Position initiale de la balle
    private BallInitialConfig ballConfig;
    
    // Positions initiales des raquettes
    private RaquetteInitialConfig raquette1Config;
    private RaquetteInitialConfig raquette2Config;
    
    // Configuration du terrain
    private TerrainConfig terrainConfig;
    
    public GameDataConfig() {}
    
    // Getters et Setters
    public Map<String, Integer> getPieceConfig() {
        return pieceConfig;
    }
    
    public void setPieceConfig(Map<String, Integer> pieceConfig) {
        this.pieceConfig = pieceConfig;
    }
    
    public BallInitialConfig getBallConfig() {
        return ballConfig;
    }
    
    public void setBallConfig(BallInitialConfig ballConfig) {
        this.ballConfig = ballConfig;
    }
    
    public RaquetteInitialConfig getRaquette1Config() {
        return raquette1Config;
    }
    
    public void setRaquette1Config(RaquetteInitialConfig raquette1Config) {
        this.raquette1Config = raquette1Config;
    }
    
    public RaquetteInitialConfig getRaquette2Config() {
        return raquette2Config;
    }
    
    public void setRaquette2Config(RaquetteInitialConfig raquette2Config) {
        this.raquette2Config = raquette2Config;
    }
    
    public TerrainConfig getTerrainConfig() {
        return terrainConfig;
    }
    
    public void setTerrainConfig(TerrainConfig terrainConfig) {
        this.terrainConfig = terrainConfig;
    }
    
    // Classes internes pour les sous-configurations
    public static class BallInitialConfig {
        private int posX;
        private int posY;
        private int speedX;
        private int speedY;
        private int radius;
        
        public BallInitialConfig() {}
        
        public BallInitialConfig(int posX, int posY, int speedX, int speedY, int radius) {
            this.posX = posX;
            this.posY = posY;
            this.speedX = speedX;
            this.speedY = speedY;
            this.radius = radius;
        }
        
        // Getters et Setters
        public int getPosX() { return posX; }
        public void setPosX(int posX) { this.posX = posX; }
        
        public int getPosY() { return posY; }
        public void setPosY(int posY) { this.posY = posY; }
        
        public int getSpeedX() { return speedX; }
        public void setSpeedX(int speedX) { this.speedX = speedX; }
        
        public int getSpeedY() { return speedY; }
        public void setSpeedY(int speedY) { this.speedY = speedY; }
        
        public int getRadius() { return radius; }
        public void setRadius(int radius) { this.radius = radius; }
    }
    
    public static class RaquetteInitialConfig {
        private int joueurId;
        private float posX;
        private float posY;
        
        public RaquetteInitialConfig() {}
        
        public RaquetteInitialConfig(int joueurId, float posX, float posY) {
            this.joueurId = joueurId;
            this.posX = posX;
            this.posY = posY;
        }
        
        // Getters et Setters
        public int getJoueurId() { return joueurId; }
        public void setJoueurId(int joueurId) { this.joueurId = joueurId; }
        
        public float getPosX() { return posX; }
        public void setPosX(float posX) { this.posX = posX; }
        
        public float getPosY() { return posY; }
        public void setPosY(float posY) { this.posY = posY; }
    }
    
    public static class TerrainConfig {
        private int width;
        private int height;
        
        public TerrainConfig() {}
        
        public TerrainConfig(int width, int height) {
            this.width = width;
            this.height = height;
        }
        
        // Getters et Setters
        public int getWidth() { return width; }
        public void setWidth(int width) { this.width = width; }
        
        public int getHeight() { return height; }
        public void setHeight(int height) { this.height = height; }
    }
}
