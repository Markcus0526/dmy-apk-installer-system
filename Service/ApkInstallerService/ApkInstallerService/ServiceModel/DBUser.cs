using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ApkInstallerService.ServiceLibrary;
using System.Runtime.Serialization;

namespace ApkInstallerService.ServiceModel
{
    #region data models
    public class ApkUser
    {
        public string username { get; set; }
    }

    public enum USERLEVEL
    {
        LEVEL1 = 1,
        LEVEL2
    }

    [DataContract]
    public class LevelUserInfo
    {
        [DataMember(Name = "Id", Order = 1)]
        public long uid { get; set; }

        [DataMember(Name = "Name", Order = 2)]
        public string username { get; set; }
    }
    #endregion

    public class DBUser
    {
        public ApkResponseData LoginUser(ServiceDBDataContext db, String username, String password)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                tbl_user userinfo = (from m in db.tbl_users
                                     where m.deleted == 0 &&
                                     m.username == username
                                     select m).FirstOrDefault();

                if (userinfo != null)
                {

                    tbl_user validuser = (from m in db.tbl_users
                                         where m.deleted == 0 &&
                                         m.username == username &&
                                         m.password == password
                                          select m).FirstOrDefault();

                    if (validuser != null)
                    {
                        DateTime nowtime = DateTime.Now;
                        //String.Format("{0:yyyy-MM-dd HH:mm:ss}", nowtime)
                        result.Result = APKERROR.ERR_SUCCESS;
                        result.Data = new
                        {
                            userlevel = userinfo.userlevel
                        };
                        result.Token = ApkCommon.GetMD5Hash(username + password + ApkCommon.strAuthSalt);
                    }
                    else
                    {
                        result.Result = APKERROR.ERR_INCORRECT_PASSWORD;
                    }
                }
                else
                {
                    result.Result = APKERROR.ERR_LOGIN_INVALIDUSER;
                }
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
            }

            return result;
        }

        public long AuthorizeUser(ServiceDBDataContext db, String token)
        {
            long result = -1;

            try
            {
                IEnumerable<tbl_user> userinfo = (from m in db.tbl_users
                                     where m.deleted == 0
                                     select m);

                foreach (tbl_user item in userinfo)
                {
                    if (ApkCommon.GetMD5Hash(item.username + item.password + ApkCommon.strAuthSalt) == token)
                    {
                        result = item.uid;
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return result;
        }

        public tbl_user GetAuthorizeUser(ServiceDBDataContext db, String token)
        {
            try
            {
                IEnumerable<tbl_user> userinfo = (from m in db.tbl_users
                                                  where m.deleted == 0
                                                  select m);

                foreach (tbl_user item in userinfo)
                {
                    if (ApkCommon.GetMD5Hash(item.username + item.password + ApkCommon.strAuthSalt) == token)
                    {
                        return item;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        public ApkResponseData RetrieveLevelUserList(ServiceDBDataContext db, long userid)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                List<LevelUserInfo> retdata = (from m in db.tbl_users
                                         where m.deleted == 0 && m.parentid == userid
                                               select new LevelUserInfo
                                         {
                                             uid = m.uid,
                                             username = m.username
                                         }).ToList();

                result.Result = APKERROR.ERR_SUCCESS;
                result.Data = retdata;
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
            }

            return result;
        }
    }
}