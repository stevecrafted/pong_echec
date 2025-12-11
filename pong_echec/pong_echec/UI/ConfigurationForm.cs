using System;
using System.Drawing;
using System.Windows.Forms;
using pong_shared.Models;

namespace pong_echec.UI
{
    public class ConfigurationForm : Form
    {
        public int NombrePiecesChoisi { get; private set; } = 4;
        public string AdresseServeur { get; private set; } = "127.0.0.1";
        public int Port { get; private set; } = 5000;

        private RadioButton radio2Pieces;
        private RadioButton radio4Pieces;
        private RadioButton radio6Pieces;
        private RadioButton radio8Pieces;
        private TextBox txtAdresseServeur;
        private TextBox txtPort;
        private Button btnValider;
        private Panel panelPreview;

        public ConfigurationForm()
        {
            InitialiserInterface();
        }

        private void InitialiserInterface()
        {
            this.Text = "Configuration - Pong Échecs";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Titre
            Label lblTitre = new Label
            {
                Text = "⚔️ PONG ÉCHECS ⚔️",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(580, 50),
                Location = new Point(10, 10)
            };
            this.Controls.Add(lblTitre);

            // Section nombre de pièces
            GroupBox groupPieces = new GroupBox
            {
                Text = "Nombre de pièces par joueur",
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(20, 70),
                Size = new Size(350, 200)
            };

            radio2Pieces = CreerRadioButton("⚡ 2 pièces - Mode Rapide", 20, 30, true);
            radio2Pieces.CheckedChanged += (s, e) => { if (radio2Pieces.Checked) { NombrePiecesChoisi = 2; UpdatePreview(); } };

            radio4Pieces = CreerRadioButton("⭐ 4 pièces - Mode Normal", 20, 70, false);
            radio4Pieces.CheckedChanged += (s, e) => { if (radio4Pieces.Checked) { NombrePiecesChoisi = 4; UpdatePreview(); } };

            radio6Pieces = CreerRadioButton("💎 6 pièces - Mode Avancé", 20, 110, false);
            radio6Pieces.CheckedChanged += (s, e) => { if (radio6Pieces.Checked) { NombrePiecesChoisi = 6; UpdatePreview(); } };

            radio8Pieces = CreerRadioButton("👑 8 pièces - Mode Complet", 20, 150, false);
            radio8Pieces.CheckedChanged += (s, e) => { if (radio8Pieces.Checked) { NombrePiecesChoisi = 8; UpdatePreview(); } };

            groupPieces.Controls.AddRange(new Control[] { radio2Pieces, radio4Pieces, radio6Pieces, radio8Pieces });
            this.Controls.Add(groupPieces);

            // Aperçu du terrain
            panelPreview = new Panel
            {
                Location = new Point(380, 70),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };
            panelPreview.Paint += PanelPreview_Paint;
            this.Controls.Add(panelPreview);

            // Section réseau
            GroupBox groupReseau = new GroupBox
            {
                Text = "Configuration réseau",
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(20, 280),
                Size = new Size(560, 120)
            };

            Label lblServeur = new Label
            {
                Text = "Serveur:",
                ForeColor = Color.White,
                Location = new Point(20, 30),
                AutoSize = true
            };

            txtAdresseServeur = new TextBox
            {
                Text = "127.0.0.1",
                Location = new Point(180, 28),
                Size = new Size(150, 25),
                Font = new Font("Arial", 10)
            };

            Label lblPort = new Label
            {
                Text = "Port:",
                ForeColor = Color.White,
                Location = new Point(350, 30),
                AutoSize = true
            };

            txtPort = new TextBox
            {
                Text = "5000",
                Location = new Point(400, 28),
                Size = new Size(80, 25),
                Font = new Font("Arial", 10)
            };

            Label lblInfo = new Label
            {
                Text = "💡 Utilisez 127.0.0.1 pour jouer sur le même PC\n" +
                       "   Utilisez l'IP du serveur pour jouer en réseau",
                ForeColor = Color.LightGray,
                Location = new Point(20, 65),
                Size = new Size(520, 40),
                Font = new Font("Arial", 9)
            };

            groupReseau.Controls.AddRange(new Control[] { lblServeur, txtAdresseServeur, lblPort, txtPort, lblInfo });
            this.Controls.Add(groupReseau);

            // Bouton valider
            btnValider = new Button
            {
                Text = "🎮 JOUER",
                Location = new Point(200, 415),
                Size = new Size(200, 50),
                Font = new Font("Arial", 16, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnValider.FlatAppearance.BorderSize = 0;
            btnValider.Click += BtnValider_Click;
            this.Controls.Add(btnValider);

            // Effet hover sur le bouton
            btnValider.MouseEnter += (s, e) => btnValider.BackColor = Color.FromArgb(0, 150, 255);
            btnValider.MouseLeave += (s, e) => btnValider.BackColor = Color.FromArgb(0, 120, 215);
        }

        private RadioButton CreerRadioButton(string texte, int x, int y, bool isChecked)
        {
            return new RadioButton
            {
                Text = texte,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Arial", 11),
                Checked = isChecked
            };
        }

        private void UpdatePreview()
        {
            panelPreview.Invalidate();
        }

        private void PanelPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            // Calculer les dimensions du terrain selon le nombre de pièces
            var config = ConfigurationJeu.ObtenirConfiguration(NombrePiecesChoisi);
            
            // Échelle pour l'aperçu
            float scale = Math.Min(
                (float)panelPreview.Width / config.TerrainWidth,
                (float)panelPreview.Height / config.TerrainHeight
            );

            int previewWidth = (int)(config.TerrainWidth * scale);
            int previewHeight = (int)(config.TerrainHeight * scale);
            int offsetX = (panelPreview.Width - previewWidth) / 2;
            int offsetY = (panelPreview.Height - previewHeight) / 2;

            // Dessiner le terrain
            g.FillRectangle(Brushes.DarkGreen, offsetX, offsetY, previewWidth, previewHeight);
            g.DrawRectangle(Pens.White, offsetX, offsetY, previewWidth, previewHeight);

            // Dessiner les pièces (simplifié)
            int pieceSize = (int)(100 * scale);
            int spacing = (int)(110 * scale);

            // Pièces joueur 1 (bas)
            for (int i = 0; i < NombrePiecesChoisi; i++)
            {
                int px = offsetX + (int)(10 * scale) + i * spacing;
                int py = offsetY + (int)(10 * scale);
                g.FillRectangle(Brushes.Blue, px, py, pieceSize, pieceSize / 2);
            }

            // Pièces joueur 2 (haut)
            for (int i = 0; i < NombrePiecesChoisi; i++)
            {
                int px = offsetX + (int)(10 * scale) + i * spacing;
                int py = offsetY + previewHeight - (int)(120 * scale);
                g.FillRectangle(Brushes.Red, px, py, pieceSize, pieceSize / 2);
            }

            // Afficher les dimensions
            string dimensions = $"{config.TerrainWidth}×{config.TerrainHeight}";
            g.DrawString(dimensions, new Font("Arial", 8), Brushes.Yellow, 
                offsetX + 5, offsetY + previewHeight - 20);
        }

        private void BtnValider_Click(object sender, EventArgs e)
        {
            // Valider l'adresse IP
            AdresseServeur = txtAdresseServeur.Text.Trim();
            if (string.IsNullOrEmpty(AdresseServeur))
            {
                MessageBox.Show("Veuillez entrer une adresse serveur valide!", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Valider le port
            if (!int.TryParse(txtPort.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Le port doit être entre 1 et 65535!", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Port = port;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}