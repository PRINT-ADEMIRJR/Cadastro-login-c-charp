using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using Microsoft.Win32;
using RegistroDeUsuario.Models;

namespace RegistroDeUsuario.Controllers
{
    public class RegistroController : Controller
    {
        //registro ação
        [HttpGet]
        public ActionResult Registrar()
        {
            return View();
        }

        //registro post ação
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar([Bind(Exclude ="EmailVerificado, CodidoVerificacao")]Registro registro )
        {
            bool status = false;
            string message = "";

            //Model validar
            if (ModelState.IsValid)
            {
                //email já existe
                var jaExiste = oEmailExist(registro.Email);


                #region//O e-mail já existe---
                if (jaExiste)
                {
                    ModelState.AddModelError("EmailExist", "O e-mail já está cadastrado");
                    return View(registro);
                }
                #endregion

                #region//Gerar código de ativação
                registro.CodidoVerificacao = Guid.NewGuid();
                #endregion

                #region//criptografando senha
                registro.Password = Crypto.Hash(registro.Password);
                registro.ConfirmarPassword = Crypto.Hash(registro.ConfirmarPassword);
                #endregion
                registro.EmailVerificado = false;

                #region//salvar no DB

                using (MeuDatabaseEntities dc = new MeuDatabaseEntities())
                {
                    dc.Registro.Add(registro);
                    dc.SaveChanges();

                    //mandar email para usuario
                    mandarLinkDeVerificacaoEmail(registro.Email, registro.CodidoVerificacao.ToString());
                    message = " Conta cadastrada com sucesso. Vá até sua conta de e-mail " + registro.Email
                      + " e clique no link para ativa-la";

                    status = true;
                }
                #endregion


            }
            else
            {

                message = "Pedido inválido";

            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(registro);
        }
        
        //verifica conta
        [HttpGet]
        public ActionResult verificaConta(string id)
        {
            bool status = false;
            using(MeuDatabaseEntities dc = new MeuDatabaseEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; //this line i have added here to avoid
                                                                //confirm password does not match issue on save change
                var v = dc.Registro.Where(a=>a.CodidoVerificacao == new Guid(id)).FirstOrDefault();
                
                if(v != null)
                {
                    v.EmailVerificado = true;
                    status = true;
                }
                else
                {
                    ViewBag.Message = "Pedido inválido";
                }
            }

            ViewBag.Status = status;
            return View();

        }

        //login
        [HttpGet]
        public ActionResult Login()
        {

            return View();
        }
        //login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(RegistroLogin login, string returnUrl="")
        {
            string message = "";

            using (MeuDatabaseEntities dc = new MeuDatabaseEntities())
            {
                var v = dc.Registro.Where(a => a.Email == login.Email).FirstOrDefault();
                if( v != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)//compara pelo hash
                    {
                        int timeOut = login.RememberMe ? 525600 : 20; //um ano
                        var ticket = new FormsAuthenticationTicket(login.Email, login.RememberMe, timeOut);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeOut);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);

                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }

                    }
                    else
                    {
                        message = "A senha não confere";
                    }
                }
                else
                {
                    message = "Usuário inválido";
                }

                
            }


            ViewBag.Message = message;
            return View();
        }

        //logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Registro");
        }

        [NonAction]
        public bool oEmailExist(string email)
        {
            using (MeuDatabaseEntities dc = new MeuDatabaseEntities())
            {
                var v = dc.Registro.Where(a => a.Email == email).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void mandarLinkDeVerificacaoEmail(string email, string codigoDeativacao)
        {
            var verificaUrl = "/Registro/verificaConta/" + codigoDeativacao;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verificaUrl);
            var fromEmail = new MailAddress("jj_lageano@hotmail.com", "Cadastro Online ");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "QCHKqJM8BLpansgZ";//REPLACE WITH ACTUAL PASSWORD

            string subject = "Conta criada com sucesso";

            string body = "<br/><br/>Obrigado por se cadastrar, agora clique no link " +
                "para confirmar sua conta" +
                "<br/><br/><a href='"+ link +"'>"+link+"<a/>";

            var smtp = new SmtpClient
            {
                Host = "smtp-relay.sendinblue.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)


            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true

            })
                smtp.Send(message);
            
        }
        
    }
}