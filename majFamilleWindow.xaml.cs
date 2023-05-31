using dllRapportVisites;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GsbRapports
{
    /// <summary>
    /// Logique d'interaction pour majFamilleWindow.xaml
    /// </summary>
    public partial class majFamilleWindow : Window
    {
        private Secretaire laSecretaire;
        private string site;
        private WebClient wb;
        public majFamilleWindow(Secretaire s, string site, WebClient wb)
        {
            InitializeComponent();
            this.wb = wb;
            this.laSecretaire = s;
            this.site = site;
            try
            {
                /* Construction de la requete get*/
                string url = this.site + "familles?ticket=" + this.laSecretaire.getHashTicketMdp();
                /* récupération des données du serveur*/
                string data = this.wb.DownloadString(url);
                /* utilisation d'un objet dynamic pour séparer le ticket des familles*/
                dynamic d = JsonConvert.DeserializeObject(data);
                string t = d.ticket;
                string familles = d.familles.ToString();
                /* convertit le json en liste de familles */
                List<Famille> l = JsonConvert.DeserializeObject<List<Famille>>(familles);
                /* On bind le datagrid à la liste des familles*/
                this.cmbFamille.ItemsSource = l;
                /* sélectionne le champ à afficher*/
                this.cmbFamille.DisplayMemberPath = "libelle";
                /*On met à jour la secrétaire avec le nouveau ticket*/
                this.laSecretaire.ticket = t;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                    MessageBox.Show(((HttpWebResponse)ex.Response).StatusCode.ToString());

            }
        }

        private void btnValider_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = this.site + "famille";
                NameValueCollection parametres = new NameValueCollection();
                parametres.Add("ticket", this.laSecretaire.getHashTicketMdp() );
                parametres.Add("idFamille", ((Famille)this.cmbFamille.SelectedItem).id);
                parametres.Add("libelle", this.txtLibFamille.Text);
                byte[] tabByte = wb.UploadValues(url, "POST", parametres);
                string reponse = UnicodeEncoding.UTF8.GetString(tabByte);
                reponse = reponse.Substring(2);
                this.laSecretaire.ticket = reponse;
               // MessageBox.Show(reponse);
                this.Close();
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                    MessageBox.Show(((HttpWebResponse)ex.Response).StatusCode.ToString());

            }
        }
    }
}
