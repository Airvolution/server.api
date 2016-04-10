using Exceptions;
using SendGrid;
using server_api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace server_api.Providers
{
    public class MessageProvider
    {
        public static async Task<bool> SendEmailToUserAsync(User user, string subject, string message)
        {
            
            var myMessage = new SendGridMessage();
            myMessage.AddTo(user.Email);
            myMessage.From = new System.Net.Mail.MailAddress(
                                "air.utah.cs@gmail.com","AirU");
            myMessage.Subject = subject;


            string img = HostingEnvironment.MapPath("~/Images/air_logo_150.png");
            ContentType ctype = new ContentType("image/png");
            var attachment = new Attachment(img, ctype);
            var linkedResource = new LinkedResource(img, ctype);
            myMessage.AddAttachment(attachment.ContentStream, attachment.Name);
            myMessage.EmbedImage(attachment.Name, linkedResource.ContentId);
            message = replaceLogo(message, "<img src=cid:" + linkedResource.ContentId + " />");
            myMessage.Text = message;
            myMessage.Html = message;


            var transportWeb = new Web("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");
            if (transportWeb != null)
            {
                try
                {
                    await transportWeb.DeliverAsync(myMessage);
                    return true;
                }
                catch (InvalidApiRequestException ex)
                {
                    var detail = new StringBuilder();

                    detail.Append("ResponseStatusCode: " + ex.ResponseStatusCode + ".   ");
                    for (int i = 0; i < ex.Errors.Count(); i++)
                    {
                        detail.Append(" -- Error #" + i.ToString() + " : " + ex.Errors[i]);
                    }

                    throw new ApplicationException(detail.ToString(), ex);
                }
            }
            else
            {
                return false;
            }
        }

        public static string replaceLogo(string email,string logo){
            string srcPattern = @"(<img\s+id=""airlogo""[\s\S]+?src="")(.*?)(""[\s\S]+?>)";
            return Regex.Replace(email,srcPattern,logo);
        }
    }
}