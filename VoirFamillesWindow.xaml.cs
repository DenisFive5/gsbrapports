using dllRapportVisites;
using System;
using System.Collections.Generic;
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
using dllRapportVisites;
using Newtonsoft.Json;

namespace GsbRapports
{
    /// <summary>
    /// Logique d'interaction pour VoirFamillesWindow.xaml
    /// </summary>
    public partial class VoirFamillesWindow : Window
    {
        private WebClient wb;
        private string site;
       
        private Secretaire laSecretaire;
        public VoirFamillesWindow(Secretaire s, string site, WebClient wb)
        {
            InitializeComponent();
            this.wb = wb;
            this.laSecretaire = s;
            this.site = site;
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
            this.dtg.ItemsSource = l;
            /*On met à jour la secrétaire avec le nouveau ticket*/
            this.laSecretaire.ticket = t;
        }
    }
}
