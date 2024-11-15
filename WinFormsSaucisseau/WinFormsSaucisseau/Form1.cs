using System.IO; // Nécessaire pour Directory.GetFiles()
using System.Windows.Forms;

namespace WinFormsSaucisseau
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeListView();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Vous pouvez spécifier un dossier ici
            string dossierMusique = @"C:\Chemin\Vers\Votre\Dossier\Musique"; // Remplacez par votre dossier

            // Vérifiez que le dossier existe
            if (Directory.Exists(dossierMusique))
            {
                // Récupérer tous les fichiers .mp3 et .wav du dossier
                string[] fichiersAudio = Directory.GetFiles(dossierMusique, "*.mp3");

                foreach (var fichier in fichiersAudio)
                {
                    // Obtenir le nom du fichier sans le chemin
                    string nomFichier = Path.GetFileName(fichier);
                    string tailleFichier = new FileInfo(fichier).Length.ToString();

                    // Ajouter le fichier à la ListView
                    listView1.Items.Add(new ListViewItem(new[] { nomFichier, tailleFichier }));
                }
            }
            else
            {
                MessageBox.Show("Le dossier spécifié n'existe pas.");
            }
        }

        private void InitializeListView()
        {
            // Configuration de la ListView
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("Titre", 200); // Colonne pour les titres de musique
            listView1.Columns.Add("Taille", 100); // Colonne pour la taille du fichier

            // Attacher un gestionnaire d'événement pour double-clic
           // listView1.MouseDoubleClick += ListView1_MouseDoubleClick;
        }
    }
}
