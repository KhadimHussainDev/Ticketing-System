using System;
using System.IO;
using System.Windows.Forms;

namespace Ticketing_System
{
    public partial class Add_Note : Form
    {
        int ticketId;
        public Add_Note(int ticketId)
        {
            InitializeComponent();
            this.ticketId = ticketId;
            dateTimePicker.Format = DateTimePickerFormat.Custom;
            dateTimePicker.CustomFormat = Form1.dateFormat;
        }
        private void button1_Click(object sender, EventArgs e)
        {

            string note = textBoxNote.Text;
            string date = dateTimePicker.Value.ToString(Form1.dateFormat);

            string csvFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csvs");
            Directory.CreateDirectory(csvFolderPath); // Ensure the folder exists
            string csvFilePath = Path.Combine(csvFolderPath, "notes.csv");


            try
            {
                using (StreamWriter sw = new StreamWriter(csvFilePath, true))
                {
                    // Write the header if the file is new
                    if (new FileInfo(csvFilePath).Length == 0)
                    {
                        sw.WriteLine("TicketID,Note,Date");
                    }

                    // Write the data
                    sw.WriteLine($"{ticketId},{note},{date}");
                }

                MessageBox.Show("Note saved successfully!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving note: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
