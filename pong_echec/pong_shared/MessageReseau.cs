using System.Text.Json;
using pong_shared.Models;
using System.Collections.Generic;

namespace pong_shared
{
    // Types de messages
    public enum TypeMessage
    {
        UpdateBall,
        UpdateRaquette,
        AssignerJoueurRaquette,
        ConnexionClient,
        DeconnexionClient,
        UpdatePieces,
        UpdatePiece,
        PlayerReady,
        UpdateGameState
    }

    // Classe de base pour les messages
    public class MessageReseau
    {
        public TypeMessage Type { get; set; }
        public string Data { get; set; } = string.Empty;

        public static MessageReseau CreerUpdateBall(int posX, int posY, int speedX, int speedY)
        {
            var ballData = new BallData
            {
                PosX = posX,
                PosY = posY,
                SpeedX = speedX,
                SpeedY = speedY
            };

            return new MessageReseau
            {
                Type = TypeMessage.UpdateBall,
                Data = JsonSerializer.Serialize(ballData)
            };
        }

        public static MessageReseau CreerUpdateRaquette(int joueurId, float posX, float posY)
        {
            var raquetteData = new RaquetteData
            {
                JoueurId = joueurId,
                PosX = posX,
                PosY = posY
            };

            return new MessageReseau
            {
                Type = TypeMessage.UpdateRaquette,
                Data = JsonSerializer.Serialize(raquetteData)
            };
        }

        public static MessageReseau CreerAssignerJoueur(int joueurId)
        {
            return new MessageReseau
            {
                Type = TypeMessage.AssignerJoueurRaquette,
                Data = joueurId.ToString()
            };
        }

        public static MessageReseau CreerUpdatePieces(List<PieceData> pieces)
        {
            return new MessageReseau
            {
                Type = TypeMessage.UpdatePieces,
                Data = JsonSerializer.Serialize(pieces)
            };
        }

        public static MessageReseau CreerPlayerReady()
        {
            return new MessageReseau { Type = TypeMessage.PlayerReady };
        }

        public static MessageReseau CreerUpdateGameState(GameStateType state)
        {
            var gameState = new GameState { StateType = state };
            return new MessageReseau
            {
                Type = TypeMessage.UpdateGameState,
                Data = JsonSerializer.Serialize(gameState)
            };
        }

        public BallData? ExtraireDataBall()
        {
            if (Type == TypeMessage.UpdateBall)
            {
                return JsonSerializer.Deserialize<BallData>(Data);
            }
            return null;
        }

        public RaquetteData? ExtraireDataRaquette()
        {
            if (Type == TypeMessage.UpdateRaquette)
            {
                return JsonSerializer.Deserialize<RaquetteData>(Data);
            }
            return null;
        }

        public int ExtraireJoueurId()
        {
            if (Type == TypeMessage.AssignerJoueurRaquette)
            {
                return int.Parse(Data);
            }
            return -1;
        }

        public List<PieceData>? ExtraireDataPieces()
        {
            if (Type == TypeMessage.UpdatePieces)
            {
                return JsonSerializer.Deserialize<List<PieceData>>(Data);
            }
            return null;
        }

        public GameState? ExtraireDataGameState()
        {
            if (Type == TypeMessage.UpdateGameState)
            {
                return JsonSerializer.Deserialize<GameState>(Data);
            }
            return null;
        }

        public string Serialiser()
        {
            return JsonSerializer.Serialize(this);
        }

        public static MessageReseau? Deserialiser(string json)
        {
            return JsonSerializer.Deserialize<MessageReseau>(json);
        }
    }
}
