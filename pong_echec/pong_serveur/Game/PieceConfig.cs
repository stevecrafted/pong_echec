using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using pong_shared.Models;

namespace pong_serveur.Game
{
    public class PieceConfig
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5) // Timeout pour éviter d'attendre indéfiniment
        };

        public Dictionary<TypePiece, int> VieParType { get; private set; }

        // Ordre d'un jeu d'échecs classique
        public static readonly List<TypePiece> OrdreEchecs = new()
        {
            TypePiece.Tour,
            TypePiece.Cavalier,
            TypePiece.Fou,
            TypePiece.Reine,
            TypePiece.Roi,
            TypePiece.Fou,
            TypePiece.Cavalier,
            TypePiece.Tour
        };

        private PieceConfig() { }

        /// <summary>
        /// Crée une instance de PieceConfig en chargeant la configuration depuis l'API
        /// </summary>
        public static async Task<PieceConfig> CreerAsync()
        {
            var config = new PieceConfig();
            await config.ChargerConfigAsync();
            return config;
        }

        /// <summary>
        /// Crée une instance de PieceConfig avec les valeurs par défaut (sans appel API)
        /// Utile pour les tests ou mode offline
        /// </summary 

        private async Task ChargerConfigAsync()
        {
            try
            {
                Console.WriteLine("Chargement de la configuration depuis l'API...");
                
                string json = await client.GetStringAsync(
                    "http://localhost:8080/compte-courant-1.0.0/api/config");

                var data = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                
                if (data == null || data.Count == 0)
                {
                    throw new InvalidOperationException("La configuration reçue est vide");
                }

                VieParType = new Dictionary<TypePiece, int>();

                foreach (var kv in data)
                {
                    if (Enum.TryParse(kv.Key, out TypePiece type))
                    {
                        VieParType[type] = kv.Value;
                        Console.WriteLine($"  {type}: {kv.Value} PV");
                    }
                    else
                    {
                        Console.WriteLine($"Attention: Type de pièce inconnu '{kv.Key}' ignoré");
                    }
                }

                // Vérifier que tous les types essentiels sont présents
                ValiderConfiguration();

                Console.WriteLine("Configuration chargée avec succès depuis l'API");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Erreur HTTP lors du chargement de la config: {ex.Message}");
                Console.WriteLine("   Utilisation des valeurs par défaut");
                
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Erreur de parsing JSON: {ex.Message}");
                Console.WriteLine("   Utilisation des valeurs par défaut");
                
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"❌ Timeout lors de la connexion à l'API: {ex.Message}");
                Console.WriteLine("   Utilisation des valeurs par défaut");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur inattendue: {ex.Message}");
                Console.WriteLine("   Utilisation des valeurs par défaut");
                
            }
        }

        private void ValiderConfiguration()
        {
            var typesRequis = new[] 
            { 
                TypePiece.Pion, 
                TypePiece.Tour, 
                TypePiece.Cavalier, 
                TypePiece.Fou, 
                TypePiece.Reine, 
                TypePiece.Roi 
            };

            var typesManquants = new List<TypePiece>();

            foreach (var type in typesRequis)
            {
                if (!VieParType.ContainsKey(type))
                {
                    typesManquants.Add(type);
                }
            }

            if (typesManquants.Count > 0)
            {
                Console.WriteLine($"⚠️ Attention: Types manquants dans la config: {string.Join(", ", typesManquants)}");
                
                // Ajouter les valeurs par défaut pour les types manquants
                foreach (var type in typesManquants)
                {
                    VieParType[type] = 1; // Valeur par défaut
                    Console.WriteLine($"   {type} ajouté avec 1 PV par défaut");
                }
            }
        }

        /// <summary>
        /// Obtient la vie d'un type de pièce de manière sécurisée
        /// </summary>
        public int ObtenirVie(TypePiece type)
        {
            if (VieParType.TryGetValue(type, out int vie))
            {
                return vie;
            }

            Console.WriteLine($"⚠️ Vie non trouvée pour {type}, retour de 1 par défaut");
            return 1;
        }

        /// <summary>
        /// Charge les valeurs par défaut si l'API n'est pas disponible
        /// </summary>
        private void ChargerValeursParDefaut()
        {
            VieParType = new Dictionary<TypePiece, int>
            {
                { TypePiece.Pion, 1 },
                { TypePiece.Tour, 3 },
                { TypePiece.Cavalier, 2 },
                { TypePiece.Fou, 2 },
                { TypePiece.Reine, 5 },
                { TypePiece.Roi, 1 }
            };
            
            Console.WriteLine("Configuration par défaut chargée:");
            foreach (var kv in VieParType)
            {
                Console.WriteLine($"  {kv.Key}: {kv.Value} PV");
            }
        }

        /// <summary>
        /// Affiche la configuration actuelle
        /// </summary>
        public void AfficherConfiguration()
        {
            Console.WriteLine("\n=== Configuration des pièces ===");
            foreach (var kv in VieParType.OrderBy(x => x.Key.ToString()))
            {
                Console.WriteLine($"{kv.Key,-12}: {kv.Value} PV");
            }
            Console.WriteLine("================================\n");
        }
    }
}