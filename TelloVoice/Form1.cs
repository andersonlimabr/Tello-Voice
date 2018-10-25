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
using TelloSdkStandard;

namespace TelloVoice
{
    public partial class Form1 : Form
    {
        static SpeechRecognitionEngine reconhecedor;
        SpeechSynthesizer resposta = new SpeechSynthesizer();
        static CultureInfo culture = new CultureInfo("pt-BR");

        Color cor = Color.Green;
        Color novaCor;

        public Form1()
        {
            InitializeComponent();

        }

        private void Gramatica()
        {
            lblStatus.Text = "Carregando a gramática...";
            led.Visible = false;
            Application.DoEvents();

            string[] listaPalavras = { "decolar", "pousar", "girar", "frente", "tras", "bateria", "foda-se", "cambalhota" };

            try
            {
                reconhecedor = new SpeechRecognitionEngine(culture);
            }
            catch (Exception ex)
            {
                throw;
            }

            var gramatica = new Choices();

            gramatica.Add(listaPalavras);

            var gb = new GrammarBuilder();
            gb.Append(gramatica);

            var g = new Grammar(gb);


            reconhecedor.RequestRecognizerUpdate();
            reconhecedor.LoadGrammarAsync(g);
            reconhecedor.SpeechRecognized += Reconhecedor_SpeechRecognized;
            reconhecedor.SpeechHypothesized += Reconhecedor_SpeechHypothesized;
            reconhecedor.SpeechRecognitionRejected += Reconhecedor_SpeechRecognitionRejected;

            reconhecedor.AudioLevelUpdated += Reconhecedor_AudioLevelUpdated;


            reconhecedor.SetInputToDefaultAudioDevice();
            resposta.SetOutputToDefaultAudioDevice();

            reconhecedor.RecognizeAsync(RecognizeMode.Multiple);
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

            var wrapper = SdkWrapper.Instance;

            if (!string.IsNullOrEmpty(e.Result.Text))
            {
                TelloSdkStandard.actions.Action action = null;
                try
                {
                    switch (frase)
                    {
                        case "foda-se":
                            {
                                resposta.Speak("Foda-se você, seu babaca.");
                                action = wrapper.BaseActions.TakeOff();
                                break;
                            }
                        case "decolar":
                            {
                                resposta.Speak("Ok, Decolando.");
                                action = wrapper.BaseActions.TakeOff();
                                break;
                            }
                        case "pousar":
                            {
                                resposta.Speak("Ok, pousando o drone.");
                                action = wrapper.BaseActions.Land();

                                break;
                            }
                        case "cambalhota":
                            {
                                resposta.Speak("Girando");
                                action = wrapper.FlipActions.FlipBackLeft();
                                break;
                            }
                        case "frente":
                            {
                                resposta.Speak("Para frente 50 centímetros.");
                                action = wrapper.FlyActions.FlyForward(50);
                                break;
                            }
                        case "tras":
                            {
                                resposta.Speak("Para trás 50 centímetros.");
                                action = wrapper.FlyActions.FlyBack(50);
                                break;
                            }
                        case "girar":
                            {
                                action = wrapper.RotationActions.RotateClockwise(360);
                                break;
                            }
                        case "bateria":
                            {
                                var resp = wrapper.BaseActions.QueryBattery().Execute();
                                if (resp == SdkWrapper.SdkReponses.OK)
                                {
                                    resposta.SpeakAsync(wrapper.BaseActions.QueryBattery().ServerResponse);
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
                    if (action != null)
                    {
                        var resp1 = action.Execute();
                        if (resp1 == SdkWrapper.SdkReponses.FAIL)
                        {
                            if (action.LastException != null)
                            {
                                throw action.LastException;
                            }
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
            resposta.Volume = 100;
            Gramatica();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            Init();

            resposta.Rate = 4;
            resposta.SpeakAsync("Olá, Lucas. Pronto para se divertir?");
            resposta.Rate = 2;
            resposta.SpeakAsync("Estou pronta. Me diga o que fazer.");


            lblStatus.Text = "Aguardando comandos: ";
            led.Visible = true;
            Application.DoEvents();
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
            var wrapper = SdkWrapper.Instance;

            TelloSdkStandard.actions.Action action = null;
            var resp = wrapper.BaseActions.QueryBattery().Execute();
            if (resp == SdkWrapper.SdkReponses.OK)
            {
                lblBateria.Text = "Bateria:" +  wrapper.BaseActions.QueryBattery().ServerResponse +"%";
            }
        }

    }
}
