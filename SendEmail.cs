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

using PdfSharp.Pdf;
using PdfSharp.Drawing;

using MimeKit;
using System.Net.Mail;
using System.Net;
using System.Xml.Linq;
using Org.BouncyCastle.Asn1.Pkcs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Ticketing_System
{
    public partial class SendEmail : Form
    {
        private SiteTicket ticket;
        public SendEmail(SiteTicket ticket)
        {
            InitializeComponent();
            this.ticket = ticket;
            PopulateFields();
        }

        private void PopulateFields()
        {
            txtTicketNumber.Text = ticket.TicketNumber.ToString();
            txtSiteAddress.Text = ticket.SiteAddress;
            txtTechnicianName.Text = ticket.TechnicianName;
            txtTechnicianPhone.Text = ticket.TechnicianContact;
        }


        private void btnGeneratePDF_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OrderDetails.pdf");

                PdfDocument document = new PdfDocument();

                // Page 1: Title and Table
                PdfPage page1 = document.AddPage();
                XGraphics gfx1 = XGraphics.FromPdfPage(page1);
                int currentY = 0; // Initialize starting Y position
                AddTitle(gfx1, page1, @"pslogo.jpg", ref currentY);
                //AddTitle(gfx1, page1, "PulseSphere WorkOrder", ref currentY);
                AddTable(gfx1, page1, ref currentY);
                AddStaticText(gfx1, page1);


                // Page 2: Header and Scope of Work
                PdfPage page2 = document.AddPage();
                XGraphics gfx2 = XGraphics.FromPdfPage(page2);
                currentY = 20; // Reset Y position for the new page
                AddTitle(gfx2, page2, @"pslogo.jpg", ref currentY);
                //AddTitle(gfx2, page2, "PulseSphere WorkOrder", ref currentY);
                AddSectionHeader(gfx2, page2, "Scope of Work");

                // Fetch the text from the RichTextBox

                WriteScopeOfWork(gfx2, ref page2, txtScopeofWork.Text, ref currentY);


                // Page 3: Static Content
                PdfPage page3 = document.AddPage();
                XGraphics gfx3 = XGraphics.FromPdfPage(page3);
                currentY = 20; // Reset Y position for the new page
                AddTitle(gfx3, page3, @"pslogo.jpg", ref currentY);
                //AddTitle(gfx3, page3, "PulseSphere WorkOrder", ref currentY);

                Add3rdpageStaticData(gfx3, page3, ref currentY);

                // Page 4: Header and Tables
                PdfPage page4 = document.AddPage();
                XGraphics gfx4 = XGraphics.FromPdfPage(page4);
                currentY = 20; // Reset Y position for the new page
                AddTitle(gfx4, page4, @"pslogo.jpg", ref currentY);
                //AddTitle(gfx4, page4, "PulseSphere WorkOrder", ref currentY);
                AddTable(gfx4, page4, ref currentY);
                AddTechnicianTable(gfx4, page4, ref currentY);
                AddLastTable(gfx4, page4, ref currentY);

                document.Save(filePath);
                //MessageBox.Show("PDF generated successfully with PDFSharp!");
                // Send email with the generated PDF
                string emailrecipient = txtTechinianEmail.Text;
                //?check whether the email is valid or not
                if (emailrecipient.Contains("@") && emailrecipient.Contains(".com"))
                {
                    string body = $"Start Time:{dateScheduled.Value.ToString()}\r\nLocation:{txtSiteAddress.Text}\r\n\r\nWork Order\r\nThe work order and all documentation for your service call is attached. Please review all documentation prior to departing and have the customer sign the attached sign-off sheet. Please review below, the pay rate and scheduled time for this call are as follows:\r\n\r\nPulseSphere Ticket: {ticket.TicketNumber}\r\n\r\nSite Name:{txtSiteName.Text}\r\n\r\nScheduled date and time: {dateScheduled.Value.ToString()}\r\n\r\nRates (Hourly):${textBoxrates.Text}\r\n\r\nTravel:${textBoxtravelrate.Text}\r\n\r\nAny additional charges must be approved by PulseSphere Project Management.\r\n\r\nAll collateral files should be uploaded via email:\r\ninfo@pulsespheretechnologies.com\r\n\r\nThank you for your assistance with this project.\r\n\r\nPulseSphere Technologies\r\n(346) 467-8177";
                    SendEmailWithAttachment("info@pulsespheretechnologies.com", emailrecipient, $"PulseSphere Ticket#{ticket.TicketNumber} - {txtSiteName.Text} | {txtSiteAddress.Text}", body, filePath);
                }
                else
                {
                    MessageBox.Show("Please enter a valid receiver email address");

                    return;
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF with PDFSharp: {ex.Message}\n{ex.StackTrace}");
            }
        }
        // Method to send email with attachment

        private void WriteScopeOfWork(XGraphics gfx, ref PdfPage page, string scopeOfWorkText, ref int currentY)
        {
            XFont font = new XFont("Verdana", 12);
            double marginX = 40; // Left margin
            double maxWidth = page.Width - 2 * marginX; // Usable width
            double lineHeight = gfx.MeasureString("Sample", font).Height; // Line height

            // Split the text into paragraphs by newlines
            string[] paragraphs = scopeOfWorkText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            foreach (string paragraph in paragraphs)
            {
                // Use the text-wrapping logic from Add3rdpageStaticData
                List<string> wrappedLines = SplitTextIntoLines(paragraph, gfx, font, maxWidth);

                foreach (var line in wrappedLines)
                {
                    // Check if there's enough space on the current page
                    if (currentY + lineHeight > page.Height - 40) // Account for bottom margin
                    {
                        // Add a new page
                        page = gfx.PdfPage.Owner.AddPage();
                        gfx.Dispose(); // Dispose the current graphics object
                        gfx = XGraphics.FromPdfPage(page); // Create a new graphics object for the new page
                        currentY = 40; // Reset vertical position for the new page
                    }

                    // Draw the current line of text
                    gfx.DrawString(line, font, XBrushes.Black,
                        new XRect(marginX, currentY, maxWidth, lineHeight), XStringFormats.TopLeft);

                    // Move to the next line
                    currentY += (int)lineHeight;
                }

                // Add extra space between paragraphs
                currentY += (int)(lineHeight / 2);
            }
        }


        // Helper method to wrap text to fit within the page width
        private List<string> WrapTextToWidth(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            List<string> wrappedLines = new List<string>();
            string[] words = text.Split(' '); // Split text into words
            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = (currentLine.Length > 0 ? currentLine + " " : "") + word;

                // If the line fits within the maxWidth, add the word to the line
                if (gfx.MeasureString(testLine, font).Width <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    // Otherwise, save the current line and start a new one
                    wrappedLines.Add(currentLine);
                    currentLine = word;
                }
            }

            // Add the last remaining line, if any
            if (!string.IsNullOrWhiteSpace(currentLine))
            {
                wrappedLines.Add(currentLine);
            }

            return wrappedLines;
        }






        private void SendEmailWithAttachment(string senderEmail, string recipientEmail, string subject, string body, string attachmentFilePath)
        {
            try
            {
                // Set up the email
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail, "PulseSphere");
                    mail.To.Add(recipientEmail);
                    mail.Subject = subject;
                    mail.Body = body;

                    foreach (string path in ticket.Photos)
                    {
                        if (File.Exists(path))
                        {
                            Attachment attachment1 = new Attachment(path);
                            mail.Attachments.Add(attachment1);
                        }
                    }

                    // Attach the PDF file
                    if (File.Exists(attachmentFilePath))
                    {
                        Attachment attachment = new Attachment(attachmentFilePath);
                        mail.Attachments.Add(attachment);
                    }

                    // Configure the SMTP client
                    using (SmtpClient smtpClient = new SmtpClient("smtp-mail.outlook.com"))
                    {
                        smtpClient.Port = 587; // or 25, depending on your SMTP server settings
                        smtpClient.Credentials = new NetworkCredential(senderEmail, "lxmhwqvrgdslvsww");
                        smtpClient.EnableSsl = true; // true if your SMTP server requires SSL

                        // Send the email
                        smtpClient.Send(mail);
                    }

                    MessageBox.Show("Email sent successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending email: {ex.Message}\n{ex.StackTrace}");
            }
        }


        /*private void AddTitle(XGraphics gfx, PdfPage page, string titleText, ref int currentY)
        {
            XFont font = new XFont("Verdana", 18);
            gfx.DrawString(titleText, font, XBrushes.DarkGray, new XRect(0, currentY, page.Width, 40), XStringFormats.Center);
            currentY += 80; // Leave space after the title
        }*/

        private void AddTitle(XGraphics gfx, PdfPage page, string logoPath, ref int currentY)
        {
            // Load the logo image
            XImage logo = XImage.FromFile(logoPath);

            // Define the dimensions for the logo (adjust as needed)
            double logoWidth = 200; // Logo width
            double logoHeight = 70; // Logo height

            // Calculate the position for the logo in the top-left corner
            double startX = 20; // 20 points margin from the left edge
            double startY = 20; // 20 points margin from the top edge

            // Draw the logo on the PDF
            gfx.DrawImage(logo, startX, startY, logoWidth, logoHeight);

            // Update the Y position for the next content
            currentY = (int)(startY + logoHeight + 60); // Leave space below the logo

            //currentY += 80;
        }



        private void AddTable(XGraphics gfx, PdfPage page, ref int currentY)
        {
            XFont attributeFont = new XFont("Verdana", 10);
            XFont valueFont = new XFont("Verdana", 10);

            int xStart = 50; // Starting X position
            int cellWidth = (int)((page.Width - 2 * xStart) / 4); // Width of each cell (4 columns)
            int cellHeight = 25; // Height of each cell

            var data = new (string Attribute, string Value)[]
            {
        ("Site Name", txtSiteName.Text),
        ("Ticket #", txtTicketNumber.Text),
        ("Site Contact", txtSiteContact.Text),
        ("Site Address", txtSiteAddress.Text),
        ("Technician Name", txtTechnicianName.Text),
        ("Technician Phone", txtTechnicianPhone.Text),
        ("City", txtCity.Text),
        ("State/Zip", txtStateZip.Text),
        ("Scheduled Date", dateScheduled.Value.ToString(Form1.dateFormat))
            };

            for (int i = 0; i < data.Length; i += 2) // Process two attributes per row
            {
                int currentX = xStart;

                // Draw first attribute and value
                DrawWrappedText(gfx, data[i].Attribute + ":", attributeFont, currentX, currentY, cellWidth, cellHeight);
                currentX += cellWidth;
                DrawWrappedText(gfx, data[i].Value, valueFont, currentX, currentY, cellWidth, cellHeight);
                currentX += cellWidth;

                // Draw second attribute and value (if exists)
                if (i + 1 < data.Length)
                {
                    DrawWrappedText(gfx, data[i + 1].Attribute + ":", attributeFont, currentX, currentY, cellWidth, cellHeight);
                    currentX += cellWidth;
                    DrawWrappedText(gfx, data[i + 1].Value, valueFont, currentX, currentY, cellWidth, cellHeight);
                }

                currentY += cellHeight; // Move to the next row
            }

            currentY += 40; // Add spacing after the table
        }




        private void AddTable1(XGraphics gfx, PdfPage page)
        {
            XFont font = new XFont("Verdana", 10);
            int x = 40;
            int y = 100;
            int cellHeight = 20;
            int cellWidth = (int)(page.Width - 80) / 4;

            // Headers
            gfx.DrawString("Attribute Name 1", font, XBrushes.Black, new XRect(x, y, cellWidth, cellHeight), XStringFormats.Center);
            gfx.DrawString("Value", font, XBrushes.Black, new XRect(x + cellWidth, y, cellWidth, cellHeight), XStringFormats.Center);
            gfx.DrawString("Attribute Name 2", font, XBrushes.Black, new XRect(x + 2 * cellWidth, y, cellWidth, cellHeight), XStringFormats.Center);
            gfx.DrawString("Value", font, XBrushes.Black, new XRect(x + 3 * cellWidth, y, cellWidth, cellHeight), XStringFormats.Center);

            // Data rows
            for (int i = 0; i < 5; i++)
            {
                y += cellHeight;
                gfx.DrawString($"Attribute {i + 1}", font, XBrushes.Black, new XRect(x, y, cellWidth, cellHeight), XStringFormats.Center);
                gfx.DrawString(txtTicketNumber.Text, font, XBrushes.Black, new XRect(x + cellWidth, y, cellWidth, cellHeight), XStringFormats.Center);
                gfx.DrawString($"Attribute {i + 6}", font, XBrushes.Black, new XRect(x + 2 * cellWidth, y, cellWidth, cellHeight), XStringFormats.Center);
                gfx.DrawString(txtTechnicianName.Text, font, XBrushes.Black, new XRect(x + 3 * cellWidth, y, cellWidth, cellHeight), XStringFormats.Center);
            }
        }


        private void AddStaticText(XGraphics gfx, PdfPage page)
        {
            // Define fonts
            XFont defaultFont = new XFont("Verdana", 10);
            XFont boldFont = new XFont("Verdana", 10, XFontStyleEx.Bold);
            XFont blueFont = new XFont("Verdana", 10, XFontStyleEx.Regular);

            // Starting positions and line height
            double startX = 50; // Fixed margin
            double startY = 300; // Start drawing from this Y position
            double lineHeight = 20; // Space between lines

            // Define the text sections
            string[] paragraphs =
            {
        "TECHNICIAN MUST CALL THE PST CALL CENTER @ (346) 467-8177 UPON ARRIVAL AND COMPLETION. IF YOU ARE GOING TO BE LATE TO THIS SERVICE CALL FOR ANY REASON, YOU MUST CALL THE PST CALL CENTER TO NOTIFY PRIOR TO THE SCHEDULED ARRIVAL TIME. FAILURE TO ABIDE BY THIS INSTRUCTION WILL RESULT IN A DEDUCTION FROM PAY AWARDED FOR THIS SERVICE CALL.",
        "YOU WILL ARRIVE ON SITE ON THE CORRECT DATE AND TIME SPECIFIED ABOVE. YOU WILL BE QUALIFIED TO COMPLETE THE WORK DESCRIBED IN THE SCOPE OF WORK BELOW. YOU WILL ALL OF THE TOOLS LISTED ON THIS WORK ORDER TO COMPLETE THE SCOPE OF WORK. YOU WILL SUBMIT ALL COLLATERAL REQUIRED FOR THIS SERVICE WITHIN 24 HOURS. AFTER COMPLETION OF THIS SERVICE CALL, YOU WILL RECEIVE A BILLING RECEIPT THAT YOU MAY APPROVE FOR PAYMENT.",
        "TECHNICIANS MUST POSSESS:"
    };

            string[] bulletPoints =
            {
        "O THE ABILITY TO LIFT UP TO 35 LBS.",
        "O ON A REGULAR AND REPETITIVE BASIS, UNASSISTED",
        "O ABILITY TO WORK WALKING OR STANDING FOR THE DURATION OF THE SERVICE EVENT",
        "O ABILITY TO USE A LADDER SAFELY",
        "O ABILITY TO STAND, SIT, BEND, LIFT, TWIST",
        "O RELIABLE TRANSPORTATION AND VALID DRIVER'S LICENSE"
    };

            // Calculate the maximum width for the text
            double maxWidth = page.Width - 2 * startX;

            // Helper function to split text into lines
            List<string> SplitTextIntoLines(string text)
            {
                List<string> lines = new List<string>();
                string[] words = text.Split(' ');

                string currentLine = "";
                foreach (string word in words)
                {
                    string testLine = currentLine + (currentLine.Length > 0 ? " " : "") + word;
                    if (gfx.MeasureString(testLine, defaultFont).Width <= maxWidth)
                    {
                        currentLine = testLine;
                    }
                    else
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                }
                if (currentLine.Length > 0)
                    lines.Add(currentLine);

                return lines;
            }

            // Draw each paragraph with wrapping
            foreach (string paragraph in paragraphs)
            {
                List<string> lines = SplitTextIntoLines(paragraph);
                foreach (string line in lines)
                {
                    gfx.DrawString(line, defaultFont, XBrushes.Black, new XRect(startX, startY, maxWidth, lineHeight), XStringFormats.TopLeft);
                    startY += lineHeight;
                }
                startY += lineHeight; // Add extra space after a paragraph
            }

            // Draw bullet points
            foreach (string bullet in bulletPoints)
            {
                gfx.DrawString(bullet, blueFont, XBrushes.SteelBlue, new XRect(startX, startY, maxWidth, lineHeight), XStringFormats.TopLeft);
                startY += lineHeight;
            }
        }





        private void AddSectionHeader(XGraphics gfx, PdfPage page, string headerText)
        {
            XFont font = new XFont("Verdana", 14);
            gfx.DrawString(headerText, font, XBrushes.Blue, new XRect(40, 100, page.Width - 80, 30), XStringFormats.TopLeft);

        }

        private void Add3rdpageStaticData(XGraphics gfx, PdfPage page, ref int currentY)
        {

            XFont font = new XFont("Verdana", 8);
            XFont boldFont = new XFont("Verdana", 9, XFontStyleEx.Bold);
            double startX = 40; // Left margin
            double startY = 95; // Top margin
            double lineHeight = 16; // Line height
            double maxWidth = page.Width - 2 * startX; // Maximum width for text

            // Title
            /* gfx.DrawString("PulseSphere Work Order Requirements", boldFont, XBrushes.Black, new XRect(startX, startY, maxWidth, lineHeight), XStringFormats.TopLeft);
             startY += lineHeight * 2;*/

            // Paragraphs with Wrapping
            List<string> paragraphs = new List<string>
    {
        "** IT IS MANDATORY THAT ALL ELECTRONIC DEVICES SHOULD HAVE A CHARGER.",
        "** SOFTWARE MUST BE CURRENT AND MAINTAINED (Excel, TeamViewer, Putty)."
    };

            foreach (var paragraph in paragraphs)
            {
                List<string> wrappedLines = SplitTextIntoLines(paragraph, gfx, font, maxWidth);
                foreach (var line in wrappedLines)
                {
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(startX, startY, maxWidth, lineHeight), XStringFormats.TopLeft);
                    startY += lineHeight;
                }
                startY += lineHeight; // Extra space between paragraphs
            }

            // Section Title
            gfx.DrawString("Standard Technician Tool Kit:", boldFont, XBrushes.Black, new XRect(startX, startY, maxWidth, lineHeight), XStringFormats.TopLeft);
            startY += lineHeight * 1.5;

            // Bullet Points with Wrapping
            List<string> bulletPoints = new List<string>
    {
        "Laptop with Windows 7 or Later Operating System",
        "Microsoft Office",
        "TeamViewer 14",
        "Cisco / ADTRAN Console Cable (USB to DB9)",
        "Mobile Hotspot",
        "1000’ (1) box of Cat 5e/6 Cable",
        "Modular Adapters",
        "Spare Power Strip",
        "Digital Camera / High-End Smartphone",
        "Label Maker",
        "Small Parts - RJ-45 Couplers and Connectors, RJ-11 Connectors, Mounting Equipment (Rack Nuts and Screws), Biscuit Jacks",
        "Hand Tools – Pliers, Multi-Tool, Flashlight, Power Drill, Long Drill Bits, Masonry Bits, General Purpose Drill Bit Set, and a Hole-Saw, Crimpers, Punch Down Tool, Wire Strippers, Screwdrivers (Phillips and Flathead), Tape Measure, Sheetrock Saw, Utility Knife, Torx (Star-Shaped) Bits, Fish Tape and/or Glow Rods, LED Headlight.",
        "Cleaning Tools – Compressed Air Can, Velcro, Tie-Wraps, Magnets, Small Broom or Vacuum",
        "Diagnostic Tools – Multimeter, Loop Back Plug."
    };

            foreach (var bullet in bulletPoints)
            {
                // Wrap bullet text
                List<string> wrappedBulletLines = SplitTextIntoLines(bullet, gfx, font, maxWidth - 20); // Adjust for bullet margin
                gfx.DrawString("•", font, XBrushes.Black, new XRect(startX, startY, 20, lineHeight), XStringFormats.TopLeft); // Bullet symbol

                foreach (var line in wrappedBulletLines)
                {
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(startX + 20, startY, maxWidth - 20, lineHeight), XStringFormats.TopLeft);
                    startY += lineHeight;

                    /* // Add a new page if content overflows
                     if (startY > page.Height - lineHeight * 3)
                     {
                         page = gfx.PdfDocument.AddPage();
                         gfx = XGraphics.FromPdfPage(page);
                         startY = 40; // Reset start position for the new page
                     }*/
                }
            }


            // Section Title
            gfx.DrawString("Programs and Apps:", boldFont, XBrushes.Black, new XRect(startX, startY, maxWidth, lineHeight), XStringFormats.TopLeft);
            startY += lineHeight * 1.5;

            // Bullet Points with Wrapping
            List<string> bulletPoints1 = new List<string>
    {
        "InSSIDer / NetSpot / Wifi Analyzer" ,
        "Microsoft Excel Must *** (Surveys)" ,
        "Putty (Terminal Emulator for consoling into devices) ",
        "Adobe"
    };

            foreach (var bullet in bulletPoints1)
            {
                // Wrap bullet text
                List<string> wrappedBulletLines = SplitTextIntoLines(bullet, gfx, font, maxWidth - 20); // Adjust for bullet margin
                gfx.DrawString("•", font, XBrushes.Black, new XRect(startX, startY, 20, lineHeight), XStringFormats.TopLeft); // Bullet symbol

                foreach (var line in wrappedBulletLines)
                {
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(startX + 20, startY, maxWidth - 20, lineHeight), XStringFormats.TopLeft);
                    startY += lineHeight;

                    /* // Add a new page if content overflows
                     if (startY > page.Height - lineHeight * 3)
                     {
                         page = gfx.PdfDocument.AddPage();
                         gfx = XGraphics.FromPdfPage(page);
                         startY = 40; // Reset start position for the new page
                     }*/
                }
            }


            // Section Title
            gfx.DrawString("Troubleshooting:", boldFont, XBrushes.Black, new XRect(startX, startY, maxWidth, lineHeight), XStringFormats.TopLeft);
            startY += lineHeight * 1.5;

            // Bullet Points with Wrapping
            List<string> bulletPoints2 = new List<string>
    {
        "Cable Certifier (Advanced tester) bandwidth and frequency.  ",
        "Cable Tester (Basic tester) Continuity, resistance, High/ Low voltage test, wired accurately and can easily identify open and shorts  ",
        "Tone and Probe (Service/Survey)"

    };

            foreach (var bullet in bulletPoints2)
            {
                // Wrap bullet text
                List<string> wrappedBulletLines = SplitTextIntoLines(bullet, gfx, font, maxWidth - 20); // Adjust for bullet margin
                gfx.DrawString("•", font, XBrushes.Black, new XRect(startX, startY, 20, lineHeight), XStringFormats.TopLeft); // Bullet symbol

                foreach (var line in wrappedBulletLines)
                {
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(startX + 20, startY, maxWidth - 20, lineHeight), XStringFormats.TopLeft);
                    startY += lineHeight;

                    /* // Add a new page if content overflows
                     if (startY > page.Height - lineHeight * 3)
                     {
                         page = gfx.PdfDocument.AddPage();
                         gfx = XGraphics.FromPdfPage(page);
                         startY = 40; // Reset start position for the new page
                     }*/
                }
            }


            // Section Title
            gfx.DrawString("Specialty Items:", boldFont, XBrushes.Black, new XRect(startX, startY, maxWidth, lineHeight), XStringFormats.TopLeft);
            startY += lineHeight * 1.5;

            // Bullet Points with Wrapping
            List<string> bulletPoints3 = new List<string>
    {
        "12’ Ladder Extension ladder (Little Giant System)  ",
        "Hammer Drill ", "Masonry Anchor Kit  ",
        "HDMI cables ","Monitor", "Keyboard/Mouse ","Wheeled cart (Deinstalls) ","ESD tools - antistatic wrist strap and the antistatic mat (used when opening equipment)","Sawsall or Circular Saw (Backboard Installs) "

    };

            foreach (var bullet in bulletPoints3)
            {
                // Wrap bullet text
                List<string> wrappedBulletLines = SplitTextIntoLines(bullet, gfx, font, maxWidth - 20); // Adjust for bullet margin
                gfx.DrawString("•", font, XBrushes.Black, new XRect(startX, startY, 20, lineHeight), XStringFormats.TopLeft); // Bullet symbol

                foreach (var line in wrappedBulletLines)
                {
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(startX + 20, startY, maxWidth - 20, lineHeight), XStringFormats.TopLeft);
                    startY += lineHeight;

                    /* // Add a new page if content overflows
                     if (startY > page.Height - lineHeight * 3)
                     {
                         page = gfx.PdfDocument.AddPage();
                         gfx = XGraphics.FromPdfPage(page);
                         startY = 40; // Reset start position for the new page
                     }*/
                }
            }
            currentY += 30;
            // Additional sections like "Programs and Apps" or "Troubleshooting" can follow the same pattern.
        }






        private List<string> SplitTextIntoLines(string text, XGraphics gfx, XFont font, double maxWidth)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');

            string currentLine = "";
            foreach (string word in words)
            {
                string testLine = currentLine + (currentLine.Length > 0 ? " " : "") + word;
                if (gfx.MeasureString(testLine, font).Width <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }
            if (!string.IsNullOrWhiteSpace(currentLine))
                lines.Add(currentLine);

            return lines;
        }

        private void AddTechnicianTable(XGraphics gfx, PdfPage page, ref int currentY)
        {
            XFont attributeFont = new XFont("Verdana", 10, XFontStyleEx.Regular);
            XFont valueFont = new XFont("Verdana", 10);

            int xStart = 50;
            int cellWidth = (int)((page.Width - 2 * xStart) / 6); // 6 columns
            int cellHeight = 25;

            var attributes = new string[]
            {
        "Technician Name", "Travel Time", "Arrival Time",
        "Technician Phone", "Miles Driven", "Departure Time"
            };

            var values = new string[]
            {
        txtTechnicianName.Text, "", "",
        txtTechnicianPhone.Text, "", ""
            };

            for (int row = 0; row < 2; row++)
            {
                int currentX = xStart;
                for (int col = 0; col < 3; col++)
                {
                    int index = row * 3 + col;

                    // Draw attribute
                    DrawWrappedText(gfx, attributes[index] + ":", attributeFont, currentX, currentY, cellWidth, cellHeight);
                    currentX += cellWidth;

                    // Draw value
                    DrawWrappedText(gfx, values[index], valueFont, currentX, currentY, cellWidth, cellHeight);
                    currentX += cellWidth;
                }

                currentY += cellHeight; // Move to the next row
            }

            currentY += 40; // Add spacing after the table
        }

        private void DrawWrappedText(XGraphics gfx, string text, XFont font, int x, int y, int width, int height)
        {
            // Draw the cell border
            gfx.DrawRectangle(XPens.Black, x, y, width, height);

            string[] words = text.Split(' ');
            string line = string.Empty;
            int lineHeight = (int)gfx.MeasureString("Sample", font).Height;
            int currentY = y;

            foreach (string word in words)
            {
                string testLine = line + word + " ";
                double testWidth = gfx.MeasureString(testLine, font).Width;

                if (testWidth > width) // Wrap to next line
                {
                    gfx.DrawString(line.TrimEnd(), font, XBrushes.Black, new XRect(x + 5, currentY, width, lineHeight), XStringFormats.TopLeft);
                    line = word + " ";
                    currentY += lineHeight;

                    // Stop if text goes out of the cell height
                    if (currentY + lineHeight > y + height)
                        break;
                }
                else
                {
                    line = testLine;
                }
            }

            // Draw the last line (if any)
            if (currentY + (int)gfx.MeasureString("Sample", font).Height <= y + height)
            {
                gfx.DrawString(line.TrimEnd(), font, XBrushes.Black, new XRect(x + 5, currentY, width, lineHeight), XStringFormats.TopLeft);
            }
        }

        private void AddLastTable(XGraphics gfx, PdfPage page, ref int currentY)
        {
            XFont attributeFont = new XFont("Verdana", 10, XFontStyleEx.Regular);
            XFont valueFont = new XFont("Verdana", 10);

            int xStart = 50;
            int cellWidth = (int)((page.Width - 2 * xStart) / 3); // 3 columns for the second row
            int cellHeight = 25;

            // First row with one column
            DrawWrappedText(gfx, "Technician Signature:", attributeFont, xStart, currentY, cellWidth * 3, cellHeight);
            currentY += cellHeight; // Move to the next row

            // Second row with three columns
            var attributes = new string[]
            {
        "Customer Name", "Customer Signature", "Date"
            };

            for (int col = 0; col < 3; col++)
            {
                int currentX = xStart + col * cellWidth;

                // Draw attribute
                DrawWrappedText(gfx, attributes[col] + ":", attributeFont, currentX, currentY, cellWidth, cellHeight);
            }

            currentY += cellHeight; // Move to the next row

            currentY += 10; // Add spacing after the table
        }

        private void SendEmail_Load(object sender, EventArgs e)
        {
            dateScheduled.Format = DateTimePickerFormat.Custom;
            dateScheduled.CustomFormat = Form1.dateFormat;
     
        }
    }
}
