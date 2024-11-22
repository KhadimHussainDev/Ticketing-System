using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ticketing_System
{
    public partial class ViewDetails : Form
    {
        private SiteTicket ticket;
        private int currentImageIndex = 0;

        public ViewDetails(SiteTicket ticket)
        {
            InitializeComponent();
            this.ticket = ticket;
            PopulateFields();
        }

        private void PopulateFields()
        {
            txtTicketNumber.Text = ticket.TicketNumber.ToString();
            txtSiteAddress.Text = ticket.SiteAddress;
            txtIssueDescription.Text = ticket.IssueDescription;
            txtTechnicianName.Text = ticket.TechnicianName;
            txtTechnicianContact.Text = ticket.TechnicianContact;
            arrivalTimeDT.Value = DateTime.Parse(ticket.ArrivalTime);
            DepartureTimeDT.Value = DateTime.Parse(ticket.DepartureTime);
            textBoxWorkingTime.Text = (DepartureTimeDT.Value - arrivalTimeDT.Value).ToString();
            arrivalTimeDT.Format = DateTimePickerFormat.Custom;
            arrivalTimeDT.CustomFormat = Form1.dateFormat;
            DepartureTimeDT.Format = DateTimePickerFormat.Custom;
            DepartureTimeDT.CustomFormat = Form1.dateFormat;
            textBoxPayment.Text = ticket.Payment;

                labelfile.Text = "Total files : " + ticket.Photos.Count.ToString() + "\n File No: " + (currentImageIndex + 1);

            if (ticket.Photos.Count > 0 && ticket.Photos[0] != "")
            {
                string filePath = ticket.Photos[currentImageIndex];
                if (IsImage(filePath))
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox1.Image = Image.FromFile(filePath);

                    // Hide Label33 if it's an image
                    labelDownloadToSee.Visible = false;
                }
                else
                {
                    // Show Label33 if it's not an image
                    labelDownloadToSee.Visible = true;
                    //labelDownloadToSee.Text = "This file is not an image: " + Path.GetFileName(filePath);
                }
            }
        }


        private void NextButton_Click(object sender, EventArgs e)
        {
            if (ticket.Photos.Count > 1)
            {

                currentImageIndex = (currentImageIndex + 1) % ticket.Photos.Count;
                labelfile.Text = "";
                labelfile.Text = "Total files : " + ticket.Photos.Count.ToString() + "\n File No: " + (currentImageIndex + 1);
                string nextFilePath = ticket.Photos[currentImageIndex];

                if (IsImage(nextFilePath))
                {
                    pictureBox1.Image = Image.FromFile(nextFilePath);

                    // Hide Label33 if it's an image
                    labelDownloadToSee.Visible = false;
                }
                else
                {
                    // Show Label33 if it's not an image
                    pictureBox1.Image = null;
                    labelDownloadToSee.Visible = true;
                    //labelDownloadToSee.Text = "This file is not an image: " + Path.GetFileName(nextFilePath);
                }
            }
            else
            {
                MessageBox.Show("No more Files to show");
            }
        }
        private bool IsImage(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            string fileExtension = Path.GetExtension(filePath).ToLower();

            return imageExtensions.Contains(fileExtension);
        }


        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ViewDetails_Load(object sender, EventArgs e)
        {

        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            if (ticket.Photos.Count > 0)
            {
                // Get the current file path
                string currentFilePath = ticket.Photos[currentImageIndex];
                string fileExtension = Path.GetExtension(currentFilePath).ToLower();

                // Initialize SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save File";

                // Set the filter based on the file extension
                if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".bmp" || fileExtension == ".gif")
                {
                    saveFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                }
                else if (fileExtension == ".pdf")
                {
                    saveFileDialog.Filter = "PDF Files|*.pdf";
                }
                else if (fileExtension == ".txt")
                {
                    saveFileDialog.Filter = "Text Files|*.txt";
                }
                else if (fileExtension == ".docx")
                {
                    saveFileDialog.Filter = "Word Documents|*.docx";
                }
                else if (fileExtension == ".xlsx")
                {
                    saveFileDialog.Filter = "Excel Files|*.xlsx";
                }
                else
                {
                    saveFileDialog.Filter = "All Files|*.*";
                }

                // Set default file name
                saveFileDialog.FileName = Path.GetFileName(currentFilePath);

                // Show the SaveFileDialog
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Check if the file is an image
                        if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".bmp" || fileExtension == ".gif")
                        {
                            // Save image if it's an image
                            pictureBox1.Image.Save(saveFileDialog.FileName);
                        }
                        else
                        {
                            // For non-image files, copy the file to the selected destination
                            File.Copy(currentFilePath, saveFileDialog.FileName, true);
                        }

                        MessageBox.Show("File saved successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving file: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("No files to download.");
            }
        }

        private void buttonManageNotes_Click(object sender, EventArgs e)
        {
            new ManageNotes(ticket.TicketNumber).ShowDialog();
        }
    }
}
