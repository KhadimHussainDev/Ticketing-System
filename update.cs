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
    public partial class Update : Form
    {
        private SiteTicket currentTicket;
        private SiteTicketManager ticketManager;
        public Update(SiteTicket siteTicket, SiteTicketManager manager)
        {
            InitializeComponent();
            currentTicket = siteTicket;
            ticketManager = manager;
        }

        private void update_Load(object sender, EventArgs e)
        {
            dateTPArriavalTime.Format = DateTimePickerFormat.Custom;
            dateTPArriavalTime.CustomFormat = Form1.dateFormat;
            dateTPArriavalTime.ShowUpDown = true;
            dateTPDepartureTime.Format = DateTimePickerFormat.Custom;
            dateTPDepartureTime.CustomFormat = Form1.dateFormat;
            dateTPDepartureTime.ShowUpDown = true;
            txtTicketNumber.Text = currentTicket.TicketNumber.ToString();
            txtSiteAddress.Text = currentTicket.SiteAddress;
            richTxtIssueDescription.Text = currentTicket.IssueDescription;
            txtTechnicianName.Text = currentTicket.TechnicianName;
            txtTechnicianContact.Text = currentTicket.TechnicianContact;
            dateTPArriavalTime.Value = DateTime.Parse(currentTicket.ArrivalTime);
            dateTPDepartureTime.Value = DateTime.Parse(currentTicket.DepartureTime);
        }

        private void buttonSaveTicket_Click(object sender, EventArgs e)
        {
           
        }

        private void btnBrowsePic_Click(object sender, EventArgs e)
        {
            string debugFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            List<string> copiedImagePaths = new List<string>();
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

                    currentTicket.Photos = copiedImagePaths;
                    // Optional: Display copied image paths
                    //MessageBox.Show("Copied Images to Debug Folder:\n" + string.Join("\n", copiedImagePaths));
                }
            }
        }

        private void btnUpdateTicket_Click(object sender, EventArgs e)
        {
            // Update the currentTicket with new values from the form
            currentTicket.SiteAddress = txtSiteAddress.Text;
            currentTicket.IssueDescription = richTxtIssueDescription.Text;
            currentTicket.TechnicianName = txtTechnicianName.Text;
            currentTicket.TechnicianContact = txtTechnicianContact.Text;
            currentTicket.ArrivalTime = dateTPArriavalTime.Value.ToString(Form1.dateFormat);
            currentTicket.DepartureTime = dateTPDepartureTime.Value.ToString(Form1.dateFormat);
            currentTicket.Payment = textBoxPayment.Text;

            // Call the TicketManager to update the ticket in the CSV file
            ticketManager.UpdateTicket(currentTicket);

            // Optionally, close the form or show a message
            MessageBox.Show("Ticket updated successfully!");
            this.Close();
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
