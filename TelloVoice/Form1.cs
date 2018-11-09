using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Speech.Recognition;
using TelloLib;

namespace TelloVoice
{
    public partial class Form1 : Form
    {
        static SpeechRecognitionEngine reconhecedor;
        SpeechSynthesizer resposta = new SpeechSynthesizer();
        static CultureInfo culture = new CultureInfo("pt-BR");
        Color cor = Color.Green;
        Color novaCor;

       // TelloProxy proxy = new TelloProxy("192.168.10.1", 8889);

        public Form1()
        {
            InitializeComponent();

        }

        private void Gramatica()
        {
            lblStatus.Text = "Carregando a gramática...";
            led.Visible = false;
            Application.DoEvents();

            string[] listaPalavras = { "decolar", "pousar", "bateria", "foda-se" };
            string[] listaComandosRetos = { "frente", "tras", "traz", "subir", "descer", "cima", "baixo"};
            string[] listaComandosGiro = { "girar" };
            string[] listaComandosDiracao = { "para esquerda", "para direita"};
            string[] listaMedidas = { "centímetros", "metros" };

            string[] valores =new string[480];
            string[] graus = new string[360];



            for (int i=20; i < 500; i++)
            {
                valores[i-20] =(i).ToString();
            }

            for (int i = 1; i <= 360; i++)
            {
                graus[i-1] = (i).ToString();
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

            var gramaticaGraus = new Choices();
            gramaticaGraus.Add(graus);

            var gramaticaMedidas = new Choices();
            gramaticaMedidas.Add(listaMedidas);


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
            gbComandosGiro.Append(gramaticaGraus);
            gbComandosGiro.Append("graus");
            gbComandosGiro.Append(gramaticaDirecao);


            var gbComandosRetos = new GrammarBuilder();
            gbComandosRetos.Append(gramaticaComandosRetos);
            gbComandosRetos.Append(gramaticaNumeros);
            gbComandosRetos.Append(gramaticaMedidas);

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
                //resposta.Rate = 5;
                //resposta.SpeakAsync("Olá, Lucas. Pronto para se divertir?");
                //resposta.Rate = 3;
                //resposta.SpeakAsync("Estou pronta. Me diga o que fazer.");
                //resposta.Rate = 5;

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
            int valor = 0;
            string[] split;

            if (!string.IsNullOrEmpty(e.Result.Text))
            {
                try
                {
                    if (frase.IndexOf("girar") > -1)
                    {
                        split = frase.Split(' ');
                        valor = int.Parse(split[1]);
                        string direcao = "";

                        if (frase.IndexOf("esquerda") > -1)
                            direcao = "e";
                        else
                            direcao = "d";

                        resposta.Speak("Drone girando");
                        Tello.Girar(valor, direcao);
                    }
                    else if (frase.IndexOf("frente") > -1)
                    {
                        split = frase.Split(' ');
                        valor = int.Parse(split[1]);

                        if (frase.IndexOf("centímetros")==-1)
                        {
                            valor = valor * 100;
                        }
                        resposta.Speak("Drone para frente");
                        Tello.Frente(valor);


                    }
                    if (frase.IndexOf("tras") > -1 || frase.IndexOf("traz")>-1)
                    {
                        split = frase.Split(' ');
                        valor = int.Parse(split[1]);
                        if (frase.IndexOf("centímetros") == -1)
                        {
                            valor = valor * 100;
                        }

                        Tello.Tras(valor);
                        resposta.Speak("Drone para tras");
                    }
                    if (frase.IndexOf("cima") > -1 || frase.IndexOf("subir") > -1)
                    {
                        split = frase.Split(' ');
                        valor = int.Parse(split[1]);

                        if (frase.IndexOf("centímetros") == -1)
                        {
                            valor = valor * 100;
                        }

                        Tello.Subir(valor);
                        resposta.Speak("Drone para cima");
                    }
                    if (frase.IndexOf("baixo") > -1 || frase.IndexOf("descer") > -1)
                    {
                        split = frase.Split(' ');
                        valor = int.Parse(split[1]);

                        if (frase.IndexOf("centímetros") == -1)
                        {
                            valor = valor * 100;
                        }

                        Tello.Descer(valor);
                        resposta.Speak("Drone para baixo");
                    }
                    else
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

                                    Tello.takeOff();
                                    break;
                                }
                          
                            case "pousar":
                                {
                                    Tello.land();
                                    resposta.Speak("Ok, pousando o drone.");

                                    break;
                                }
                            case "cambalhota":
                                {
                                    resposta.Speak("Girando");
                                    break;
                                }
                          

                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }

          
            led.Visible = true;

        }

        private void Init()
        {
            lblStatus.Text = "Carregando gramática... ";
            Application.DoEvents();
            resposta.Volume = 100;
            Gramatica();


            IniciarTello();
        }

        private void IniciarTello()
        {
            //subscribe to Tello connection events
            Tello.onConnection += (Tello.ConnectionState newState) =>
            {
                if (newState == Tello.ConnectionState.Connected)
                {
                    //When connected update maxHeight to 5 meters
                    Tello.setMaxHeight(15);
                }

                lblStatus.Invoke((MethodInvoker)delegate {
                    lblStatus.Text = "Tello Conectado";
                });
                
            };


            Tello.onVideoData += Tello_onVideoData;

            Tello.onUpdate += Tello_onUpdate;

            Tello.startConnecting();


        }

        private void Tello_onVideoData(byte[] data)
        {
            //byte[] data = new byte[102400];
            //int recv = server.ReceiveFrom(data, ref Remote);
            MemoryStream ms = new MemoryStream(data);
            Image returnImage = Image.FromStream(ms);

            pictureBox2.Image = returnImage;
           
        }

        private void Tello_onUpdate(int cmdId)
        {

            if (cmdId == 86)
            {

                lblStatus.Invoke((MethodInvoker)delegate {
                     lblDados.Text = "Bateria: " + Tello.state.batteryPercentage.ToString() + "%\n";
                     lblDados.Text += "Temperatura: " + Tello.state.temperatureHeight.ToString() + " Graus\n";
                     lblDados.Text += "Altura: " + Tello.state.height.ToString() + "m\n";
                     lblDados.Text += "Velocidade: " + Tello.state.flySpeed.ToString() + "cm/s\n";
                });

               
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            lblDados.Text = "";
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

        
    }
}
