using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoboPiedade
{
    public partial class SalvarDadosUrbas : Form
    {
        #region variaveis
        public EtapaConsulta EtapaConsulta { get; set; }
        System.Windows.Forms.Timer time = new System.Windows.Forms.Timer();
        IDictionary<string, string> valuesSelectBairro = new Dictionary<string, string>();
        List<string> SaidaTerminalDiasUteis;
        List<string> SaidaTerminalSabado;
        List<string> SaidaTerminalDomingo;
        List<string> SaidaBairroDiasUteis;
        List<string> SaidaBairroSabado;
        List<string> SaidaBairroDomingo;
        List<string> SaidaTerminalRuas;
        List<string> SaidaBairroRuas;
        List<string> Observacoes;
        string Horarios;
        string NomeBairro;
        string DataUltimaAtualizacao;
        string Ruas;
        string srcMapa;
        bool getValuesSelectBairro = true;
        bool ultimoBairro = false;
        int contatorIdBairro = 1;
        Bairros bairro;
        Horarios horario;
        Observacoes obsDb;
        Itinerarios itinerario;
        Mapas mapa;
        #endregion

        public SalvarDadosUrbas()
        {
            InitializeComponent();
            EtapaConsulta = EtapaConsulta.Navegando;

            time.Tick += new EventHandler(time_Tick);
            time.Interval = 2000;
            time.Start();
        }

        private void time_Tick(object sender, EventArgs e)
        {
            time.Stop();
            switch (this.EtapaConsulta)
            {
                case EtapaConsulta.Navegando:

                    Helper.SetBrowserEmulation(AppDomain.CurrentDomain.FriendlyName, IE.IE10);
                    if (!ultimoBairro)
                        webBrowser.Navigate("http://www.transpiedade.com.br/cl/tabelasdehorarios.html");
                    else
                        webBrowser.Navigate("http://www.transpiedade.com.br/cl/mapasdelinhas.html");
                    EtapaConsulta = EtapaConsulta.SelecionarBairro;
                    break;
                case EtapaConsulta.SelecionarBairro:
                    var element = webBrowser.Document.GetElementById("selLinhas");

                    ResetVariables();
                    if (getValuesSelectBairro)
                        GetAllValuesSelectBairro(element);
                    string value = string.Empty;
                    if (!ultimoBairro)
                        value = GetAllValuesSelectBairro();
                    else
                        value = GetAllValuesSelectBairroMapas();
                    element.SetAttribute("value", value);
                    element.InvokeMember("onchange");
                    EtapaConsulta = EtapaConsulta.RecolherDados;
                    break;
                case EtapaConsulta.RecolherDados:

                    if (!ultimoBairro)
                    {
                        NomeBairro = CamelCase(getNomeBairro(webBrowser.Document.GetElementById("selLinhas")).Split('-')[1].Trim());

                        DataUltimaAtualizacao = TrataDataUltimaAtualizacao(webBrowser.Document.GetElementById("ultalt").InnerText);

                        Horarios = webBrowser.Document.GetElementById("psduteis").InnerText;
                        SaidaTerminalDiasUteis = InsertDataList(Horarios, SaidaTerminalDiasUteis);

                        Horarios = webBrowser.Document.GetElementById("pssabados").InnerText;
                        SaidaTerminalSabado = InsertDataList(Horarios, SaidaTerminalSabado);

                        Horarios = webBrowser.Document.GetElementById("psdomingos").InnerText;
                        SaidaTerminalDomingo = InsertDataList(Horarios, SaidaTerminalDomingo);

                        Horarios = webBrowser.Document.GetElementById("prduteis").InnerText;
                        SaidaBairroDiasUteis = InsertDataList(Horarios, SaidaBairroDiasUteis);

                        Horarios = webBrowser.Document.GetElementById("prsabados").InnerText;
                        SaidaBairroSabado = InsertDataList(Horarios, SaidaBairroSabado);

                        Horarios = webBrowser.Document.GetElementById("prdomingos").InnerText;
                        SaidaBairroDomingo = InsertDataList(Horarios, SaidaBairroDomingo);

                        Ruas = webBrowser.Document.GetElementById("pruas").InnerText;
                        SaidaTerminalRuas = TrataRua(Ruas, SaidaTerminalRuas);

                        Observacoes = InsertObservacoes(webBrowser.Document.GetElementById("pobs").InnerText);
                    }
                    else
                    {
                        var iframe = webBrowser.Document.GetElementsByTagName("iframe");
                        if(iframe.Count > 0)
                            srcMapa = GetAttribute(iframe[0]);
                    }

                    EtapaConsulta = EtapaConsulta.InserirDados;
                    break;
                case EtapaConsulta.InserirDados:
                    if (!ultimoBairro)
                    {
                        InsertBairroDb();
                        InsertHorarioDb();
                        InsertObservacoesDb();
                        InsertItinerarioDb();
                        if (valuesSelectBairro.Last().Value.Equals("s"))
                        {
                            ultimoBairro = true;
                            EtapaConsulta = EtapaConsulta.Navegando;
                        }
                        else
                            EtapaConsulta = EtapaConsulta.SelecionarBairro;

                    }
                    else
                    {
                        InsertMapaDb();
                        EtapaConsulta = EtapaConsulta.SelecionarBairro;
                    }
                    break;
            }
            if (contatorIdBairro <= 34)
                time.Start();
            else
                MessageBox.Show("CABO");
        }

        private void ResetVariables()
        {
            SaidaTerminalDiasUteis = new List<string>();
            SaidaTerminalSabado = new List<string>();
            SaidaTerminalDomingo = new List<string>();
            SaidaBairroDiasUteis = new List<string>();
            SaidaBairroSabado = new List<string>();
            SaidaBairroDomingo = new List<string>();
            SaidaTerminalRuas = new List<string>();
            SaidaBairroRuas = new List<string>();
            Observacoes = new List<string>();
            Horarios = string.Empty;
            NomeBairro = string.Empty;
            DataUltimaAtualizacao = string.Empty;
            Ruas = string.Empty;
            srcMapa = string.Empty;
            bairro = new Bairros();
            horario = new Horarios();
            obsDb = new Observacoes();
            itinerario = new Itinerarios();
            mapa = new Mapas();
        }

        private void GetAllValuesSelectBairro(HtmlElement el)
        {
            var AllItems = el.All;
            foreach (HtmlElement item in AllItems)
            {
                if (!item.InnerHtml.Contains("SELECIONE"))
                    valuesSelectBairro.Add(item.GetAttribute("value"), "n");
            }
            getValuesSelectBairro = false;
        }

        private string GetAttribute(HtmlElement el)
        {
            string src = string.Empty;
            src = el.GetAttribute("src");
            return src;
        }

        private string GetAllValuesSelectBairro()
        {
            string value = string.Empty;
            foreach (var item in valuesSelectBairro)
            {
                if (item.Value.Equals("n"))
                {
                    value = item.Key;
                    break;
                }
            }

            valuesSelectBairro[value] = "s";
            return value;
        }

        private string GetAllValuesSelectBairroMapas()
        {
            string value = string.Empty;
            foreach (var item in valuesSelectBairro)
            {
                if (item.Value.Equals("s"))
                {
                    value = item.Key;
                    break;
                }
            }

            valuesSelectBairro[value] = "n";
            return value;
        }

        private string CamelCase(string nome)
        {
            string[] auxNome = nome.Split(' ');
            string retorno = string.Empty;
            for (int i = 0; i < auxNome.Count(); i++)
            {
                if (auxNome[i] == "DE" || auxNome[i] == "DO")
                    retorno += auxNome[i].ToLower() + " ";
                else if (auxNome[i] == "SIG")
                    retorno += auxNome[i] + " ";
                else
                    retorno += auxNome[i].Substring(0, 1).ToUpper() + auxNome[i].Substring(1).ToLower() + " ";
            }
            return retorno.Trim();
        }

        private string TrataDataUltimaAtualizacao(string data)
        {
            string[] aux = data.Split(' ');
            return aux[3];
        }

        private List<string> TrataRua(string rua, List<string> list)
        {
            if (rua != null && !string.IsNullOrWhiteSpace(rua) && !rua.Contains("script"))
            {
                string[] aux;
                aux = rua.Replace("\r", "").Replace("\n", " ").Replace("\"", "").Replace("rua", "Rua").Split(new string[] { "-", ":" }, StringSplitOptions.None);
                bool bairro = false;

                for (int i = 0; i < aux.Count(); i++)
                {
                    if (i != 0)
                    {
                        if (!bairro)
                        {
                            if (aux[i].Contains("Bairro"))
                            {
                                int index = aux[i].IndexOf("Bairro");
                                if (!string.IsNullOrWhiteSpace(aux[i]))
                                {
                                    string aux2 = aux[i].Substring(0, index).Trim();
                                    if (!string.IsNullOrWhiteSpace(aux2))
                                        SaidaTerminalRuas.Add(aux2);
                                }

                                bairro = true;
                            }
                            else
                                if (!string.IsNullOrWhiteSpace(aux[i]))
                                SaidaTerminalRuas.Add(aux[i].Trim());
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(aux[i]))
                                SaidaBairroRuas.Add(aux[i].Trim());
                        }
                    }
                }
            }
            return list;
        }

        private List<string> InsertObservacoes(string obs)
        {
            if (!string.IsNullOrWhiteSpace(obs) && !obs.Contains("script"))
            {
                string[] aux;
                aux = obs.Replace("\r", "").Split('\n');
                foreach (var item in aux)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        Observacoes.Add(TrataObservacao(item).Trim());
                }
            }
            return Observacoes;
        }

        private string TrataObservacao(string obs)
        {
            string retorno = string.Empty;
            if (obs.First().ToString() == "-")
            {
                retorno = obs.Remove(0, 2).Trim().First().ToString().ToUpper() + obs.Substring(3);
            }
            else
            {
                retorno = obs.First().ToString().ToUpper() + obs.Substring(1);
            }

            retorno = TrataObservacaoPorBairro(retorno);

            return retorno;
        }

        private string TrataObservacaoPorBairro(string obs)
        {
            string retorno = obs;
            if (obs.Contains("SÁBADOS, DOMINGOS E FERIADOS - RETORNO (BAIRRO - CENTRO) PELA RUA JOÃO STUKAS"))
            {
                retorno = obs.First().ToString().ToUpper() + obs.Substring(1).ToLower().Replace("joão", "João").Replace("stukas", "Stukas").Trim();
            }
            if (obs.Contains("ARTICULADO"))
            {
                retorno = obs.Replace("ARTICULADO", "Articulado");
            }
            if (obs.Contains("*"))
            {
                retorno = obs.Replace("*", "");
            }
            if (obs.Contains("FACECLA"))
            {
                retorno = obs.Replace("FACECLA", "Facecla").Replace("DIAS ÚTEIS", "Dias Úteis");
            }
            if (obs.Contains("ERCE"))
            {
                retorno = obs.Replace("ERCE", "Erce").Replace("HAVAN", "Havan");
            }
            if (obs.Contains("PLATAFORMA DA LINHA"))
            {
                retorno = obs.First().ToString().ToUpper() + obs.Substring(1).ToLower().Replace("ferrari", "Ferrari").Replace("guabiroba", "Guabiroba").Replace("sig combibloc", "SIG Combibloc").Replace("plataforma", "Plataforma");
            }
            if (obs.Contains("REGIÃO DOS PAULISTAS"))
            {
                retorno = obs.First().ToString().ToUpper() + obs.Substring(1).ToLower().Replace("paulistas", "Paulistas");
            }
            if (obs.Contains("VAI SOMENTE ATÉ A GARAGEM"))
            {
                retorno = obs.First().ToString().ToUpper() + obs.Substring(1).ToLower();
            }
            if (obs.Contains("PASSAM PELO SALGADINHO"))
            {
                retorno = obs.First().ToString().ToUpper() + obs.Substring(1).ToLower().Replace("salgadinho", "Salgadinho").Replace("pianaro", "Pianaro");
            }
            if (obs.Contains("LEMBRAMOS QUE OS"))
            {
                retorno = obs.First().ToString().ToUpper() + obs.Substring(1).ToLower();
            }
            return retorno;
        }

        private List<string> InsertDataList(string horarios, List<string> list)
        {
            string[] auxHorarios;

            if (horarios.Contains("\r"))
            {
                auxHorarios = horarios.Replace("\n", "").Replace("*", "").Replace("(", "").Replace(")", "").Replace("-", "").Split('\r');
                foreach (var item in auxHorarios)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        list.Add(item.Trim().Substring(0, 5));
                }
            }
            else
            {
                auxHorarios = horarios.Replace("*", "").Replace("(", "").Replace(")", "").Replace("-", "").Split('/');
                foreach (var item in auxHorarios)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        if (item != "NÃO OPERA")
                            if (item.Length > 7)
                                list.Add(item.Substring(0, 6).Trim());
                            else
                                list.Add(item.Trim());
                    }
                }
            }
            return list;
        }

        private string getNomeBairro(HtmlElement el)
        {
            string retorno = string.Empty;
            dynamic dom = el.DomElement;
            int index = (int)dom.selectedIndex();
            if (index != -1)
            {
                retorno = el.Children[index].InnerText;
            }
            return retorno;
        }

        private void InsertBairroDb()
        {
            if (!string.IsNullOrWhiteSpace(NomeBairro))
            {
                UrBUSEntities db = new UrBUSEntities();
                bairro.NomeBairro = NomeBairro;
                bairro.DataUltimaAtualizacao = Convert.ToDateTime(DataUltimaAtualizacao);
                db.Bairros.Add(bairro);
                db.SaveChanges();
            }

        }

        private void InsertHorarioDb()
        {
            if (!SaidaTerminalDiasUteis.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();
                foreach (var item in SaidaTerminalDiasUteis)
                {
                    horario.TipoDia = (int)TipoDia.DiaUtil;
                    horario.TipoSaida = (int)TipoSaida.Terminal;
                    horario.Hora = item;
                    horario.IdBairro = bairro.IdBairro;
                    db.Horarios.Add(horario);
                    db.SaveChanges();
                }
            }

            if (!SaidaTerminalSabado.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();
                foreach (var item in SaidaTerminalSabado)
                {
                    horario.TipoDia = (int)TipoDia.Sabado;
                    horario.TipoSaida = (int)TipoSaida.Terminal;
                    horario.Hora = item;
                    horario.IdBairro = bairro.IdBairro;
                    db.Horarios.Add(horario);
                    db.SaveChanges();
                }
            }

            if (!SaidaTerminalDomingo.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();
                foreach (var item in SaidaTerminalDomingo)
                {
                    horario.TipoDia = (int)TipoDia.Domingo;
                    horario.TipoSaida = (int)TipoSaida.Terminal;
                    horario.Hora = item;
                    horario.IdBairro = bairro.IdBairro;
                    db.Horarios.Add(horario);
                    db.SaveChanges();
                }
            }

            if (!SaidaBairroDiasUteis.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();
                foreach (var item in SaidaBairroDiasUteis)
                {
                    horario.TipoDia = (int)TipoDia.DiaUtil;
                    horario.TipoSaida = (int)TipoSaida.Bairro;
                    horario.Hora = item;
                    horario.IdBairro = bairro.IdBairro;
                    db.Horarios.Add(horario);
                    db.SaveChanges();
                }
            }

            if (!SaidaBairroSabado.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();
                foreach (var item in SaidaBairroSabado)
                {
                    horario.TipoDia = (int)TipoDia.Sabado;
                    horario.TipoSaida = (int)TipoSaida.Bairro;
                    horario.Hora = item;
                    horario.IdBairro = bairro.IdBairro;
                    db.Horarios.Add(horario);
                    db.SaveChanges();
                }
            }

            if (!SaidaBairroDomingo.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();
                foreach (var item in SaidaBairroDomingo)
                {
                    horario.TipoDia = (int)TipoDia.Domingo;
                    horario.TipoSaida = (int)TipoSaida.Bairro;
                    horario.Hora = item;
                    horario.IdBairro = bairro.IdBairro;
                    db.Horarios.Add(horario);
                    db.SaveChanges();
                }
            }
        }

        private void InsertObservacoesDb()
        {
            if (!Observacoes.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();

                foreach (var item in Observacoes)
                {
                    obsDb.Observacao = item;
                    obsDb.IdBairro = bairro.IdBairro;
                    db.Observacoes.Add(obsDb);
                    db.SaveChanges();
                }
            }
        }

        private void InsertItinerarioDb()
        {
            if (!SaidaTerminalRuas.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();

                foreach (var item in SaidaTerminalRuas)
                {
                    itinerario.Rua = item;
                    itinerario.TipoSaida = (int)TipoSaida.Terminal;
                    itinerario.IdBairro = bairro.IdBairro;
                    db.Itinerarios.Add(itinerario);
                    db.SaveChanges();
                }
            }

            if (!SaidaBairroRuas.isListNullOrEmpty())
            {
                UrBUSEntities db = new UrBUSEntities();

                foreach (var item in SaidaBairroRuas)
                {
                    itinerario.Rua = item;
                    itinerario.TipoSaida = (int)TipoSaida.Bairro;
                    itinerario.IdBairro = bairro.IdBairro;
                    db.Itinerarios.Add(itinerario);
                    db.SaveChanges();
                }
            }
        }

        private void InsertMapaDb()
        {
            if (!string.IsNullOrWhiteSpace(srcMapa))
            {
                UrBUSEntities db = new UrBUSEntities();
                mapa.LinkMapa = srcMapa;
                mapa.IdBairro = contatorIdBairro;
                db.Mapas.Add(mapa);
                db.SaveChanges();
            }
            contatorIdBairro++;
        }
    }
}
