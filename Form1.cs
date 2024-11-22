using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ticketing_System
{
    public partial class Form1 : Form
    {
        public static string dateFormat = "MM/dd/yy h:mm:ss tt";
        private List<string> copiedImagePaths;
        private string debugFolderPath;
        private SiteTicketManager ticketManager;
        private int ticketNumber;
        public Form1(SiteTicketManager ticketManager)
        {
            InitializeComponent();
            copiedImagePaths = new List<string>();
            debugFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            InitializeDateTimePicker();
            this.ticketManager = ticketManager;
            ticketNumber = ticketManager.GetNextTicketNumber();
            textBoxTicketNumber.Text = ticketNumber.ToString();
        }
        private void InitializeDateTimePicker()
        {
            dateTimePickerArriavalTime.Format = DateTimePickerFormat.Custom;
            dateTimePickerArriavalTime.CustomFormat = Form1.dateFormat;
            //dateTimePickerArriavalTime.ShowUpDown = true;
            dateTimePickerDepartureTime.Format = DateTimePickerFormat.Custom;
            dateTimePickerDepartureTime.CustomFormat = Form1.dateFormat;
            //dateTimePickerDepartureTime.ShowUpDown = true;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void buttonSaveTicket_Click(object sender, EventArgs e)
        {


            // Step 2: Prepare ticket data
            string siteAddress = textBoxSiteAddress.Text;
            string issueDescription = richTextBoxIssueDescription.Text;
            string technicianName = textBoxTechnicianName.Text;
            string technicianContact = textBoxTechnicianContact.Text;
            string arrivalTime = dateTimePickerArriavalTime.Value.ToString(Form1.dateFormat);
            string departureTime = dateTimePickerDepartureTime.Value.ToString(Form1.dateFormat);
            string paymet = textBoxPayment.Text;

            List<string> imagePaths = copiedImagePaths;

            // Step 3: Create a new Ticket object
            SiteTicket newTicket = new SiteTicket
            {
                TicketNumber = ticketNumber,
                SiteAddress = siteAddress,
                IssueDescription = issueDescription,
                TechnicianName = technicianName,
                TechnicianContact = technicianContact,
                ArrivalTime = arrivalTime,
                DepartureTime = departureTime,
                Photos = imagePaths,
                Payment = paymet,
                IsBilled = false
            };

            // Step 4: Add the new ticket to the TicketManager
            ticketManager.AddTicket(newTicket);

            // Optional: Notify the user
            MessageBox.Show("Ticket saved successfully!");
            this.Close();


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonBrowsePic_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Files";
                openFileDialog.Filter = "All Files|*.*"; // Allow any file type
                openFileDialog.Multiselect = true; // Allow multiple file selection

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Ensure the debug folder exists
                    if (!Directory.Exists(debugFolderPath))
                    {
                        Directory.CreateDirectory(debugFolderPath);
                    }

                    // Clear previous selections
                    copiedImagePaths.Clear();

                    // Handle each selected file
                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        string extension = Path.GetExtension(filePath);
                        string randomFileName = $"{Guid.NewGuid()}{extension}"; // Generate a random name
                        string destinationPath = Path.Combine(debugFolderPath, randomFileName);

                        // Copy the file to the debug folder
                        File.Copy(filePath, destinationPath, true); // Overwrite if it already exists

                        // Store the copied file path in the list
                        copiedImagePaths.Add(destinationPath);
                    }
                }
            }

     

            // Optional: Display copied paths
            // MessageBox.Show("Copied Files to Debug Folder:\n" + string.Join("\n", copiedImagePaths));
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

