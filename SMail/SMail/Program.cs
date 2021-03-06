using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace SMail
{
    class Program
    {
        static string Help = @"
Use following arguments:
        -a          From email address and account name
        -p          Account password
        -s          Subject of email
        -b          Body of email
        -f          Attachment file (multiple files add another -f)
        -t          To address (multiple addresses add another -t)
        -cc         CC address (multiple addresses add another -cc)
        -bcc        BCC address (multiple addresses add another -bcc)
";
        static void Main(string[] args)
        {
            try
            {
                var emailAddr = "";
                var password = "";
                var subject = "";
                var body = "";
                var toAddrs = new List<string>();
                var ccAddrs = new List<string>();
                var bccAddrs = new List<string>();
                var attachments = new List<string>();

                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-") && i < args.Length - 1)
                    {
                        switch (args[i].ToLower())
                        {
                            case "-a":
                                emailAddr = CheckArgs(args[i], args[i + 1]);
                                break;
                            case "-p":
                                password = CheckArgs(args[i], args[i + 1]);
                                break;
                            case "-s":
                                subject = CheckArgs(args[i], args[i + 1]);
                                break;
                            case "-b":
                                body = CheckArgs(args[i], args[i + 1]);
                                break;
                            case "-f":
                                attachments.Add(CheckArgs(args[i], args[i + 1]));
                                break;
                            case "-t":
                                toAddrs.Add(CheckArgs(args[i], args[i + 1]));
                                break;
                            case "-cc":
                                ccAddrs.Add(CheckArgs(args[i], args[i + 1]));
                                break;
                            case "-bcc":
                                bccAddrs.Add(CheckArgs(args[i], args[i + 1]));
                                break;
                            default:
                                Console.WriteLine(Help);
                                Environment.Exit(0);
                                break;
                        }

                    }
                }
                if (emailAddr != "" && password != "")
                {
                    SendEmail(emailAddr, password, toAddrs, subject, body, attachments);
                    Console.WriteLine("Email is sent.");
                    Environment.Exit(0);
                }
                Console.WriteLine("Arguments are not correct.");
                Console.WriteLine(Help);
                Environment.Exit(1);
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void SendEmail(string email, string pwd, List<string> to, string subject, string body, List<string> attachments)
        {
            var client = new SmtpClient("smtp.gmail.com")
            {
                Credentials = new NetworkCredential(email, pwd),
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var message = new MailMessage
            {
                From = new MailAddress(email),
                Subject = subject,
                Body = body
            };

            to.ForEach(addr => {
                message.To.Add(new MailAddress(addr));
            });

            attachments.ForEach(a => {
                message.Attachments.Add(GetAttachment(a));
            });

            try
            {
                client.Send(message);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static string CheckArgs(string arg, string value)
        {
            switch (arg.ToLower()) {
                case "-a":
                    if (IsEmail(value))
                        return value;
                    throw new ArgumentException("Account email address is not correct.");
                case "-t":
                    if (IsEmail(value))
                        return value;
                    throw new ArgumentException("To email address is not correct.");
                case "-cc":
                    if (IsEmail(value))
                        return value;
                    throw new ArgumentException("CC email address is not correct.");
                case "-bcc":
                    if (IsEmail(value))
                        return value;
                    throw new ArgumentException("BCC email address is not correct.");
                case "-p":
                    if (!value.StartsWith("-"))
                        return value;
                    throw new ArgumentException("Password cannot be empty.");
                case "-s":
                    if (!value.StartsWith("-"))
                        return value;
                    return "";
                case "-b":
                    if (!value.StartsWith("-"))
                        return value;
                    return "";
                case "-f":
                    if (File.Exists(value))
                        return value;
                    throw new ArgumentException($"File \"{value}\" does not exist.");
                default:
                    return string.Empty;
            }

        }

        private static bool IsEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static Attachment GetAttachment(string attachmentFile)
        {
            var attachment = new Attachment(attachmentFile, MediaTypeNames.Application.Octet);
            var disposition = attachment.ContentDisposition;
            disposition.CreationDate = File.GetCreationTime(attachmentFile);
            disposition.ModificationDate = File.GetLastWriteTime(attachmentFile);
            disposition.ReadDate = File.GetLastAccessTime(attachmentFile);
            disposition.FileName = Path.GetFileName(attachmentFile);
            disposition.Size = new FileInfo(attachmentFile).Length;
            disposition.DispositionType = DispositionTypeNames.Attachment;
            return attachment;
        }
    }
}
