using dllRapportVisites;
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
    /// Logique d'interaction pour ajoutFamilleWindow.xaml
    /// </summary>
    public partial class ajoutFamilleWindow : Window
    {
        private Secretaire laSecretaire;
        private string site;
        private WebClient wb;
        public ajoutFamilleWindow(Secretaire s, string site, WebClient wb)
        {
            InitializeComponent();
            this.wb = wb;
            this.laSecretaire = s;
            this.site = site;
        }

        private void btnValider_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = this.site + "familles";
                NameValueCollection parametres = new NameValueCollection();
                parametres.Add("ticket", this.laSecretaire.getHashTicketMdp());
                parametres.Add("idFamille", this.txtId.Text);
                parametres.Add("libelle", this.txtLibelle.Text);
                byte[] tabByte = wb.UploadValues(url, "POST", parametres);
                string reponse = UnicodeEncoding.UTF8.GetString(tabByte);
                reponse = reponse.Substring(2);
                this.laSecretaire.ticket = reponse;
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
