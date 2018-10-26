using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Speech.Recognition;


namespace TelloVoice
{
    public partial class Form1 : Form
    {
        static SpeechRecognitionEngine reconhecedor;
        SpeechSynthesizer resposta = new SpeechSynthesizer();
        static CultureInfo culture = new CultureInfo("pt-BR");
        Color cor = Color.Green;
        Color novaCor;

        TelloProxy proxy = new TelloProxy("192.168.10.1", 8889);

        public Form1()
        {
            InitializeComponent();

        }

        private void Gramatica()
        {
            lblStatus.Text = "Carregando a gramática...";
            led.Visible = false;
            Application.DoEvents();

            string[] listaPalavras = { "decolar", "pousar", "bateria", "foda-se", "conectar" };
            string[] listaComandosRetos = { "para frente", "para tras", "subir", "descer", "para cima", "para baixo"};
            string[] listaComandosGiro = { "girar" };
            string[] listaComandosDiracao = { "para esquerda", "para direita", "a esquerda", "a direita" };
            string[] valores =new string[480];

            
            for (int i=20; i < 500; i++)
            {
                valores[i-20] =(i).ToString();
            }


            try
            {
                reconhecedor = new SpeechRecognitionEngine(culture);
            }
            catch (Exception ex)
            {
                throw;
            }


            reconhecedor.RequestRecognizerUpdate();

            reconhecedor.SpeechRecognized += Reconhecedor_SpeechRecognized;
            reconhecedor.SpeechHypothesized += Reconhecedor_SpeechHypothesized;
            reconhecedor.SpeechRecognitionRejected += Reconhecedor_SpeechRecognitionRejected;

            reconhecedor.AudioLevelUpdated += Reconhecedor_AudioLevelUpdated;



            var gramaticaNumeros = new Choices();
            gramaticaNumeros.Add(valores);

            var gramaticaDirecao = new Choices();
            gramaticaDirecao.Add(listaComandosDiracao);

            var gramatica = new Choices();
            gramatica.Add(listaPalavras);
            var gb = new GrammarBuilder();
            gb.Append(gramatica);

            var gramaticaComandosGiro = new Choices();
            gramaticaComandosGiro.Add(listaComandosGiro);

            var gramaticaComandosRetos = new Choices();
            gramaticaComandosRetos.Add(listaComandosRetos);


            var gbComandosGiro = new GrammarBuilder();
            gbComandosGiro.Append(gramaticaComandosGiro);
            gbComandosGiro.Append(gramaticaNumeros);
            gbComandosGiro.Append("graus");
            gbComandosGiro.Append(gramaticaDirecao);


            var gbComandosRetos = new GrammarBuilder();
            gbComandosRetos.Append(gramaticaComandosRetos);
            gbComandosRetos.Append(gramaticaNumeros);
            gbComandosRetos.Append("centímetros");

            reconhecedor.LoadGrammarCompleted += Reconhecedor_LoadGrammarCompleted;



            var g = new Grammar(gb);
            var gComandos = new Grammar(gbComandosRetos);
            var gComandosGiro = new Grammar(gbComandosGiro);
            gComandosGiro.Name = "giro";
            reconhecedor.LoadGrammarAsync(g);
            reconhecedor.LoadGrammarAsync(gComandos);
            reconhecedor.LoadGrammarAsync(gComandosGiro);



            reconhecedor.SetInputToDefaultAudioDevice();
            resposta.SetOutputToDefaultAudioDevice();

            reconhecedor.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void Reconhecedor_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
            if (e.Grammar.Name == "giro")
            {
                resposta.Rate = 5;
                resposta.SpeakAsync("Olá, Lucas. Pronto para se divertir?");
                resposta.Rate = 3;
                resposta.SpeakAsync("Estou pronta. Me diga o que fazer.");
                resposta.Rate = 5;

                lblStatus.Text = "Aguardando comandos: ";
                led.Visible = true;
                Application.DoEvents();
            }
        }

        private void Reconhecedor_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            resposta.Speak("Não entendi. Pode repetir?");
            led.BackColor = Color.Red;
            timer1.Enabled = false;
            led.Visible = true;
        }

        private void Reconhecedor_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            novaCor = ControlPaint.Dark(cor, e.AudioLevel);
            pbCor.Width = e.AudioLevel * 2;
        }

        private void Reconhecedor_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            led.BackColor = Color.LimeGreen;
            timer1.Enabled = true;
            led.Visible = true;
        }

        private void Reconhecedor_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string frase = e.Result.Text;
            led.Visible = true;
            lblStatus.Text = "Comando recebido: " + frase;
            Application.DoEvents();

 

            if (!string.IsNullOrEmpty(e.Result.Text))
            {
                try
                {
                    switch (frase)
                    {
                        case "foda-se":
                            {
                                resposta.Speak("Foda-se você, seu babaca.");
                                
                                break;
                            }
                        case "decolar":
                            {
                                resposta.Speak("Decolando...");
                                var voando = proxy.Decolar();
                                if (voando)
                                {
                                    resposta.Speak("Drone em voo");
                                }
                                else
                                {
                                    resposta.Speak("Não foi possível decolar.");
                                }
                                break;
                            }
                        case "conectar":
                            {
                                resposta.Speak("Conectando...");
                                var conectado = proxy.Connect();
                                if (conectado)
                                {
                                    resposta.Speak("Estamos prontos");
                                }
                                else
                                {
                                    resposta.Speak("Não foi possível conectar.");
                                }
                                break;
                            }
                        case "pousar":
                            {
                                resposta.Speak("Ok, pousando o drone.");

                                break;
                            }
                        case "cambalhota":
                            {
                                resposta.Speak("Girando");
                                break;
                            }
                        case "frente":
                            {
                                resposta.Speak("Para frente 50 centímetros.");
                                break;
                            }
                        case "tras":
                            {
                                resposta.Speak("Para trás 50 centímetros.");
                                break;
                            }
                        case "girar":
                            {
                                break;
                            }
                        case "bateria":
                            {
                               var resp= proxy.GetBattery();
                                if (resp !=0)
                                {
                                    resposta.SpeakAsync("Bateria com " + resp + "%");
                                }
                                break;
                            }
                        case "8":
                            {
                                break;
                            }
                        case "9":
                            {

                                break;
                            }

                    }
           

                }
                catch (Exception ex)
                {

                }
            }

            timer1.Enabled = false;
            led.Visible = true;

        }

        private void Init()
        {
            lblStatus.Text = "Carregando gramática... ";
            Application.DoEvents();

            resposta.Volume = 100;
            Gramatica();


        }


        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            Init();
        }

        private void pbVol_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            led.Visible = !led.Visible;
            Application.DoEvents();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //var wrapper = SdkWrapper.Instance;

            //TelloSdkStandard.actions.Action action = null;
            //var resp = wrapper.BaseActions.QueryBattery().Execute();
            //if (resp == SdkWrapper.SdkReponses.OK)
            //{
            //    lblBateria.Text = "Bateria:" +  wrapper.BaseActions.QueryBattery().ServerResponse +"%";
            //}
        }

    }
}
