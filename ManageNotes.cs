using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Ticketing_System
{
    public partial class ManageNotes : Form
    {
        int ticketID;
        public ManageNotes(int ticketId)
        {
            InitializeComponent();
            ticketID = ticketId;
            loadData();
        }

        private void loadData()
        {
            string csvFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csvs");
            Directory.CreateDirectory(csvFolderPath); // Ensure the folder exists
            string csvFilePath = Path.Combine(csvFolderPath, "notes.csv");

            DataTable dt = new DataTable();
            // dt.Columns.Add("TicketID");
            dt.Columns.Add("Note");
            dt.Columns.Add("Date");

            if (File.Exists(csvFilePath))
            {
                var lines = File.ReadAllLines(csvFilePath);
                foreach (var line in lines.Skip(1)) // Skip header line
                {
                    var values = line.Split(',');
                    if (values[0] == ticketID.ToString())
                    {
                        dt.Rows.Add(values[1], values[2]);
                    }
                }
            }

            dgv.DataSource = dt;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Add_Note add_Note = new Add_Note(ticketID);
            add_Note.ShowDialog();
            loadData();
        }

        private void ManageNotes_Load(object sender, EventArgs e)
        {

        }
    }
}
