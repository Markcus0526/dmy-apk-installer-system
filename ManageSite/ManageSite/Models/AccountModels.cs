using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using System.Security.Cryptography;

namespace ManageSite.Models
{

    #region Models

    public class LogOnModel
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [DisplayName("用户名:")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [DisplayName("密码:")]
        public string Password { get; set; }

        [DisplayName("下次自动登录")]
        public bool RememberMe { get; set; }
    }

    #endregion

    public class Foreign_Servant
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "{0}至少为{1}位.";
        private readonly int _minCharacters = 6;

        public ValidatePasswordLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentUICulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }
    }

    public class AccountModel
    {
        private SqlDBDataContext db = new SqlDBDataContext();

        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public tbl_admin ValidateUser(string username, string password)
        {
            string sha1Pswd = GetMD5Hash(password);
            tbl_admin userObj = GetUserObjByUserNameOrMailAddr(username, sha1Pswd);

            if (userObj != null)
                return userObj;
            return null;
        }

        public tbl_admin GetUserObjByUserNameOrMailAddr(string userName, string passWord)
        {
            try
            {
                tbl_admin userinfo = (from m in db.tbl_admins
                                      where (
                                        (m.name.ToLower() == userName.ToLower() ||
                                        m.mailaddress.ToLower() == userName.ToLower()) &&
                                        m.password == passWord &&
                                        m.status == (byte)ADMINSTATUS.APPROVED && m.deleted == 0)
                                      select m).FirstOrDefault();

                if (userinfo != null)
                {
                    return userinfo;
                }
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile(this.GetType().Name, "GetUserObjByUserNameOrMailAddr()", e.ToString());
            }
            return null;
        }

        public static string GetMD5Hash(string value)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public static bool UpdatePersonalInfo(string mailaddr, string password)
        {
            SqlDBDataContext db = CommonModel.GetDBContext();

            string username = new CommonModel().GetCurrentUserName();

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();
                var userinfo = (from m in db.tbl_admins
                                where m.deleted == 0 && m.status == (byte)ADMINSTATUS.APPROVED && m.name == username
                                select m).FirstOrDefault();

                if (userinfo == null) return false;

                string validateStr = "";// UserModel.ValidateUserData(userinfo.uid, userinfo.employee_uid, "", mailaddr);

                if (validateStr != ADMIN_SUBMITSTATUS.SUCCESS_SUBMIT)
                {
                    return false;
                }

                userinfo.mailaddress = mailaddr;

                if (password.Length >= 6)
                {
                    userinfo.password = GetMD5Hash(password);

                }

                db.SubmitChanges();
                db.Transaction.Commit();
            }
            catch (Exception e)
            {
                CommonModel.WriteLogFile("AccountModel", "UpdatePersonalInfo()", e.ToString());
                db.Transaction.Rollback();
                return false;
            }

            return true;
        }

        public static string GetUserMailAddr()
        {
            SqlDBDataContext db = CommonModel.GetDBContext();

            string username = new CommonModel().GetCurrentUserName();

            return (from m in db.tbl_admins
                    where m.deleted == 0 && m.name == username
                    select m.mailaddress).FirstOrDefault();
        }

        public static bool ModifyPassword(string password, string mailaddr)
        {
            using (SqlDBDataContext db = CommonModel.GetDBContext())
            {
                try
                {
                    var userinfo = (from m in db.tbl_admins
                                    where m.deleted == 0 && m.status == (byte)ADMINSTATUS.APPROVED && m.mailaddress == mailaddr
                                    select m).FirstOrDefault();

                    if (userinfo == null)
                    {
                        return true;
                    }

                    userinfo.password = GetMD5Hash(password);

                    db.SubmitChanges();
                    return true;
                }
                catch (Exception e)
                {
                    CommonModel.WriteLogFile("AccountModel", "ModifyPassword()", e.ToString());
                    return false;
                }
            }
        }
    }
}
