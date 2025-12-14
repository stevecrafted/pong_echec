using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pong_serveur.Game
{
    /// <summary>
    /// Client pour communiquer avec l'API d'état du jeu
    /// </summary>
    public class GameStateClient
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private const string BASE_URL = "http://localhost:8080/compte-courant-1.0.0/api/game";

        /// <summary>
        /// Charge l'état du jeu depuis l'API
        /// </summary>
        public static async Task<GameStateDTO> ChargerEtatAsync()
        {
            try
            {
                Console.WriteLine("📡 Chargement de l'état du jeu depuis l'API...");
                
                string json = await client.GetStringAsync($"{BASE_URL}/state");
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var state = JsonSerializer.Deserialize<GameStateDTO>(json, options);
                
                Console.WriteLine("✓ État chargé depuis l'API:");
                Console.WriteLine($"  • Nombre de pièces: {state.NombrePieces}");
                Console.WriteLine($"  • Balle: ({state.BallX}, {state.BallY})");
                Console.WriteLine($"  • Balle lancée: {state.BalleLancee}");
                Console.WriteLine($"  • Raquette J1: ({state.Raquette1X}, {state.Raquette1Y})");
                Console.WriteLine($"  • Raquette J2: ({state.Raquette2X}, {state.Raquette2Y})");
                
                if (state.ViesPieces != null && state.ViesPieces.Count > 0)
                {
                    Console.WriteLine("  • Vies des pièces:");
                    foreach (var kvp in state.ViesPieces)
                    {
                        Console.WriteLine($"    - {kvp.Key}: {kvp.Value}");
                    }
                }
                
                return state;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Erreur HTTP: {ex.Message}");
                throw new Exception("Impossible de charger l'état du jeu depuis l'API", ex);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Erreur parsing JSON: {ex.Message}");
                throw new Exception("Format de réponse invalide", ex);
            }
        }

        /// <summary>
        /// Sauvegarde l'état complet du jeu
        /// </summary>
        public static async Task SauvegarderEtatAsync(GameStateDTO state)
        {
            try
            {
                Console.WriteLine("💾 Sauvegarde de l'état du jeu...");
                
                var json = JsonSerializer.Serialize(state);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.PutAsync($"{BASE_URL}/save", content);
                response.EnsureSuccessStatusCode();
                
                Console.WriteLine("✓ État sauvegardé avec succès");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la sauvegarde: {ex.Message}");
            }
        }

        /// <summary>
        /// Réinitialise le jeu avec un nombre de pièces donné
        /// </summary>
        public static async Task ReinitialiserJeuAsync(int nombrePieces)
        {
            try
            {
                Console.WriteLine($"🔄 Réinitialisation du jeu avec {nombrePieces} pièces...");
                
                var response = await client.PostAsync(
                    $"{BASE_URL}/reset?pieces={nombrePieces}", 
                    null
                );
                response.EnsureSuccessStatusCode();
                
                Console.WriteLine("✓ Jeu réinitialisé");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la réinitialisation: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Met à jour la position de la balle
        /// </summary>
        public static async Task MettreAJourBalleAsync(int x, int y, int speedX, int speedY)
        {
            try
            {
                var update = new { x, y, speedX, speedY };
                var json = JsonSerializer.Serialize(update);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                await client.PutAsync($"{BASE_URL}/ball", content);
            }
            catch (Exception ex)
            {
                // Log mais ne pas crasher
                Console.WriteLine($"⚠️ Erreur mise à jour balle: {ex.Message}");
            }
        }

        /// <summary>
        /// Met à jour la position d'une raquette
        /// </summary>
        public static async Task MettreAJourRaquetteAsync(int joueur, int x, int y)
        {
            try
            {
                var update = new { x, y };
                var json = JsonSerializer.Serialize(update);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                await client.PutAsync($"{BASE_URL}/raquette/{joueur}", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur mise à jour raquette: {ex.Message}");
            }
        }

        /// <summary>
        /// Lance la balle
        /// </summary>
        public static async Task LancerBalleAsync(int speedX, int speedY)
        {
            try
            {
                var launch = new { speedX, speedY };
                var json = JsonSerializer.Serialize(launch);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                await client.PostAsync($"{BASE_URL}/launch", content);
                Console.WriteLine($"✓ Balle lancée avec vitesse ({speedX}, {speedY})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lancement balle: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// DTO pour l'état du jeu (correspond au JSON de l'API)
    /// </summary>
    public class GameStateDTO
    {
        public int NombrePieces { get; set; }
        
        public int BallX { get; set; }
        public int BallY { get; set; }
        public int BallSpeedX { get; set; }
        public int BallSpeedY { get; set; }
        public bool BalleLancee { get; set; }
        
        public int Raquette1X { get; set; }
        public int Raquette1Y { get; set; }
        public int Raquette2X { get; set; }
        public int Raquette2Y { get; set; }
        
        // Vies des pièces (Map<TypePiece, Vie>)
        public Dictionary<string, int> ViesPieces { get; set; }
    }
}