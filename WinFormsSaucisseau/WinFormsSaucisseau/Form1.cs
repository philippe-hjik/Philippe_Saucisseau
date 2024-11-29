using MQTTnet;
using MQTTnet.Protocol;
using System.IO; // Nécessaire pour Directory.GetFiles()
using System.Windows.Forms;
using System.Text;
using MQTTnet.Adapter;
using MQTTnet.Channel;
using TagLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;

namespace WinFormsSaucisseau
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeListView();
        }

        private IMqttClient mqttClient; // Client MQTT global
        private MqttClientOptions mqttOptions; // Options de connexion globales
                                               
        private MqttClientFactory factory = new MqttClientFactory();

        string broker = "mqtt.blue.section-inf.ch";
        int port = 1883;
        string clientId = Guid.NewGuid().ToString();
        string topic = "test";
        string username = "ict";
        string password = "321";

        List<MediaData> list;

        private void Form1_Load(object sender, EventArgs e)
        {
            // Vous pouvez spécifier un dossier ici
            string dossierMusique = @"C:\Users\pf25xeu\Desktop\musique"; // Remplacez par votre dossier

            // Vérifiez que le dossier existe
            if (Directory.Exists(dossierMusique))
            {
                // Récupérer tous les fichiers .mp3 du dossier
                string[] fichiersAudio = Directory.GetFiles(dossierMusique, "*.mp3");
                list = new List<MediaData>();

                foreach (var fichier in fichiersAudio)
                {
                    MediaData data = new MediaData();
                    TagLib.File musicinfo = TagLib.File.Create(fichier);

                    data.File_name = musicinfo.Tag.Title;
                    data.File_artist = musicinfo.Tag.FirstPerformer;
                    data.File_type = Path.GetExtension(fichier);
                    data.File_size = musicinfo.Length;

                    TimeSpan duration = musicinfo.Properties.Duration;
                    data.File_duration = $"{duration.Minutes:D2}:{duration.Seconds:D2}";

                    list.Add(data);

                    // Ajouter le fichier à la ListView
                    listView1.Items.Add(new ListViewItem(new[] { data.File_name, data.File_artist, data.File_type, data.File_duration }));
                }
            }
            else
            {
                MessageBox.Show("Le dossier spécifié n'existe pas.");

            }

            creatConnection();
        }

        // Méthode pour obtenir la liste des musiques
        private string GetMusicList()
        {
            if (listView1.InvokeRequired)
            {
                // Utilisation de BeginInvoke pour accéder au contrôle depuis le thread principal
                return (string)listView1.Invoke(new Func<string>(GetMusicList));
            }
            else
            {
                var musicList = new StringBuilder();

                foreach (ListViewItem item in listView1.Items)
                {
                    // Ajoutez chaque titre de musique à la chaîne
                    musicList.AppendLine(item.SubItems[0].Text); // La première colonne contient le titre
                }

                return musicList.ToString();
            }


        }

        private void InitializeListView()
        {
            // Configuration de la ListView
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("Titre", 200);     // Colonne pour les titres de musique
            listView1.Columns.Add("Artiste", 200);  // Colonne pour les titres de musique
            listView1.Columns.Add("Type", 100);    // Colonne pour la taille du fichier
            listView1.Columns.Add("Taille", 100); // Colonne pour la taille du fichier
            listView1.Columns.Add("Durée", 100); // Colonne pour la taille du fichier

            // Attacher un gestionnaire d'événement pour double-clic
            // listView1.MouseDoubleClick += ListView1_MouseDoubleClick;
        }

        public async void creatConnection()
        {

            mqttClient = factory.CreateMqttClient();

            // Créez les options de connexion MQTT
            mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithCredentials(username, password)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            // Connectez-vous au broker MQTT
            var connectResult = await mqttClient.ConnectAsync(mqttOptions);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                //MessageBox.Show("Connected to MQTT broker successfully.");

                // Subscribe with "No Local" option
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f =>
                    {
                        f.WithTopic(topic);
                        f.WithNoLocal(true); // Ensure the client does not receive its own messages
                    })
                    .Build();

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(subscribeOptions);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    //MessageBox.Show($"Received message: {receivedMessage}");

                    if (receivedMessage.Contains("HELLO") == true)
                    {
                        // Obtenez la liste des musiques
                        string musicList = GetMusicList();

                        // Construisez le message à envoyer
                        string response = $"{clientId} (Philippe) possède les musiques suivantes :\n{musicList}";

                        if (mqttClient == null || !mqttClient.IsConnected)
                        {
                            MessageBox.Show("Client not connected. Reconnecting...");
                            await mqttClient.ConnectAsync(mqttOptions);
                        }

                        // Créez le message à envoyer
                        var message = new MqttApplicationMessageBuilder()
                            .WithTopic(topic)
                            .WithPayload(response)
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                            .WithRetainFlag(false)
                            .Build();

                        // Envoyez le message
                        mqttClient.PublishAsync(message);
                        Console.WriteLine("Message sent successfully!");
                    }

                    return;
                };

            }
        }
       
        private async void SendData(string data)
        {
            // Create a MQTT client instance
            var mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                .WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            // Connectez-vous au broker MQTT
            var connectResult = await mqttClient.ConnectAsync(options);

            var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(data)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag()
                    .Build();

            await mqttClient.PublishAsync(message);
            await Task.Delay(1000); // Wait for 1 second

            mqttClient.UnsubscribeAsync(topic);
            mqttClient.DisconnectAsync();

        }
        
        private async void button1_Click_1(object sender, EventArgs e)
        {
            SendData("HELLO, qui a des musiques");
        }
    }
}