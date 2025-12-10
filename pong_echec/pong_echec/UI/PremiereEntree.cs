using pong_echec.Reseau;

namespace pong_echec.UI
{
    public class PremiereEntree
    {
        public async static void DemanderModeReseau(ClientReseau clientReseau, bool modeReseau)
        {
            var result = MessageBox.Show(
                "Voulez-vous jouer en mode réseau ?",
                "Mode de jeu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                string adresse = PromptAdresseServeur();
                if (!string.IsNullOrEmpty(adresse))
                {
                    bool connecte = await clientReseau.ConnecterAsync(adresse, 5000);
                    if (connecte)
                    {
                        modeReseau = true;
                        MessageBox.Show("Connecté au serveur!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Impossible de se connecter au serveur.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public static string PromptAdresseServeur()
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Adresse du serveur",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = "Adresse IP du serveur:", Width = 350 };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 350, Text = "127.0.0.1" };
            Button confirmation = new Button() { Text = "OK", Left = 250, Width = 100, Top = 80, DialogResult = DialogResult.OK };

            confirmation.Click += (sender, e) => { prompt.Close(); };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }
    }
}