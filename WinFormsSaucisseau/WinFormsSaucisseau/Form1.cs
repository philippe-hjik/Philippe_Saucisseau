using MQTTnet.Client;
using MQTTnet;
using System.IO; // Nécessaire pour Directory.GetFiles()
using System.Windows.Forms;
using System.Text;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace WinFormsSaucisseau
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeListView();

        }

        string broker = "mqtt.blue.section-inf.ch";
        int port = 1883;
        string clientId = Guid.NewGuid().ToString();
        string topic = "test";
        string username = "ict";
        string password = "321";

        private void Form1_Load(object sender, EventArgs e)
        {
            // Vous pouvez spécifier un dossier ici
            string dossierMusique = @"C:\Users\pf25xeu\Desktop\musique"; // Remplacez par votre dossier

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

            creatConnection();
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

        public async void creatConnection()
        {

            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            var mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                .WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                MessageBox.Show("Connected to MQTT broker successfully.");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    MessageBox.Show($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    return Task.CompletedTask;
                };
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

    }
}
