using MQTTnet;
using MQTTnet.Protocol;
using System.IO; // Nécessaire pour Directory.GetFiles()
using System.Windows.Forms;
using System.Text;
using MQTTnet.Adapter;
using MQTTnet.Channel;

using System.Diagnostics;
using System.Text.Json;
using WinFormsSaucisseau.Classes;
using WinFormsSaucisseau.Classes.Enveloppes;
using WinFormsSaucisseau.Classes.Interfaces;
using static System.Windows.Forms.DataFormats;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WinFormsSaucisseau
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeListView(listView1);
            InitializeListView(listView2);
            listView2.Columns.Add("PersonId", 150);   // Colonne pour le nom du fichier pour le download
        }

        private IMqttClient mqttClient; // Client MQTT global
        private MqttClientOptions mqttOptions; // Options de connexion globales

        private MqttClientFactory factory = new MqttClientFactory();

        string broker = "localhost";
        int port = 1883;
        string clientId = Guid.NewGuid().ToString();
        string topic = "lucastest";
        string username = "ict";
        string password = "321";

        private System.Windows.Forms.ListView listView2 = new System.Windows.Forms.ListView();


        // Vous pouvez spécifier un dossier ici
        string dossierMusique = @"C:\Users\pf25xeu\Desktop\musique"; // Remplacez par votre dossier

        List<MediaData> list;

        private void Form1_Load(object sender, EventArgs e)
        {
            updateMusicList();
            creatConnection();
        }

        private void updateMusicList()
        {
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

                    data.Title = musicinfo.Tag.Title;
                    data.Artist = musicinfo.Tag.FirstPerformer;
                    data.Type = Path.GetExtension(fichier);


                    TimeSpan duration = musicinfo.Properties.Duration;
                    data.Duration = $"{duration.Minutes:D2}:{duration.Seconds:D2}";

                    //data. = musicinfo.Tag.Title + Path.GetExtension(fichier);

                    list.Add(data);

                    // Ajouter le fichier à la ListView
                    listView1.Items.Add(new ListViewItem(new[] { data.Title, data.Artist, data.Type, data.Duration, data.Title + data.Type }));
                }
            }
            else
            {
                MessageBox.Show("Le dossier spécifié n'existe pas.");

            }
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
                    musicList.AppendLine(item.Text); // La première colonne contient le titre
                }

                return musicList.ToString();
            }

        }

        private void InitializeListView(System.Windows.Forms.ListView listView)
        {
            // Configuration de la ListView
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.Columns.Add("Titre", 200);     // Colonne pour les titres de musique
            listView.Columns.Add("Artiste", 200);  // Colonne pour les titres de musique
            listView.Columns.Add("Type", 100);    // Colonne pour la taille du fichier
            listView.Columns.Add("Taille", 100); // Colonne pour la taille du fichier
            listView.Columns.Add("nom", 100);   // Colonne pour le nom du fichier pour le download

            // Attacher un gestionnaire d'événement pour double-clic
            // listView1.MouseDoubleClick += ListView1_MouseDoubleClick;
        }

        private void updateOnlineMusic(Dictionary<string, List<MediaData>> data, GenericEnvelope envelope)
        {
            // S'assurer que l'appel est effectué sur le thread de l'interface utilisateur
            if (listView2.InvokeRequired)
            {
                listView2.Invoke(new Action(() => updateOnlineMusic(data, envelope)));
                return;
            }

            // Désactiver le rafraîchissement de la ListView pendant l'ajout des éléments pour éviter des redessins inutiles
            listView2.BeginUpdate();
            try
            {
                // Effacer les anciens éléments de la ListView
                listView2.Items.Clear();

                // Parcours du dictionnaire des musiques
                foreach (var item in data)
                {
                    // Ajoute chaque musique du propriétaire
                    item.Value.ForEach(dataItem =>
                    {
                        // Crée un nouvel élément pour la ListView avec les données de la musique
                        var listViewItem = new ListViewItem(new[]
                        {
                            dataItem.Title,  // Nom du fichier
                            dataItem.Artist,  // Artiste du fichier
                            dataItem.Type,  // Type du fichier
                            dataItem.Duration,  // Durée du fichier
                            envelope.SenderId  // Id de l'expéditeur
                });

                        // Ajouter l'élément à la ListView
                        listView2.Items.Add(listViewItem);
                    });
                }
            }
            finally
            {
                // Réactive le rafraîchissement de la ListView
                listView2.EndUpdate();
            }
        }


        public void getMssages()
        {
            if (mqttClient != null)
            {
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    ReiceiveMessage(e);
                    return Task.CompletedTask;
                };
            }
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

                await mqttClient.SubscribeAsync("philippe");

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    string receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

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

                    ReiceiveMessage(e);

                    return;
                };
            }
            getMssages();
        }

        public Dictionary<string, List<MediaData>> mediaDataWithOwner = new Dictionary<string, List<MediaData>>();

        private void ReiceiveMessage(MqttApplicationMessageReceivedEventArgs message)
        {
            try
            {
                Debug.Write(Encoding.UTF8.GetString(message.ApplicationMessage.Payload));
                try
                {
                    GenericEnvelope enveloppe = JsonSerializer.Deserialize<GenericEnvelope>(Encoding.UTF8.GetString(message.ApplicationMessage.Payload));

                    if (enveloppe.SenderId == clientId) return;
                    switch (enveloppe.MessageType)
                    {
                        case MessageType.ENVOIE_CATALOGUE:
                            {
                                EnvoieCatalogue enveloppeEnvoieCatalogue = JsonSerializer.Deserialize<EnvoieCatalogue>(enveloppe.EnvelopeJson);

                                // Mets à jour le catalogue de qqn ou le rajoute dans un dictionnaire
                                if (mediaDataWithOwner.ContainsKey(enveloppe.SenderId))
                                {
                                    mediaDataWithOwner[enveloppe.SenderId] = enveloppeEnvoieCatalogue.Content;
                                }
                                else
                                {
                                    mediaDataWithOwner.Add(enveloppe.SenderId, new List<MediaData>());
                                    mediaDataWithOwner[enveloppe.SenderId] = enveloppeEnvoieCatalogue.Content;
                                }

                                updateOnlineMusic(mediaDataWithOwner, enveloppe);

                                break;
                            }
                        case MessageType.DEMANDE_CATALOGUE:
                            {
                                EnvoieCatalogue envoieCatalogue = new EnvoieCatalogue();
                                envoieCatalogue.Content = list;
                                SendMessage(mqttClient, MessageType.ENVOIE_CATALOGUE, clientId, envoieCatalogue, topic);
                                break;
                            }
                        case MessageType.DEMANDE_FICHIER:
                            {
                                DemandeFichier enveloppeDemandeFichier = JsonSerializer.Deserialize<DemandeFichier>(enveloppe.EnvelopeJson);
                                EnvoieFichier envoiFichier = new EnvoieFichier();

                                envoiFichier.Content = Convert.ToBase64String(File.ReadAllBytes(dossierMusique + enveloppeDemandeFichier.FileName));

                                SendMessage(mqttClient, MessageType.ENVOIE_FICHIER, clientId, enveloppeDemandeFichier, enveloppe.SenderId);

                                break;
                            }
                        case MessageType.ENVOIE_FICHIER:
                            {
                                EnvoieFichier enveloppeEnvoieFichier = JsonSerializer.Deserialize<EnvoieFichier>(enveloppe.EnvelopeJson);
                                MediaData metaData = enveloppeEnvoieFichier.FileInfo;
                                byte[] file = Convert.FromBase64String(enveloppeEnvoieFichier.Content);

                                string path = dossierMusique + metaData.Title + metaData.Type;

                                File.WriteAllBytes(path, file);

                                MessageBox.Show("Téléchargement réussi", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                updateMusicList();

                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                
            }
            catch (Exception ex)
            {
                Trace.WriteLine("wuigdiuwqgdiuqgdigq    "+ex.ToString());
            }
        }

        private async void SendMessage(IMqttClient mqttClient, MessageType type, string senderId, IJsonSerializableMessage content, string topic)
        {
            GenericEnvelope enveloppe = new GenericEnvelope();
            enveloppe.SenderId = senderId;
            enveloppe.EnvelopeJson = content == null ? null : content.ToJson();
            enveloppe.MessageType = type;
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(JsonSerializer.Serialize(enveloppe))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            await mqttClient.PublishAsync(message);
            await Task.Delay(1000);
        }

        private async void SendData(string data)
        {
            // Créez le message à envoyer
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(data)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            // Envoyez le message
            mqttClient.PublishAsync(message);
            Console.WriteLine("Message sent successfully!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMessage(mqttClient, MessageType.DEMANDE_CATALOGUE, clientId, null, topic);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Crée une instance du formulaire à afficher
            Form form2 = new Form();

            form2.Height = 600;
            form2.Width = 900;

            form2.Controls.Add(listView2);

            listView2.Width = 850;
            listView2.Height = 400;

            // Affiche le formulaire
            form2.Show();

            SendMessage(mqttClient, MessageType.DEMANDE_CATALOGUE, clientId, null, topic);

        }

    // Implémentation de l'événement ItemActivate
    private void listView2_ItemActivate(object sender, EventArgs e)
        {
            // Récupérer l'élément sélectionné
            var selectedItem = listView2.SelectedItems[0];

            // Accéder aux données de l'élément, par exemple, le nom du fichier
            string fileName = selectedItem.SubItems[0].Text;  // Index de la colonne correspondante
            string fileArtist = selectedItem.SubItems[1].Text; // Index de la colonne correspondante
            string type = selectedItem.SubItems[2].Text;

            MessageBox.Show($"Vous avez cliqué sur : {fileName}, Artiste : {fileArtist}");

            SendMessage(mqttClient, MessageType.DEMANDE_FICHIER, clientId, null, selectedItem.SubItems[4].Text);
        }
    }
}