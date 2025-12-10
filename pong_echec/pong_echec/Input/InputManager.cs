namespace pong_echec.Input
{
    /// <summary>
    /// Gère l'état de toutes les touches du clavier.
    /// Utilise le pattern Singleton pour un accès global.
    /// </summary>
    public class InputManager
    {
        // Singleton
        private static InputManager? _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InputManager();
                return _instance;
            }
        }

        // Dictionnaire pour stocker l'état des touches
        private Dictionary<Keys, bool> touchesPressees;
        private Dictionary<Keys, bool> touchesPresseesFramePrecedente;

        private InputManager()
        {
            touchesPressees = new Dictionary<Keys, bool>();
            touchesPresseesFramePrecedente = new Dictionary<Keys, bool>();
        }

        /// <summary>
        /// Vérifie si une touche est actuellement pressée
        /// </summary>
        public bool EstPresse(Keys touche)
        {
            // Console.WriteLine("Touche " + touche + " pressée");
            return touchesPressees.ContainsKey(touche) && touchesPressees[touche];
        }

        /// <summary>
        /// Vérifie si une touche vient d'être pressée (pas pressée avant, pressée maintenant)
        /// Utile pour actions ponctuelles (sauter, tirer, etc.)
        /// </summary>
        public bool EstPresseCetteFrame(Keys touche)
        {
            bool presseeMaintenant = EstPresse(touche);
            bool presseeAvant = touchesPresseesFramePrecedente.ContainsKey(touche) 
                && touchesPresseesFramePrecedente[touche];
            
            return presseeMaintenant && !presseeAvant;
        }

        /// <summary>
        /// Vérifie si une touche vient d'être relâchée
        /// </summary>
        public bool EstRelacheCetteFrame(Keys touche)
        {
            bool presseeMaintenant = EstPresse(touche);
            bool presseeAvant = touchesPresseesFramePrecedente.ContainsKey(touche) 
                && touchesPresseesFramePrecedente[touche];
            
            return !presseeMaintenant && presseeAvant;
        }

        /// <summary>
        /// Appelé quand une touche est enfoncée
        /// </summary>
        public void OnKeyDown(Keys touche)
        {
            touchesPressees[touche] = true;
        }

        /// <summary>
        /// Appelé quand une touche est relâchée
        /// </summary>
        public void OnKeyUp(Keys touche)
        {
            touchesPressees[touche] = false;
        }

        /// <summary>
        /// Met à jour l'état pour la frame suivante
        /// À appeler à chaque frame dans la game loop
        /// </summary>
        public void Update()
        {
            // Sauvegarder l'état actuel pour la prochaine frame
            touchesPresseesFramePrecedente.Clear();
            foreach (var kvp in touchesPressees)
            {
                touchesPresseesFramePrecedente[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Réinitialise tous les états (utile pour pause, changement de scène, etc.)
        /// </summary>
        public void Reset()
        {
            touchesPressees.Clear();
            touchesPresseesFramePrecedente.Clear();
        }

        /// <summary>
        /// Retourne une valeur -1, 0 ou 1 selon deux touches (utile pour axes de mouvement)
        /// Exemple: GetAxis(Keys.Left, Keys.Right) retourne -1 si Left, +1 si Right, 0 si aucune/les deux
        /// </summary>
        public int GetAxis(Keys toucheNegative, Keys touchePositive)
        {
            bool negative = EstPresse(toucheNegative);
            bool positive = EstPresse(touchePositive);

            if (negative && !positive) return -1;
            if (positive && !negative) return 1;
            return 0;
        }

        /// <summary>
        /// Vérifie si AU MOINS UNE des touches fournies est pressée
        /// Utile pour actions alternatives (Espace OU Entrée pour sauter)
        /// </summary>
        public bool EstPresseParmi(params Keys[] touches)
        {
            foreach (var touche in touches)
            {
                if (EstPresse(touche))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Vérifie si TOUTES les touches fournies sont pressées
        /// Utile pour combos (Ctrl + S pour sauvegarder)
        /// </summary>
        public bool SontToutesPressees(params Keys[] touches)
        {
            foreach (var touche in touches)
            {
                if (!EstPresse(touche))
                    return false;
            }
            return true;
        }
    }
}