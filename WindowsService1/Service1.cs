using System;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Start the timer when the service starts
            timer = new Timer();
            timer.Interval = TimeSpan.FromHours(12).TotalMilliseconds; // 12 hours interval
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Collect PC workload data and save it to a text file
            string workloadData = GetPCWorkload();
            SaveToFile(workloadData);

            // Send an email with the attached text file
            SendEmail(workloadData);
        }

        private string GetPCWorkload()
        {
            // Collect operating system workload data (CPU, Memory, HDD, Network)
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            PerformanceCounter hddCounter = new PerformanceCounter("LogicalDisk", "% Free Space", "C:");
            PerformanceCounter networkCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", "your_network_interface_name");

            float cpuUsage = cpuCounter.NextValue();
            float availableMemory = memoryCounter.NextValue();
            float hddFreeSpace = hddCounter.NextValue();
            float networkUsage = networkCounter.NextValue();

            // Format the data
            string workloadData = $"CPU Usage: {cpuUsage}%\nMemory Available: {availableMemory} MB\nHDD Free Space: {hddFreeSpace}%\nNetwork Usage: {networkUsage} Bytes/sec";

            return workloadData;
        }

        private void SaveToFile(string data)
        {
            // Save workload data to a text file
            string filePath = "C:\\WorkloadData.txt";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(data);
            }
        }

        private void SendEmail(string workloadData)
        {
            // Email configuration
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUsername = "hereismyemail.abdullah@gmail.com";
            string smtpPassword = "IwillNotWritethePassword";
            string senderEmail = "hereismyemail.abdullah@gmail.com";
            string recipientEmail = "sendeeeeeeeer@gmail.com";
            string subject = "PC Workload Data";

            // Create email message
            MailMessage mail = new MailMessage(senderEmail, recipientEmail, subject, workloadData);

            // Create SMTP client
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            smtpClient.EnableSsl = true;

            // Send email
            smtpClient.Send(mail);
        }

        protected override void OnStop()
        {
            // Stop the timer when the service stops
            timer.Stop();
        }
    }
}
