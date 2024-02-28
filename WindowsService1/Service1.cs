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
            SaveToFile("Service Started in " + DateTime.Now);

            timer = new Timer();
            timer.Interval = 12 * 60 * 60 * 1000;
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            string workloadData = GetPCWorkload();

            SaveToFile(workloadData);
            SendEmail(workloadData);
        }

        private string GetPCWorkload()
        {
            // Collect operating system workload data (CPU, Memory, HDD, Network)
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            PerformanceCounter hddCounter = new PerformanceCounter("LogicalDisk", "% Free Space", "C:");

            float cpuUsage = cpuCounter.NextValue();
            float availableMemory = memoryCounter.NextValue();
            float hddFreeSpace = hddCounter.NextValue();

            // Format the data
            string workloadData = $"CPU Usage: {cpuUsage}%\nMemory Available: {availableMemory} MB\nHDD Free Space: {hddFreeSpace}%\n";
            return workloadData;
        }

        private void SaveToFile(string data)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            // Save workload data to a text file
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(data);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(data);
                }
            }
        }

        private void SendEmail(string workloadData)
        {

            string smtpServer;
            int smtpPort;
            string smtpUsername;
            string smtpPassword;
            string senderEmail;
            string recipientEmail;
            string subject;


            using (StreamReader sw = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\credentials.txt"))
            {
                smtpServer = sw.ReadLine();
                smtpPort = 587;
                smtpUsername = sw.ReadLine();
                smtpPassword = sw.ReadLine();
                senderEmail = sw.ReadLine();
                recipientEmail = sw.ReadLine();
                subject = "PC Workload Data";
            }


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
            SaveToFile("Service ended in " + DateTime.Now);
            // Stop the timer when the service stops
            timer.Stop();
        }
    }
}
