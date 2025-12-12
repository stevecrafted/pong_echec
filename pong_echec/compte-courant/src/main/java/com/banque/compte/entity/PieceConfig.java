package com.banque.compte.entity;

import jakarta.persistence.*;

@Entity
@Table(name = "piece_config")
public class PieceConfig {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "type_piece", nullable = false, unique = true)
    private String typePiece;
    
    @Column(name = "vie", nullable = false)
    private Integer vie;
    
    public PieceConfig() {}
    
    public PieceConfig(String typePiece, Integer vie) {
        this.typePiece = typePiece;
        this.vie = vie;
    }
    
    public Long getId() {
        return id;
    }
    
    public void setId(Long id) {
        this.id = id;
    }
    
    public String getTypePiece() {
        return typePiece;
    }
    
    public void setTypePiece(String typePiece) {
        this.typePiece = typePiece;
    }
    
    public Integer getVie() {
        return vie;
    }
    
    public void setVie(Integer vie) {
        this.vie = vie;
    }

}