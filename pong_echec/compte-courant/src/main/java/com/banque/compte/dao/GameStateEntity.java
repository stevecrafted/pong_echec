package com.banque.compte.dao;

import jakarta.persistence.*; 
import java.io.Serializable;
import java.util.Map;

@Entity
@Table(name = "game_state")
public class GameStateEntity implements Serializable {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    // Configuration
    @Column(name = "nombre_pieces")
    private int nombrePieces;

    // Balle
    @Column(name = "ball_x")
    private int ballX;
    
    @Column(name = "ball_y")
    private int ballY;
    
    @Column(name = "ball_speed_x")
    private int ballSpeedX;
    
    @Column(name = "ball_speed_y")
    private int ballSpeedY;
    
    @Column(name = "balle_lancee")
    private boolean balleLancee;

    // Raquettes
    @Column(name = "raquette1_x")
    private int raquette1X;
    
    @Column(name = "raquette1_y")
    private int raquette1Y;
    
    @Column(name = "raquette2_x")
    private int raquette2X;
    
    @Column(name = "raquette2_y")
    private int raquette2Y;

    // JSONB stocké comme TEXT avec conversion manuelle
    @Column(name = "vies_pieces", columnDefinition = "text")
    @Convert(converter = JsonMapConverter.class)
    private Map<String, Integer> viesPieces;

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