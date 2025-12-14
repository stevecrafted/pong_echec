package com.banque.compte.dto;

import java.util.Map;

public class GameStateDTO {

    public int nombrePieces;

    public int ballX;
    public int ballY;
    public int ballSpeedX;
    public int ballSpeedY;
    public boolean balleLancee;

    public int raquette1X;
    public int raquette1Y;
    public int raquette2X;
    public int raquette2Y;

    public Map<String, Integer> viesPieces;
}
