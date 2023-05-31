using dllRapportVisites;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System;
using System.Xml.Serialization;
using System.IO;

namespace GsbRapports
{
    /// <summary>
    /// Logique d'interaction pour VisiteursWindow.xaml
    /// </summary>
    public partial class VisiteursWindow : Window
    {
        private WebClient wb;
        private string site;
        private Secretaire laSecretaire;
        private Visiteur visiteurEnCours;

        public VisiteursWindow(Secretaire s, string site, WebClient wb)
        {
            InitializeComponent();
            this.wb = wb;
            this.laSecretaire = s;
            this.site = site;
            this.GetVisiteurs();
        }

        private void GetVisiteurs()
        {
            try
            {
                /* Réinitialisation du formulaire et du visiteur en cours */
                this.visiteurEnCours = null;
                this.txtNom.Text = null;
                this.txtPrenom.Text = null;
                this.txtAdresse.Text = null;
                this.txtCp.Text = null;
                this.txtVille.Text = null;
                this.dpEmbauche.SelectedDate = null;
                /* Par défaut on masque les formulaires */
                this.formulaireVisiteur.Visibility = Visibility.Collapsed;
                this.formulaireRapport.Visibility = Visibility.Collapsed;
                /* Construction de la requete get*/
                string url = this.site + "visiteurs?ticket=" + this.laSecretaire.getHashTicketMdp();
                /* récupération des données du serveur*/
                string data = this.wb.DownloadString(url);
                /* utilisation d'un objet dynamic pour séparer le ticket des visiteurs*/
                dynamic d = JsonConvert.DeserializeObject(data);
                string t = d.ticket;
                string visiteurs = d.visiteurs.ToString();
                /* convertit le json en liste de visiteurs */
                List<Visiteur> l = JsonConvert.DeserializeObject<List<Visiteur>>(visiteurs);
                /* On bind la listView à la liste des familles*/
                this.listView.ItemsSource = l;
                /*On met à jour la secrétaire avec le nouveau ticket*/
                this.laSecretaire.ticket = t;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                    MessageBox.Show(((HttpWebResponse)ex.Response).StatusDescription);
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                try
                {
                    /* On récupère le visiteur */
                    Visiteur visiteur = e.AddedItems[0] as Visiteur;
                    /* Construction de la requete get*/
                    string url = this.site + "visiteur?ticket=" + this.laSecretaire.getHashTicketMdp() + "&idVisiteur=" + visiteur.id;
                    /* récupération des données du serveur*/
                    string data = this.wb.DownloadString(url);
                    /* utilisation d'un objet dynamic pour séparer le ticket des visiteurs*/
                    dynamic d = JsonConvert.DeserializeObject(data);
                    string t = d.ticket;
                    /* convertit le json en liste de visiteurs */
                    this.visiteurEnCours = JsonConvert.DeserializeObject<Visiteur>(d.visiteur.ToString());
                    /*On met à jour la secrétaire avec le nouveau ticket*/
                    this.laSecretaire.ticket = t;
                    /* On met à jour les champs du formulaire */
                    this.txtNom.Text = this.visiteurEnCours.nom;
                    this.txtPrenom.Text = this.visiteurEnCours.prenom;
                    this.txtAdresse.Text = this.visiteurEnCours.adresse;
                    this.txtCp.Text = this.visiteurEnCours.cp;
                    this.txtVille.Text = this.visiteurEnCours.ville;
                    this.HideGridRapports();
                    /* On masque la date d'embauche car elle n'est pas modifiable */
                    this.lblDateEmbauche.Visibility = Visibility.Collapsed;
                    this.dpEmbauche.Visibility = Visibility.Collapsed;
                    /* On met les champs prénom et nom en lecture seule car non modifiables */
                    this.txtNom.IsReadOnly = true;
                    this.txtPrenom.IsReadOnly = true;
                    this.btnVoirRapports.Visibility = Visibility.Visible;
                    /* On affiche le formulaire des rapports */
                    this.formulaireRapport.Visibility = Visibility.Visible;
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse)
                        MessageBox.Show(((HttpWebResponse)ex.Response).StatusDescription);
                }
            }
        }

        private void btnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            /* Vérification que le formulaire est valide */
            if (this.FormulaireValide())
            {
                try
                {
                    string url = null;
                    /* Initialisation du visiteur */
                    /* Si visiteur null, c'est un nouveau visiteur */
                    if (this.visiteurEnCours == null)
                    {
                        /* Génération de l'id du visiteur */
                        string id = this.GenererId();
                        this.visiteurEnCours = new Visiteur(id, this.txtNom.Text, this.txtVille.Text, this.txtAdresse.Text, this.txtCp.Text, this.txtPrenom.Text, this.dpEmbauche.SelectedDate.Value);
                        /* Construction de la requete post*/
                        url = this.site +
                            "visiteurs?ticket=" + this.laSecretaire.getHashTicketMdp() +
                            "&idVisiteur=" + this.visiteurEnCours.id +
                            "&nom=" + this.visiteurEnCours.nom +
                            "&prenom=" + this.visiteurEnCours.prenom +
                            "&adresse=" + this.visiteurEnCours.adresse +
                            "&cp=" + this.visiteurEnCours.cp +
                            "&ville=" + this.visiteurEnCours.ville +
                            "&dateEmbauche=" + this.visiteurEnCours.dateEmbauche.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        /* Sinon c'est une maj d'un visiteur */
                        this.visiteurEnCours.nom = this.txtNom.Text;
                        this.visiteurEnCours.prenom = this.txtPrenom.Text;
                        this.visiteurEnCours.adresse = this.txtAdresse.Text;
                        this.visiteurEnCours.cp = this.txtCp.Text;
                        this.visiteurEnCours.ville = this.txtVille.Text;
                        /* Construction de la requete post*/
                        url = this.site +
                            "visiteur?ticket=" + this.laSecretaire.getHashTicketMdp() +
                            "&idVisiteur=" + this.visiteurEnCours.id +
                            "&adresse=" + this.visiteurEnCours.adresse +
                            "&cp=" + this.visiteurEnCours.cp +
                            "&ville=" + this.visiteurEnCours.ville;
                    }

                    /* Envoie du visiteur au serveur*/
                    string data = this.wb.UploadString(url, JsonConvert.SerializeObject(this.visiteurEnCours));
                    /*On met à jour la secrétaire avec le nouveau ticket*/
                    this.laSecretaire.ticket = data.Replace("\n", string.Empty);
                    /* Affichage confirmation */
                    MessageBox.Show("Enregistrement réussi !");
                    /* On recharge la liste des visiteurs */
                    this.GetVisiteurs();
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse)
                        MessageBox.Show(((HttpWebResponse)ex.Response).StatusDescription);
                }
            }
        }

        private void btnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            /* On affiche une popup de confirmation de la suppression */
            MessageBoxResult messageBoxResult = MessageBox.Show("Êtes-vous sûr de vouloir supprimer le visiteur " + this.visiteurEnCours.nom + " " + this.visiteurEnCours.prenom, "Confirmation de suppresion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    /* Construction de la requete delete*/
                    string url = this.site + "visiteur/supprimer?ticket=" + this.laSecretaire.getHashTicketMdp() + "&idVisiteur=" + this.visiteurEnCours.id;
                    /* Suppression du visiteur sur le serveur */
                    string data = this.wb.UploadString(url, string.Empty);
                    /*On met à jour la secrétaire avec le nouveau ticket*/
                    this.laSecretaire.ticket = data.Replace("\n", string.Empty);
                    /* Réinitialisation du formulaire et du visiteur en cours */
                    this.visiteurEnCours = null;
                    this.txtNom.Text = null;
                    this.txtPrenom.Text = null;
                    this.txtAdresse.Text = null;
                    this.txtCp.Text = null;
                    this.txtVille.Text = null;
                    this.dpEmbauche.SelectedDate = null;
                    /* Par défaut on masque les formulaires */
                    this.formulaireVisiteur.Visibility = Visibility.Collapsed;
                    this.formulaireRapport.Visibility = Visibility.Collapsed;
                    /* Affichage confirmation */
                    MessageBox.Show("Suppression réussie !");
                    /* On recharge la liste des visiteurs */
                    this.GetVisiteurs();
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse)
                        MessageBox.Show(((HttpWebResponse)ex.Response).StatusDescription);
                }
            }
        }

        private void btnAjouter_Click(object sender, RoutedEventArgs e)
        {
            /* Réinitialisation du formulaire et du visiteur en cours */
            this.visiteurEnCours = null;
            this.txtNom.Text = null;
            this.txtPrenom.Text = null;
            this.txtAdresse.Text = null;
            this.txtCp.Text = null;
            this.txtVille.Text = null;
            this.dpEmbauche.SelectedDate = null;
            /* On masque la grille pour afficher le formulaire */
            this.HideGridRapports(true);
            /* On affiche la date d'embauche pour un nouveau visiteur */
            this.lblDateEmbauche.Visibility = Visibility.Visible;
            this.dpEmbauche.Visibility = Visibility.Visible;
            /* On peut modifier le nom et rénom pour un nouveau visiteur */
            this.txtNom.IsReadOnly = false;
            this.txtPrenom.IsReadOnly = false;
            /* On masque le bouton pour voir les rapports et pour exporter */
            this.btnVoirRapports.Visibility = Visibility.Collapsed;
            this.btnExporterRapports.Visibility = Visibility.Collapsed;
            /* On masque le formulaire des rapports */
            this.formulaireRapport.Visibility = Visibility.Collapsed;
        }

        private void btnVoirRapports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /* Construction de la requete get*/
                string url = this.site +
                    "rapports?ticket=" + this.laSecretaire.getHashTicketMdp() +
                    "&idVisiteur=" + this.visiteurEnCours.id;

                /* Si une des dates est sélectionnée on les passe dans la requête */
                if (this.dpRapportDu.SelectedDate != null || this.dpRapportAu.SelectedDate != null)
                {
                    /* Si la date de début n'est pas fournie on va chercher à partir de la date minimum */
                    DateTime dateDebut = this.dpRapportDu.SelectedDate ?? DateTime.MinValue;
                    /* Si la date de fin n'est pas fournie on va chercher jusqu'à demain */
                    DateTime dateFin = this.dpRapportAu.SelectedDate ?? DateTime.Now.AddDays(1);

                    url += "&dateDebut=" + dateDebut.ToString("yyyy-MM-dd") +
                        "&dateFin=" + dateFin.ToString("yyyy-MM-dd");
                }

                /* récupération des données du serveur*/
                string data = this.wb.DownloadString(url);
                /* utilisation d'un objet dynamic pour séparer le ticket des rapports*/
                dynamic d = JsonConvert.DeserializeObject(data);
                string t = d.ticket;
                string rapports = d.rapports.ToString();
                /* convertit le json en liste de rapports */
                List<Rapport> l = JsonConvert.DeserializeObject<List<Rapport>>(rapports);
                /* On bind la datagrid à la liste des familles*/
                this.dgRapports.ItemsSource = l;
                /* On affiche la grille des rapports */
                this.ShowGridRapports();
                /*On met à jour la secrétaire avec le nouveau ticket*/
                this.laSecretaire.ticket = t;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                    MessageBox.Show(((HttpWebResponse)ex.Response).StatusDescription);
            }
        }

        private void btnRetourFicheClient_Click(object sender, RoutedEventArgs e)
        {
            this.HideGridRapports();
        }

        private void btnExporterRapports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /* Récupération des rapports */
                var rapports = this.dgRapports.ItemsSource as List<Rapport>;
                if (rapports != null)
                {
                    /* On affiche une popup pour sélectionner le dossier où sauvegarder le fichier XML */
                    using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                        /* On créé un serializer xml */
                        XmlSerializer serialiser = new XmlSerializer(typeof(List<Rapport>));
                        /* On génère le chemin du fichier à sauvegarder */
                        string filePath = Path.Combine(dialog.SelectedPath, this.visiteurEnCours.nom + " " + this.visiteurEnCours.prenom + " rapports.xml");
                        /* On créé un txt TextWriter for the serialiser to use */
                        TextWriter filestream = new StreamWriter(filePath);
                        /* On écrit le fichier */
                        serialiser.Serialize(filestream, rapports);
                        /* On ferme le fichier */
                        filestream.Close();
                        /* On affiche un message de confirmation */
                        MessageBox.Show("Fichier des rapports généré avec succès");
                    }
                }
            }
            catch (Exception ex)
            {
                /* On affiche l'erreur */
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private bool FormulaireValide()
        {
            if (string.IsNullOrEmpty(this.txtNom.Text))
            {
                MessageBox.Show("Le nom est obligatoire");
                return false;
            }

            if (string.IsNullOrEmpty(this.txtPrenom.Text))
            {
                MessageBox.Show("Le prénom est obligatoire");
                return false;
            }

            if (string.IsNullOrEmpty(this.txtVille.Text))
            {
                MessageBox.Show("La ville est obligatoire");
                return false;
            }

            if (string.IsNullOrEmpty(this.txtAdresse.Text))
            {
                MessageBox.Show("L'adresse est obligatoire");
                return false;
            }

            if (string.IsNullOrEmpty(this.txtCp.Text))
            {
                MessageBox.Show("Le code postal est obligatoire");
                return false;
            }

            if (this.dpEmbauche.Visibility == Visibility.Visible && this.dpEmbauche.SelectedDate == null)
            {
                MessageBox.Show("La date d'embauche est obligatoire");
                return false;
            }

            return true;
        }

        private string GenererId()
        {
            /* On génère un id random de 4 caractères */
            Random random = new Random();
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            string id = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
            var visiteurs = this.listView.ItemsSource as List<Visiteur>;
            if (visiteurs != null)
            {
                /* Tant qu'un visiteur possède déjà le même id, on regénère */
                while (visiteurs.Any(x => x.id == id))
                {
                    id = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
                }
            }

            return id;
        }

        private void ShowGridRapports()
        {
            /* On affiche la grille des rapports et on masque le formulaire */
            this.dgRapports.Visibility = Visibility.Visible;
            this.formulaireVisiteur.Visibility = Visibility.Collapsed;
            /* On affiche le bouton pour revenir sur le formulaire client */
            this.btnRetourFicheClient.Visibility = Visibility.Visible;
            /* On afficher le bouton exporter rapports */
            this.btnExporterRapports.Visibility = Visibility.Visible;
            /* On masque le bouton de suppression de visiteur */
            this.btnSupprimer.Visibility = Visibility.Collapsed;
            /* On affiche le formulaire des rapports */
            this.formulaireRapport.Visibility = Visibility.Visible;
        }

        private void HideGridRapports(bool isAjouterVisiteur = false)
        {
            /* On masque la grille des rapports au cas où si elle est affichée */
            this.dgRapports.Visibility = Visibility.Collapsed;
            /* On masque les bouton retour et exporter rapports au cas où si il sont affichés */
            this.btnRetourFicheClient.Visibility = Visibility.Collapsed;
            this.btnExporterRapports.Visibility = Visibility.Collapsed;
            /* On affiche le formulaire */
            this.formulaireVisiteur.Visibility = Visibility.Visible;

            /* Si on est dans le cas d'ajout d'un visiteur on masque le bouton pour supprimer */
            if (isAjouterVisiteur)
            {
                /* On masque le bouton de suppression de visiteur */
                this.btnSupprimer.Visibility = Visibility.Collapsed;
            }
            else
            {
                /* Sinon on affiche de formulaire rapport pour filter sur les dates */
                this.formulaireRapport.Visibility = Visibility.Visible;
                /* On affiche le bouton de suppression de visiteur */
                this.btnSupprimer.Visibility = Visibility.Visible;
            }
        }
    }
}
