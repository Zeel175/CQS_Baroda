using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Domain;
using System.Linq;

namespace CQSAirborne.Services.Implementation.Utils
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailHistoryRepository _emailHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EmailHelper(IConfiguration configuration, IEmailHistoryRepository emailHistoryRepository, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _emailHistoryRepository = emailHistoryRepository;
            _unitOfWork = unitOfWork;
        }

        public void SendEmail(string To, string Subject, string body, List<string> Bcc = null, List<string> cc = null)
        {
            EmailHistoryEntity entity = new EmailHistoryEntity();
            List<string> invalidEmails = new List<string>();
            try
            {
                // --- Save initial email record ---
                entity.ToEmail = To;
                entity.Subject = Subject;
                entity.CreatedOn = DateTime.Now;
                entity.BccEmail = Bcc == null ? "" : string.Join(",", Bcc);
                entity.CCEmail = cc == null ? "" : string.Join(",", cc);
                entity.IsSuccess = false; // new optional field if you have
                entity.ErrorMessage = null;
                _emailHistoryRepository.Insert(entity);
                _unitOfWork.Commit();

                // --- Prepare and send mail ---
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(_configuration["Email:Host"]);

                mail.From = new MailAddress(_configuration["Email:From"]);

                // To
                if (!string.IsNullOrEmpty(To))
                {
                    string[] toEmails = To.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var toEml in toEmails)
                    {
                        var email = toEml.Trim();
                        if (IsValidEmail(email))
                        {
                            mail.To.Add(email);
                        }
                        else
                        {
                            invalidEmails.Add("TO: " + email);
                        }
                    }
                }

                // Bcc
                if (Bcc != null)
                {
                    foreach (var bccemail in Bcc)
                    {
                        var email = bccemail.Trim();
                        if (IsValidEmail(email))
                        {
                            mail.Bcc.Add(email);
                        }
                        else
                        {
                            invalidEmails.Add("BCC: " + email);
                        }
                    }
                }

                // Cc
                if (cc != null)
                {
                    foreach (var ccEmail in cc)
                    {
                        var email = ccEmail.Trim();
                        if (IsValidEmail(email))
                        {
                            mail.CC.Add(email);
                        }
                        else
                        {
                            invalidEmails.Add("CC: " + email);
                        }
                    }
                }

                mail.Subject = Subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpServer.EnableSsl = _configuration["Email:EnableSsl"] == "true";
                SmtpServer.Port = Convert.ToInt32(_configuration["Email:Port"]);
                var senderEmail = _configuration["Email:Email"];
                var senderPassword = _configuration["Email:Password"];

                if (!string.IsNullOrEmpty(senderEmail) && !string.IsNullOrEmpty(senderPassword))
                {
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new NetworkCredential(senderEmail, senderPassword);
                }
                //bool result = false;

                SmtpServer.Send(mail);

                // --- Mark success ---
                entity.IsSuccess = true;
                entity.ErrorMessage = null;
                entity.SentOn = DateTime.Now; // optional field
                if (invalidEmails.Any())
                {
                    entity.ErrorMessage = "Invalid Email IDs Skipped: " + string.Join(", ", invalidEmails);
                }
                _emailHistoryRepository.Update(entity);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    // Save the actual error message in DB
                    entity.ErrorMessage = ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "");
                    entity.IsSuccess = false;
                    _emailHistoryRepository.Update(entity);
                    _unitOfWork.Commit();
                }
                catch
                {
                    // last resort: swallow DB error to avoid recursive failure
                }

                // optionally rethrow or log somewhere else
                throw;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
