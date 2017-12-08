using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Core.AESEncryptStringSample;

namespace Core.CookieMgr
{
    public static class CookieManager
    {
        static string passPhrase = "123@lkjd;";
        public static string GetConnectionString()
        {
            if (System.Web.HttpContext.Current.Request.Cookies["DCS"] != null)
            {
                string Ct = (string)System.Web.HttpContext.Current.Request.Cookies["DCS"].Value;

                string Pt = AESStringCrypto.Decrypt(Ct, passPhrase);

                return Pt;
            }
            else
                return string.Empty;
        }


        public static UserPreferences GetUserPreferences()
        {
            if (System.Web.HttpContext.Current.Request.Cookies["UserPref"] != null)
            {
                var Cookie = System.Web.HttpContext.Current.Request.Cookies["UserPref"].Values;
                UserPreferences upObj = new UserPreferences();
                upObj.SiteId = Convert.ToInt32(AESStringCrypto.Decrypt(Cookie["SId"], passPhrase));
                upObj.DivisionId = Convert.ToInt32(AESStringCrypto.Decrypt(Cookie["DId"], passPhrase));
                return upObj;
            }
            else
                return null;
        }

        public static void SetConnectionString(string Conn)
        {
            HttpCookie aCookie = new HttpCookie("DCS");
            aCookie.Value = AESStringCrypto.Encrypt(Conn, passPhrase);
            aCookie.Expires = DateTime.Now.AddYears(1);
            aCookie.Domain = "localhost";
            aCookie.Secure = true;
            aCookie.HttpOnly = false;
            System.Web.HttpContext.Current.Response.Cookies.Add(aCookie);
        }


        public static void SetUserPreferences(int SId, int DId)
        {
            HttpCookie aCookie = new HttpCookie("UserPref");
            aCookie.Values["SId"] = AESStringCrypto.Encrypt(SId.ToString(), passPhrase);
            aCookie.Values["DId"] = AESStringCrypto.Encrypt(DId.ToString(), passPhrase);
            aCookie.Expires = DateTime.Now.AddYears(1);
            aCookie.Domain = "localhost";
            aCookie.Secure = true;
            aCookie.HttpOnly = false;
            System.Web.HttpContext.Current.Response.Cookies.Add(aCookie);
        }

        public static void DisposeCookies()
        {
            HttpCookie aCookie = new HttpCookie("UserPref");
            HttpCookie aCookie2 = new HttpCookie("DCS");
            aCookie.Expires = DateTime.Now.AddDays(-1);
            aCookie2.Expires = DateTime.Now.AddDays(-1);
            System.Web.HttpContext.Current.Response.Cookies.Add(aCookie);
            System.Web.HttpContext.Current.Response.Cookies.Add(aCookie2);
        }

    }
}
