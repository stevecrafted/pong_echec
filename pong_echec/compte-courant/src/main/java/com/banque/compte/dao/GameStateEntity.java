package com.banque.compte.dao;

import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import java.io.Serializable;
import java.util.Map;

@Entity
@Table(name = "game_state")
public class GameStateEntity implements Serializable {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    /* =======================
       Configuration
       ======================= */

    @JsonProperty("NombrePieces")
    @Column(name = "nombre_pieces", nullable = false)
    private int nombrePieces;

    /* =======================
       Balle
       ======================= */

    @JsonProperty("BallX")
    @Column(name = "ball_x", nullable = false)
    private int ballX;

    @JsonProperty("BallY")
    @Column(name = "ball_y", nullable = false)
    private int ballY;

    @JsonProperty("BallSpeedX")
    @Column(name = "ball_speed_x", nullable = false)
    private int ballSpeedX;

    @JsonProperty("BallSpeedY")
    @Column(name = "ball_speed_y", nullable = false)
    private int ballSpeedY;

    @JsonProperty("BalleLancee")
    @Column(name = "balle_lancee", nullable = false)
    private boolean balleLancee;

    /* =======================
       Raquettes
       ======================= */

    @JsonProperty("Raquette1X")
    @Column(name = "raquette1_x", nullable = false)
    private int raquette1X;

    @JsonProperty("Raquette1Y")
    @Column(name = "raquette1_y", nullable = false)
    private int raquette1Y;

    @JsonProperty("Raquette2X")
    @Column(name = "raquette2_x", nullable = false)
    private int raquette2X;

    @JsonProperty("Raquette2Y")
    @Column(name = "raquette2_y", nullable = false)
    private int raquette2Y;

    /* =======================
       Vies des pièces (JSON)
       ======================= */

    @JsonProperty("ViesPiecesJoueur1")
    @Column(name = "vies_pieces_joueur1", columnDefinition = "text")
    @Convert(converter = JsonMapConverter.class)
    private Map<String, Integer> viesPiecesJoueur1;

    @JsonProperty("ViesPiecesJoueur2")
    @Column(name = "vies_pieces_joueur2", columnDefinition = "text")
    @Convert(converter = JsonMapConverter.class)
    private Map<String, Integer> viesPiecesJoueur2;

    /* =======================
       Getters / Setters
       ======================= */

    public Long getId() {
        return id;
    }

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

    public Map<String, Integer> getViesPiecesJoueur1() {
        return viesPiecesJoueur1;
    }

    public void setViesPiecesJoueur1(Map<String, Integer> viesPiecesJoueur1) {
        this.viesPiecesJoueur1 = viesPiecesJoueur1;
    }

    public Map<String, Integer> getViesPiecesJoueur2() {
        return viesPiecesJoueur2;
    }

    public void setViesPiecesJoueur2(Map<String, Integer> viesPiecesJoueur2) {
        this.viesPiecesJoueur2 = viesPiecesJoueur2;
    }
}
