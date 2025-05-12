using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvolutiveSystem.UI.Forms // Namespace suggerito per la tua libreria UI
{
    /// <summary>
    /// Un form personalizzato che funge da InputBox per ottenere un singolo valore testuale dall'utente.
    /// Questo form è progettato per essere utilizzato come finestra di dialogo modale.
    /// </summary>
    public partial class InputBoxForm : Form
    {
        // Proprietà pubblica per accedere al valore inserito dall'utente dopo che il dialogo è stato chiuso con OK.
        public string InputValue { get; private set; }

        // Riferimenti ai controlli del form.
        // Se usi il designer, questi saranno parziali e definiti nel file .Designer.cs.
        // Se crei i controlli manualmente, dovrai dichiararli qui e istanziarli nel costruttore.
        private Label lblPrompt;
        private TextBox txtInput;
        private Button btnOk;
        private Button btnCancel;

        /// <summary>
        /// Costruttore per creare una nuova istanza di InputBoxForm.
        /// </summary>
        /// <param name="prompt">Il messaggio da mostrare all'utente (es. "Inserisci il nome:").</param>
        /// <param name="title">Il titolo della finestra di dialogo.</param>
        /// <param name="defaultValue">Un valore predefinito opzionale per la textbox.</param>
        public InputBoxForm(string prompt, string title, string defaultValue = "")
        {
            // InitializeComponent() è chiamato dal designer per configurare i controlli.
            // Se non usi il designer, dovrai commentare questa riga e creare/configurare i controlli manualmente.
            InitializeComponent();

            // --- Configurazione Manuale dei Controlli (Se NON usi il Designer) ---
            // Scommenta e adatta il codice seguente se non utilizzi il designer di Windows Forms.
            
            this.Text = title; // Imposta il titolo della finestra
            this.ClientSize = new Size(300, 150); // Imposta la dimensione del form
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Rende il form non ridimensionabile
            this.MaximizeBox = false; // Disabilita il pulsante Massimizza
            this.MinimizeBox = false; // Disabilita il pulsante Minimizza
            this.StartPosition = FormStartPosition.CenterParent; // Posiziona il form al centro del form genitore
            this.ShowInTaskbar = false; // Non mostrare il form nella barra delle applicazioni
            this.AcceptButton = btnOk; // Imposta il pulsante OK come pulsante di default (premendo Invio)
            this.CancelButton = btnCancel; // Imposta il pulsante Annulla (premendo ESC)

            // Label per il prompt
            lblPrompt = new Label();
            lblPrompt.Text = prompt;
            lblPrompt.Location = new Point(10, 10); // Posizione
            lblPrompt.AutoSize = true; // Adatta la dimensione al testo
            this.Controls.Add(lblPrompt); // Aggiungi la label al form

            // TextBox per l'input
            txtInput = new TextBox();
            txtInput.Location = new Point(10, 30); // Posizione sotto la label
            txtInput.Size = new Size(270, 20); // Dimensione
            txtInput.Text = defaultValue; // Imposta il valore predefinito
            txtInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // Ancoraggio (utile anche se FixedDialog)
            this.Controls.Add(txtInput); // Aggiungi la textbox al form

            // Pulsante OK
            btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Location = new Point(130, 70); // Posizione
            btnOk.DialogResult = DialogResult.OK; // Imposta il risultato del dialogo
            btnOk.Click += BtnOk_Click; // Associa il gestore eventi Click
            this.Controls.Add(btnOk); // Aggiungi il pulsante al form

            // Pulsante Annulla
            btnCancel = new Button();
            btnCancel.Text = "Annulla";
            btnCancel.Location = new Point(210, 70); // Posizione
            btnCancel.DialogResult = DialogResult.Cancel; // Imposta il risultato del dialogo
            this.Controls.Add(btnCancel); // Aggiungi il pulsante al form
            
            // --- Fine Configurazione Manuale ---


            // Se usi il designer, i controlli sono dichiarati in .Designer.cs.
            // Assicurati che i loro nomi (lblPrompt, txtInput, btnOk, btnCancel)
            // corrispondano a quelli usati qui e nel designer.

            // Imposta il testo della label e il valore predefinito della textbox utilizzando i controlli del designer.
            // Controlla che i controlli non siano null, nel caso in cui InitializeComponent() non sia chiamato o fallisca.
            if (lblPrompt != null) lblPrompt.Text = prompt;
            if (txtInput != null) txtInput.Text = defaultValue;
            this.Text = title; // Imposta il titolo del form

            // Associa i gestori eventi Click ai pulsanti OK e Annulla.
            // Questo può essere fatto anche nel designer. Se lo fai nel designer, rimuovi queste righe.
            if (btnOk != null) btnOk.Click += BtnOk_Click;
            if (btnCancel != null) btnCancel.Click += BtnCancel_Click;

            // Imposta i pulsanti di default e di annullamento.
            // Questo può essere fatto anche nel designer. Se lo fai nel designer, rimuovi queste righe.
            if (btnOk != null) this.AcceptButton = btnOk;
            if (btnCancel != null) this.CancelButton = btnCancel;
        }

        /// <summary>
        /// Gestore eventi per il click sul pulsante OK.
        /// Imposta il valore di InputValue prima che il form si chiuda con DialogResult.OK.
        /// </summary>
        private void BtnOk_Click(object sender, EventArgs e)
        {
            // Assumendo che tu abbia una TextBox chiamata txtInput nel designer
            if (txtInput != null)
            {
                InputValue = txtInput.Text; // Salva il testo inserito dall'utente
            }
            // Il DialogResult del pulsante OK è già impostato a DialogResult.OK,
            // quindi il form si chiuderà automaticamente dopo l'esecuzione di questo handler.
        }

        /// <summary>
        /// Gestore eventi per il click sul pulsante Annulla.
        /// Non fa nulla di speciale, il form si chiuderà con DialogResult.Cancel.
        /// </summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // InputValue rimarrà null o il suo valore iniziale.
            // Il DialogResult del pulsante Annulla è già impostato a DialogResult.Cancel,
            // quindi il form si chiuderà automaticamente.
        }

        // --- Metodo statico helper per mostrare l'InputBox ---
        /// <summary>
        /// Mostra la finestra di dialogo InputBox in modo modale e restituisce il valore inserito dall'utente.
        /// Utilizza 'using' per garantire che il form venga correttamente rilasciato.
        /// </summary>
        /// <param name="prompt">Il messaggio da mostrare all'utente (es. "Inserisci il nome:").</param>
        /// <param name="title">Il titolo della finestra di dialogo.</param>
        /// <param name="defaultValue">Un valore predefinito opzionale per la textbox.</param>
        /// <returns>Il valore inserito dall'utente se ha cliccato OK; altrimenti, null.</returns>
        public static string Show(string prompt, string title, string defaultValue = "")
        {
            // Crea una nuova istanza del form InputBoxForm
            using (InputBoxForm form = new InputBoxForm(prompt, title, defaultValue))
            {
                // Mostra il form come finestra di dialogo modale.
                // L'esecuzione si ferma qui finché l'utente non chiude il form.
                DialogResult result = form.ShowDialog();

                // Controlla il risultato del dialogo.
                if (result == DialogResult.OK)
                {
                    // Se l'utente ha cliccato OK, restituisci il valore inserito.
                    return form.InputValue;
                }
                else
                {
                    // Se l'utente ha cliccato Annulla o chiuso il form in altro modo, restituisci null.
                    return null;
                }
            }
        }

        // NOTA: Se usi il designer, dovrai aprire il file InputBoxForm.cs [Design]
        // e aggiungere manualmente i controlli:
        // 1. Una Label (rinominata lblPrompt)
        // 2. Una TextBox (rinominata txtInput)
        // 3. Un Button (rinominato btnOk, con Text="OK" e DialogResult="OK")
        // 4. Un Button (rinominato btnCancel, con Text="Annulla" e DialogResult="Cancel")
        // Posizionali in modo appropriato.
        // Il metodo InitializeComponent() verrà generato automaticamente nel file .Designer.cs
        // e si occuperà di creare e configurare questi controlli.
        // Assicurati che gli eventi Click per btnOk e btnCancel siano associati nel designer
        // o nel codice del costruttore come mostrato sopra.
    }
}
