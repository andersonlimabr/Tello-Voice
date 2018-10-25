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
        SpeechSynthesizer resposta =  new SpeechSynthesizer();
        static CultureInfo culture = new CultureInfo("pt-BR");
        public Form1()
        {
            InitializeComponent();
        }
      
        private void Gramatica()
        {
            lblStatus.Text = "Carregando a gramática...";        
            Application.DoEvents();

            string[] listaPalavras = { "decolar", "pousar", "girar","frente", "tras","bateria", "foda-se" };

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
            reconhecedor.SetInputToDefaultAudioDevice();
            resposta.SetOutputToDefaultAudioDevice();


            reconhecedor.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void Reconhecedor_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string frase = e.Result.Text;


            lblStatus.Text = "Comando recebido: " + frase;
            Application.DoEvents();

                       


            Application.DoEvents();

            if (!string.IsNullOrEmpty(e.Result.Text))
            {

                var wrapper = SdkWrapper.Instance;
               
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
                            case "3":
                                {
                                    action = wrapper.FlipActions.FlipBackLeft();
                                    break;
                                }
                            case "frente":
                                {
                                    resposta.Speak("Para frente, 50 centiímetros.");
                                    action = wrapper.FlyActions.FlyForward(50);
                                    break;
                                }
                            case "tras":
                                {
                                    resposta.Speak("Para trás, 50 centiímetros.");
                                    action = wrapper.FlyActions.FlyBack(50);
                                    break;
                                }
                            case "6":
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
                                    //ExecuteFlightPlan(wrapper);
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


   
        }

        private void Init()
        {
            resposta.Volume = 100;
            resposta.Rate = 3;
            

            Gramatica();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            Init();


            resposta.SpeakAsync("Olá, Lucas. Estou pronta. Me diga o que fazer.");

            lblStatus.Text = "Aguardando comandos: ";
            Application.DoEvents();
        }
    }
  
}
