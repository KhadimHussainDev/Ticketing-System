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
    public partial class grid : Form
    {
        private SiteTicketManager ticketManager;
        public grid()
        {
            InitializeComponent();
            string csvFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csvs");
            Directory.CreateDirectory(csvFolderPath); // Ensure the folder exists
            string csvFilePath = Path.Combine(csvFolderPath, "tickets.csv");

            ticketManager = new SiteTicketManager(csvFilePath);
            LoadTickets();
        }

        private void LoadTickets()
        {
            textBoxTicketNumber.Clear();
            List<SiteTicket> tickets = ticketManager.GetUnBilledTickets(); // Use the TicketManager to get all tickets

            showTickets(tickets);
        }
        private void showTickets(List<SiteTicket> tickets)
        {
            dgv.DataSource = tickets.Select(ticket => new
            {
                TicketNumber = ticket.TicketNumber,
                SiteAddress = ticket.SiteAddress,
                IssueDescription = ticket.IssueDescription,
                TechnicianName = ticket.TechnicianName,
                TechnicianContact = ticket.TechnicianContact,
                ArrivalTime = ticket.ArrivalTime,
                DepartureTime = ticket.DepartureTime,
                WorkingTime = (DateTime.Parse(ticket.DepartureTime) - DateTime.Parse(ticket.ArrivalTime)).ToString(),
                Payment = ticket.Payment,
                IsBilled = ticket.IsBilled
            }).ToList();
        }
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1(ticketManager);
            form1.ShowDialog();
            LoadTickets();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count > 0) // Check if any row is selected
            {
                // Get the selected row
                var selectedRow = dgv.SelectedRows[0];

                // Extract the ticket information from the selected row
                var ticketNumber = Convert.ToInt32(selectedRow.Cells["TicketNumber"].Value);
                var siteAddress = selectedRow.Cells["SiteAddress"].Value.ToString();
                var issueDescription = selectedRow.Cells["IssueDescription"].Value.ToString();
                var technicianName = selectedRow.Cells["TechnicianName"].Value.ToString();
                var technicianContact = selectedRow.Cells["TechnicianContact"].Value.ToString();
                var arrivalTime = selectedRow.Cells["ArrivalTime"].Value.ToString();
                var departureTime = selectedRow.Cells["DepartureTime"].Value.ToString();


                // Create a new SiteTicket object
                var selectedTicket = new SiteTicket
                {
                    TicketNumber = ticketNumber,
                    SiteAddress = siteAddress,
                    IssueDescription = issueDescription,
                    TechnicianName = technicianName,
                    TechnicianContact = technicianContact,
                    ArrivalTime = arrivalTime,
                    DepartureTime = departureTime,
                    Payment = selectedRow.Cells["Payment"].Value.ToString(),
                    IsBilled = Convert.ToBoolean(selectedRow.Cells["IsBilled"].Value)
                };
                Update updateForm = new Update(selectedTicket, ticketManager);
                updateForm.ShowDialog();
                LoadTickets();
            }
            else
            {
                MessageBox.Show("Please select a ticket to update.");
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count > 0) // Check if any row is selected
            {
                // Get the selected row
                var selectedRow = dgv.SelectedRows[0];

                // Extract the ticket number from the selected row
                var ticketNumber = Convert.ToInt32(selectedRow.Cells["TicketNumber"].Value);

                // Delete the ticket using the TicketManager
                ticketManager.DeleteTicket(ticketNumber);

                LoadTickets();
            }
            else
            {
                MessageBox.Show("Please select a ticket to delete.");
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            //if (int.TryParse(textBoxTicketNumber.Text, out int ticketNumber)) // Validate the ticket number input
            //{
            //    // Search for the ticket in the TicketManager
            //    var tickets = ticketManager.GetTicketsByNumber(ticketNumber); // Implement this method

            //    // Set the DataGridView DataSource based on the search results
            //    dgv.DataSource = tickets.Select(ticket => new
            //    {
            //        TicketNumber = ticket.TicketNumber,
            //        SiteAddress = ticket.SiteAddress,
            //        IssueDescription = ticket.IssueDescription,
            //        TechnicianName = ticket.TechnicianName,
            //        TechnicianContact = ticket.TechnicianContact,
            //        ArrivalTime = ticket.ArrivalTime,
            //        DepartureTime = ticket.DepartureTime
            //    }).ToList();

            //    // Check if any tickets were found
            //    if (!tickets.Any())
            //    {
            //        MessageBox.Show("No tickets found with that number.");
            //        textBoxTicketNumber.Clear();
            //        LoadTickets(); // Reload all tickets if no match is found
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Please enter a valid ticket number.");
            //}
        }

        private void viewDetailsbtn_Click(object sender, EventArgs e)
        {

        }

        private void dgv_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgv.SelectedRows.Count > 0) // Check if any row is selected
            {
                var selectedRow = dgv.SelectedRows[0];
                int ticketNumber = Convert.ToInt32(selectedRow.Cells["TicketNumber"].Value);

                // Retrieve ticket details using SiteTicketManager
                var ticket = ticketManager.GetTicketsByNumber(ticketNumber).FirstOrDefault();

                if (ticket != null)
                {
                    // Pass the ticket data to the ViewDetails form
                    ViewDetails viewDetailsForm = new ViewDetails(ticket);
                    viewDetailsForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Ticket details not found.");
                }
            }
            else
            {
                MessageBox.Show("Please select a ticket to view details.");
            }
        }

        private void textBoxTicketNumber_TextChanged(object sender, EventArgs e)
        {
            if (textBoxTicketNumber.Text != "")
            {
                if (int.TryParse(textBoxTicketNumber.Text, out int ticketNumber)) // Validate the ticket number input
                {
                    // Search for the ticket in the TicketManager
                    var tickets = ticketManager.GetTicketsByNumber(ticketNumber); // Implement this method

                    showTickets(tickets);


                    // Check if any tickets were found
                    if (!tickets.Any())
                    {
                        //MessageBox.Show("No tickets found with that number.");
                        //textBoxTicketNumber.Clear();
                        //LoadTickets(); // Reload all tickets if no match is found
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid ticket number.");
                    textBoxTicketNumber.Clear();
                    LoadTickets();
                }
            }
            else
            {
                LoadTickets();
            }
        }

        private void grid_Load(object sender, EventArgs e)
        {

        }

        private void buttonShowBilledItems_Click(object sender, EventArgs e)
        {
            var tickets = ticketManager.GetBilledTickets();
            showTickets(tickets);
        }

        private void buttonMarkasBilled_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count > 0)
            {

                var selectedRow = dgv.SelectedRows[0];
                if (selectedRow.Cells["IsBilled"].Value.ToString() == "True")
                {
                    MessageBox.Show("Ticket is already marked as billed.");
                    return;
                }
                int ticketNumber = Convert.ToInt32(selectedRow.Cells["TicketNumber"].Value);
                ticketManager.MarkTicketAsBilled(ticketNumber);
                LoadTickets();
            }
            else
            {
                MessageBox.Show("Please select a ticket to mark as billed.");
            }
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            
            LoadTickets();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count > 0) // Check if any row is selected
            {
                var selectedRow = dgv.SelectedRows[0];
                int ticketNumber = Convert.ToInt32(selectedRow.Cells["TicketNumber"].Value);

                // Retrieve ticket details using SiteTicketManager
                var ticket = ticketManager.GetTicketsByNumber(ticketNumber).FirstOrDefault();

                if (ticket != null)
                {
                    // Pass the ticket data to the ViewDetails form
                    SendEmail sendemail = new SendEmail(ticket);
                    sendemail.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Ticket details not found.");
                }
            }
            else
            {
                MessageBox.Show("Please select a ticket to Send the Email");
            }
        }
    }
}
