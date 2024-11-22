using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class SiteTicket
{
    public int TicketNumber { get; set; }
    public string SiteAddress { get; set; }
    public string IssueDescription { get; set; }
    public string TechnicianName { get; set; }
    public string TechnicianContact { get; set; }
    public string ArrivalTime { get; set; }
    public string DepartureTime { get; set; }
    public List<string> Photos { get; set; } = new List<string>();
    public string Payment { get; set; }
    public bool IsBilled { get; set; }

    public SiteTicket() { }
}

public class SiteTicketManager
{
    private readonly string _filePath;
    private List<SiteTicket> _cachedTickets; // Cached tickets in memory

    public SiteTicketManager(string filePath)
    {
        _filePath = filePath;
        _cachedTickets = LoadTicketsFromFile();
    }

    // Load tickets from file once and cache in memory
    private List<SiteTicket> LoadTicketsFromFile()
    {
        var tickets = new List<SiteTicket>();

        if (File.Exists(_filePath))
        {
            foreach (var line in File.ReadAllLines(_filePath))
            {
                var columns = line.Split(',');
                var ticket = new SiteTicket
                {
                    TicketNumber = int.Parse(columns[0]),
                    SiteAddress = columns[1],
                    IssueDescription = columns[2],
                    TechnicianName = columns[3],
                    TechnicianContact = columns[4],
                    ArrivalTime = columns[5],
                    DepartureTime = columns[6],
                    Photos = columns[7].Split(';').ToList(),
                    Payment = columns[8],
                    IsBilled = bool.Parse(columns[9])
                };
                tickets.Add(ticket);
            }
        }

        return tickets;
    }

    // Add Ticket
    public void AddTicket(SiteTicket ticket)
    {
        _cachedTickets.Add(ticket); // Add to cached tickets
        var line = $"{ticket.TicketNumber},{ticket.SiteAddress},{ticket.IssueDescription}," +
                   $"{ticket.TechnicianName},{ticket.TechnicianContact}," +
                   $"{ticket.ArrivalTime},{ticket.DepartureTime}," +
                   $"{string.Join(";", ticket.Photos)},{ticket.Payment},{ticket.IsBilled}";

        File.AppendAllText(_filePath, line + Environment.NewLine); // Append to file
    }

    // Get next ticket number
    public int GetNextTicketNumber()
    {
        return _cachedTickets.Any() ? _cachedTickets.Max(t => t.TicketNumber) + 1 : 1;
    }

    // Get all tickets
    public List<SiteTicket> GetAllTickets()
    {
        return _cachedTickets;
    }

    // Update Ticket
    public bool UpdateTicket(SiteTicket updatedTicket)
    {
        var ticket = _cachedTickets.FirstOrDefault(t => t.TicketNumber == updatedTicket.TicketNumber);

        if (ticket != null)
        {
            // Update the in-memory ticket
            ticket.SiteAddress = updatedTicket.SiteAddress;
            ticket.IssueDescription = updatedTicket.IssueDescription;
            ticket.TechnicianName = updatedTicket.TechnicianName;
            ticket.TechnicianContact = updatedTicket.TechnicianContact;
            ticket.ArrivalTime = updatedTicket.ArrivalTime;
            ticket.DepartureTime = updatedTicket.DepartureTime;
            ticket.Photos = updatedTicket.Photos;
            ticket.Payment = updatedTicket.Payment;
            ticket.IsBilled = updatedTicket.IsBilled;

            SaveAllTickets(_cachedTickets); // Update file with changes
            return true;
        }

        return false;
    }

    // Get tickets by ticket number
    public List<SiteTicket> GetTicketsByNumber(int ticketNumber)
    {
        return _cachedTickets.Where(ticket => ticket.TicketNumber == ticketNumber).ToList();
    }

    // Delete Ticket
    public bool DeleteTicket(int ticketNumber)
    {
        var ticket = _cachedTickets.FirstOrDefault(t => t.TicketNumber == ticketNumber);

        if (ticket != null)
        {
            _cachedTickets.Remove(ticket); // Remove from cache
            SaveAllTickets(_cachedTickets); // Save updated list to file
            return true;
        }

        return false;
    }

    // Helper Method: Save all tickets from cache to file
    private void SaveAllTickets(List<SiteTicket> tickets)
    {
        var lines = tickets.Select(ticket =>
            $"{ticket.TicketNumber},{ticket.SiteAddress},{ticket.IssueDescription}," +
            $"{ticket.TechnicianName},{ticket.TechnicianContact}," +
            $"{ticket.ArrivalTime},{ticket.DepartureTime}," +
            $"{string.Join(";", ticket.Photos)},{ticket.Payment},{ticket.IsBilled}"
        );

        File.WriteAllLines(_filePath, lines);
    }

    // Get all billed tickets from cache
    public List<SiteTicket> GetBilledTickets()
    {
        return _cachedTickets.Where(ticket => ticket.IsBilled).ToList();
    }
    public List<SiteTicket> GetUnBilledTickets()
    {
        return _cachedTickets.Where(ticket => !ticket.IsBilled).ToList();
    }
    // Mark a ticket as billed by ticket number
    public bool MarkTicketAsBilled(int ticketNumber)
    {
        var ticket = _cachedTickets.FirstOrDefault(t => t.TicketNumber == ticketNumber);

        if (ticket != null)
        {
            ticket.IsBilled = true; // Mark in cache
            SaveAllTickets(_cachedTickets); // Save updated list to file
            return true;
        }

        return false; // Ticket with the given number not found
    }
}

