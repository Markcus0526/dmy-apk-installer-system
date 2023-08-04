using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ApkInstallerService.ServiceLibrary;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Data.Linq;

namespace ApkInstallerService.ServiceModel
{
    #region data models
    [DataContract]
    public class ApkGroupInfo
    {
        [DataMember(Name = "Id", Order = 1)]
        public long uid { get; set; }

        [DataMember(Name = "Name", Order = 2)]
        public string name { get; set; }
        
        [DataMember(Name = "ImgPath", Order = 3)]
        public string imgpath { get; set; }

        [DataMember(Name = "CreateTime", Order = 4)]
        public string createtime { get; set; }
    }

    [DataContract]
    public class ApkInfo
    {
        [DataMember(Name = "Id", Order = 1)]
        public long uid { get; set; }

        [DataMember(Name = "GroupId", Order = 2)]
        public string groupids { get; set; }

        [DataMember(Name = "Name", Order = 3)]
        public string name { get; set; }

        [DataMember(Name = "FilePath", Order = 4)]
        public string filepath { get; set; }

        [DataMember(Name = "ImgPath", Order = 5)]
        public string imgpath { get; set; }

        [DataMember(Name = "FileSize", Order = 6)]
        public long? filesize { get; set; }

        [DataMember(Name = "Version", Order = 7)]
        public string version { get; set; }

        [DataMember(Name = "CreateTime", Order = 8)]
        public string createtime { get; set; }
    }

    [DataContract]
    public class ApkInstallLog
    {
        [DataMember(Name = "Id", Order = 1)]
        public long uid { get; set; }

        [DataMember(Name = "ApkName", Order = 2)]
        public string apkname { get; set; }

        [DataMember(Name = "TelVendor", Order = 3)]
        public string telvendor { get; set; }

        [DataMember(Name = "TelType", Order = 4)]
        public string teltype { get; set; }

        [DataMember(Name = "AndroidID", Order = 5)]
        public string androidid { get; set; }

        [DataMember(Name = "ApkId", Order = 6)]
        public long apkid { get; set; }

        [DataMember(Name = "TelId", Order = 7)]
        public long telid { get; set; }

        [DataMember(Name = "UserId", Order = 8)]
        public long userid { get; set; }

        [DataMember(Name = "ImgPath", Order = 9)]
        public string imgpath { get; set; }

        [DataMember(Name = "CreateTime", Order = 10)]
        public string createtime { get; set; }
    }

    public class TelAppInfo
    {
        public String appname;
        public String appversion;
        public DateTime installtime;
    }

    public enum USERQUERYLEVEL
    {
        LEVELALL = -100,
        LEVEL2ALL,
        ONESELF
    }

    public enum LOGTYPE
    {
        INSTALL,
        UNINSTALL,
        UPGRADE
    }

    public enum GROUPTYPE
    {
        PUBLIC,
        PRIVATE
    }

    #endregion
    
    public class DBApk
    {
        public ApkResponseData RetrieveGroupList(ServiceDBDataContext db, long userid)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                List<ApkGroupInfo> retData = new List<ApkGroupInfo>();
                List<tbl_apkgroup> tmpGroup = (from m in db.tbl_apkgroups
                                              where m.deleted == 0
                                              select m).ToList();

                foreach (tbl_apkgroup group in tmpGroup)
                {
                    ApkGroupInfo newitem = new ApkGroupInfo();
                    if (group.gtype == (byte)GROUPTYPE.PUBLIC)
                    {
                        newitem.uid = group.uid;
                        newitem.name = group.name;
                        newitem.imgpath = group.imgpath;
                        newitem.createtime = String.Format("{0:yyyy-MM-dd HH:mm:ss}", group.createtime);
                        retData.Add(newitem);
                    }
                    else if (group.gtype == (byte)GROUPTYPE.PRIVATE)
                    {
                        if (group.userids.Split(',').Contains(userid.ToString()))
                        {
                            newitem.uid = group.uid;
                            newitem.name = group.name;
                            newitem.imgpath = group.imgpath;
                            newitem.createtime = String.Format("{0:yyyy-MM-dd HH:mm:ss}", group.createtime);
                            retData.Add(newitem);
                        }
                    }
                }

                result.Result = APKERROR.ERR_SUCCESS;
                result.Data = retData;
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
            }

            return result;
        }

        public ApkResponseData RetrieveApkList(ServiceDBDataContext db, long groupid)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                List<ApkInfo> retdata = (from m in db.tbl_apkinfos
                                            where m.deleted == 0 /*&& m.groupids.Split(',').Contains(groupid.ToString())*/
                                              select new ApkInfo
                                              {
                                                  uid = m.uid,
                                                  groupids = m.groupids,
                                                  name = m.name,
                                                  filepath = m.filepath,
                                                  imgpath = m.imgpath,
                                                  filesize = m.filesize,
                                                  version = m.version,
                                                  createtime = String.Format("{0:yyyy-MM-dd HH:mm:ss}", m.createtime)
                                              }).ToList();

                retdata = retdata.AsQueryable().Where(m => m.groupids.Split(',').Contains(groupid.ToString())).ToList();

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

        public ApkResponseData RetrieveApkInstallLog(ServiceDBDataContext db, tbl_user curruser, long puid)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                List<tbl_user> users = (from m in db.tbl_users
                                        where m.deleted == 0
                                        select m).ToList();

                IQueryable<ApkInstallLog> retdata = db.tbl_apkinstalllogs
                    .Where(p => p.deleted == 0 &&
                        (p.logtype == (byte)LOGTYPE.INSTALL || p.logtype == (byte)LOGTYPE.UPGRADE) &&
                        p.status == 0)
                    .Join(db.tbl_users, m => m.userid, l => l.uid, (m, l) => new { installlog = m, user = l })
                    .Join(db.tbl_apkinfos, m => m.installlog.apkid, l => l.uid, (m, l) => new { log = m, apk = l })
                    .Join(db.tbl_telinfos, m => m.log.installlog.telid, l => l.uid, (m, l) => new { log = m, tel = l })
                    .OrderByDescending(p => p.log.log.installlog.createtime)
                    .Select(row => new ApkInstallLog
                    {
                        uid = row.log.log.installlog.uid,
                        apkid = row.log.apk.uid,
                        userid = row.log.log.installlog.userid,
                        telid = row.tel.uid,
                        apkname = row.log.apk.name,
                        telvendor = row.tel.vendor,
                        teltype = row.tel.type,
                        androidid = row.tel.serial,
                        imgpath = row.log.apk.imgpath,
                        createtime = String.Format("{0:yyyy-MM-dd HH:mm:ss}", row.log.log.installlog.createtime)
                    });

                if (curruser.userlevel == (byte)USERLEVEL.LEVEL1)
                {
                    if (puid == (long)USERQUERYLEVEL.LEVELALL)
                    {
                        List<long> level2users = (from m in db.tbl_users
                                                          where m.deleted == 0 && m.parentid == curruser.uid
                                                          select m.uid).ToList();

                        result.Data = retdata.Where(m => (m.userid == curruser.uid || level2users.Contains(m.userid)) &&
                                db.tbl_apkinstalllogs
                                .Where(s =>
                                    s.apkid == m.apkid && s.telid == m.telid &&
                                    s.deleted == 0 &&
                                    (s.logtype == (byte)LOGTYPE.INSTALL || s.logtype == (byte)LOGTYPE.UPGRADE) &&
                                    s.status == 0)
                                .OrderBy(s => s.createtime)
                                .Select(row => new
                                {
                                    uid = row.uid,
                                })
                                .FirstOrDefault().uid == m.uid
                            ).ToList();
                    }
                    else if (puid == (long)USERQUERYLEVEL.LEVEL2ALL)
                    {
                        List<long> level2users = (from m in db.tbl_users
                                                  where m.deleted == 0 && m.parentid == curruser.uid
                                                  select m.uid).ToList();

                        result.Data = retdata.Where(m => level2users.Contains(m.userid) && 
                                db.tbl_apkinstalllogs
                                .Where(s =>
                                    s.apkid == m.apkid && s.telid == m.telid &&
                                    s.deleted == 0 &&
                                    (s.logtype == (byte)LOGTYPE.INSTALL || s.logtype == (byte)LOGTYPE.UPGRADE) &&
                                    s.status == 0)
                                .OrderBy(s => s.createtime)
                                .Select(row => new
                                {
                                    uid = row.uid,
                                })
                                .FirstOrDefault().uid == m.uid
                            ).ToList();
                    }
                    else if (puid == (long)USERQUERYLEVEL.ONESELF)
                    {
                        result.Data = retdata.Where(m => m.userid == curruser.uid &&
                                db.tbl_apkinstalllogs
                                .Where(s =>
                                    s.apkid == m.apkid && s.telid == m.telid &&
                                    s.deleted == 0 &&
                                    (s.logtype == (byte)LOGTYPE.INSTALL || s.logtype == (byte)LOGTYPE.UPGRADE) &&
                                    s.status == 0)
                                .OrderBy(s => s.createtime)
                                .Select(row => new
                                {
                                    uid = row.uid,
                                })
                                .FirstOrDefault().uid == m.uid
                            ).ToList();
                    }
                    else
                    {
                        result.Data = retdata.Where(m => m.userid == puid &&
                                db.tbl_apkinstalllogs
                                .Where(s =>
                                    s.apkid == m.apkid && s.telid == m.telid &&
                                    s.deleted == 0 &&
                                    (s.logtype == (byte)LOGTYPE.INSTALL || s.logtype == (byte)LOGTYPE.UPGRADE) &&
                                    s.status == 0)
                                .OrderBy(s => s.createtime)
                                .Select(row => new
                                {
                                    uid = row.uid,
                                })
                                .FirstOrDefault().uid == m.uid
                            ).ToList();
                    }
                }
                else if (curruser.userlevel == (byte)USERLEVEL.LEVEL2)
                {
                    result.Data = retdata.Where(m => m.userid == curruser.uid).ToList();
                }

                result.Result = APKERROR.ERR_SUCCESS;
            }
            catch (System.Exception ex) 
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
            }

            return result;
        }

        public static Func<ServiceDBDataContext, long, long, tbl_apkinstalllog> GetConflictLog
     {
        get
        {
            Func<ServiceDBDataContext, long, long, tbl_apkinstalllog> func =
                CompiledQuery.Compile<ServiceDBDataContext, long, long, tbl_apkinstalllog>((ServiceDBDataContext db, long apkid, long telid)
                    => db.tbl_apkinstalllogs
                            .Where(m =>
                                m.apkid == apkid && m.telid == telid &&
                                m.deleted == 0 &&
                                (m.logtype == (byte)LOGTYPE.INSTALL || m.logtype == (byte)LOGTYPE.UPGRADE) &&
                                m.status == 0)
                            .OrderBy(m => m.createtime)
//                             .Select(row => new
//                             {
//                                 uid = row.uid,
//                             })
                            .FirstOrDefault());
            return func;
        }
    }

        public ApkResponseData AddApkUninstallLog(
            ServiceDBDataContext db,
            String teltype,
            String vendor,
            String serial,
            String imei,
            String apkname,
            String status,
            String note,
            String clientver,
            long userid)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();

                var props = OperationContext.Current.IncomingMessageProperties;
                var endpointProperty = props[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                String remoteipaddr = "";
                if (endpointProperty != null)
                {
                    remoteipaddr = endpointProperty.Address;
                }

                tbl_apkinstalllog newlog = new tbl_apkinstalllog
                {
                    userid = userid,
                    apkname = apkname,
                    logtype = (byte)LOGTYPE.UNINSTALL,
                    createtime = DateTime.Now,
                    clientver = clientver,
                    remoteip = remoteipaddr,
                    note = note,
                };

                newlog.status = byte.Parse(status);

                tbl_telinfo oldtel = (from m in db.tbl_telinfos
                                      where m.deleted == 0 && m.type == teltype && m.vendor == vendor && m.serial == serial && m.imei == imei
                                      select m).FirstOrDefault();

                if (oldtel == null)
                {
                    tbl_telinfo newtel = new tbl_telinfo
                    {
                        type = teltype,
                        vendor = vendor,
                        sysver = "",
                        serial = serial,
                        imei = imei
                    };

                    db.tbl_telinfos.InsertOnSubmit(newtel);
                    db.SubmitChanges();

                    newlog.telid = newtel.uid;
                }
                else
                {
                    newlog.telid = oldtel.uid;
                }

                db.tbl_apkinstalllogs.InsertOnSubmit(newlog);

                db.SubmitChanges();
                db.Transaction.Commit();

                result.Result = APKERROR.ERR_SUCCESS;
                result.Data = null;
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
                db.Transaction.Rollback();
            }
            finally
            {
                db.Connection.Close();
            }

            return result;
        }

        public ApkResponseData AddApkInstallLog(
            ServiceDBDataContext db,
            String teltype,
            String vendor,
            long apkid,
            String serial,
            String imei,
            String status,
            String note,
            String clientver,
            long userid)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();

                var props = OperationContext.Current.IncomingMessageProperties;
                var endpointProperty = props[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                String remoteipaddr = "";
                if (endpointProperty != null)
                {
                    remoteipaddr = endpointProperty.Address;
                }

                tbl_apkinstalllog newlog = new tbl_apkinstalllog
                {
                    userid = userid,
                    apkid = apkid,
                    createtime = DateTime.Now,
                    logtype = (byte)LOGTYPE.INSTALL,
                    clientver = clientver,
                    remoteip = remoteipaddr,
                    note = note,
                };
                newlog.status = byte.Parse(status);

                tbl_telinfo oldtel = (from m in db.tbl_telinfos
                                          where m.deleted == 0 && m.type == teltype && m.vendor == vendor && m.serial == serial && m.imei == imei
                                          select m).FirstOrDefault();

                if (oldtel == null)
                {
                    tbl_telinfo newtel = new tbl_telinfo
                    {
                        type = teltype,
                        vendor = vendor,
                        sysver = "",
                        serial = serial,
                        imei = imei
                    };

                    db.tbl_telinfos.InsertOnSubmit(newtel);
                    db.SubmitChanges();

                    newlog.telid = newtel.uid;
                }
                else
                {
                    newlog.telid = oldtel.uid;
                }

                db.tbl_apkinstalllogs.InsertOnSubmit(newlog);

                db.SubmitChanges();
                db.Transaction.Commit();

                result.Result = APKERROR.ERR_SUCCESS;
                result.Data = null;
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
                db.Transaction.Rollback();
            }
            finally
            {
                db.Connection.Close();
            }

            return result;
        }

        public ApkResponseData AddApkUpdateLog(
            ServiceDBDataContext db,
            String teltype,
            String vendor,
            long apkid,
            String serial,
            String imei,
            String status,
            String note,
            String clientver,
            long userid)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();

                var props = OperationContext.Current.IncomingMessageProperties;
                var endpointProperty = props[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                String remoteipaddr = "";
                if (endpointProperty != null)
                {
                    remoteipaddr = endpointProperty.Address;
                }

                tbl_apkinstalllog newlog = new tbl_apkinstalllog
                {
                    userid = userid,
                    apkid = apkid,
                    createtime = DateTime.Now,
                    logtype = (byte)LOGTYPE.UPGRADE,
                    clientver = clientver,
                    remoteip = remoteipaddr,
                    note = note,
                };
                newlog.status = byte.Parse(status);

                tbl_telinfo oldtel = (from m in db.tbl_telinfos
                                      where m.deleted == 0 && m.type == teltype && m.vendor == vendor && m.serial == serial && m.imei == imei
                                      select m).FirstOrDefault();

                if (oldtel == null)
                {
                    tbl_telinfo newtel = new tbl_telinfo
                    {
                        type = teltype,
                        vendor = vendor,
                        sysver = "",
                        serial = serial,
                        imei = imei
                    };

                    db.tbl_telinfos.InsertOnSubmit(newtel);
                    db.SubmitChanges();

                    newlog.telid = newtel.uid;
                }
                else
                {
                    newlog.telid = oldtel.uid;
                }

                db.tbl_apkinstalllogs.InsertOnSubmit(newlog);

                db.SubmitChanges();
                db.Transaction.Commit();

                result.Result = APKERROR.ERR_SUCCESS;
                result.Data = null;
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
                db.Transaction.Rollback();
            }
            finally
            {
                db.Connection.Close();
            }

            return result;
        }

        public ApkResponseData CheckNewDevice(
            ServiceDBDataContext db,
            String teltype,
            String vendor,
            String serial,
            String imei,
            long userid)
        {
            ApkResponseData result = new ApkResponseData();

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();

                tbl_telinfo oldtel = (from m in db.tbl_telinfos
                                      where m.deleted == 0 && m.type == teltype && m.vendor == vendor && m.serial == serial //&& m.imei == imei
                                      select m).FirstOrDefault();

                if (oldtel == null)
                {
                    tbl_telinfo newtel = new tbl_telinfo
                    {
                        type = teltype,
                        vendor = vendor,
                        sysver = "",
                        serial = serial,
                        imei = imei
                    };

                    db.tbl_telinfos.InsertOnSubmit(newtel);
                    db.SubmitChanges();

                    db.Transaction.Commit();
                    result.Result = APKERROR.ERR_SUCCESS;
                }
                else
                {
                    if (oldtel.imei != imei)
                    {
                        oldtel.imei = imei;
                        db.SubmitChanges();
                        db.Transaction.Commit();
                    }
                    result.Result = APKERROR.ERR_ALREADY_EXISTS_TELAPPS;
                }

                result.Data = null;
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
                db.Transaction.Rollback();
            }
            finally
            {
                db.Connection.Close();
            }

            return result;
        }

        public ApkResponseData AddTelAppInfo(
            ServiceDBDataContext db,
            String teltype,
            String vendor,
            String serial,
            String imei,
            List<TelAppInfo> applist,
            long userid)
        {
            ApkResponseData result = new ApkResponseData();
            List<tbl_telapp> addlist = new List<tbl_telapp>();
            long telid;

            try
            {
                db.Connection.Open();
                db.Transaction = db.Connection.BeginTransaction();

                tbl_telinfo oldtel = (from m in db.tbl_telinfos
                                      where m.deleted == 0 && m.type == teltype && m.vendor == vendor && m.serial == serial && m.imei == imei
                                      select m).FirstOrDefault();

                if (oldtel == null)
                {
                    tbl_telinfo newtel = new tbl_telinfo
                    {
                        type = teltype,
                        vendor = vendor,
                        sysver = "",
                        serial = serial,
                        imei = imei
                    };

                    db.tbl_telinfos.InsertOnSubmit(newtel);
                    db.SubmitChanges();

                    telid = newtel.uid;

                    foreach (var item in applist)
                    {
                        tbl_telapp newapp = new tbl_telapp
                        {
                            telid = telid,
                            appname = item.appname,
                            appversion = item.appversion,
                            installtime = item.installtime
                        };

                        addlist.Add(newapp);
                    }

                    db.tbl_telapps.InsertAllOnSubmit(addlist);
                    db.SubmitChanges();
                    db.Transaction.Commit();
                    result.Result = APKERROR.ERR_SUCCESS;
                }
                else
                {
                    foreach (var item in applist)
                    {
                        tbl_telapp newapp = new tbl_telapp
                        {
                            telid = oldtel.uid,
                            appname = item.appname,
                            appversion = item.appversion,
                            installtime = item.installtime
                        };

                        addlist.Add(newapp);
                    }

                    db.tbl_telapps.InsertAllOnSubmit(addlist);
                    db.SubmitChanges();
                    db.Transaction.Commit();

                    result.Result = APKERROR.ERR_ALREADY_EXISTS_TELAPPS;
                }

                result.Data = null;
            }
            catch (System.Exception ex)
            {
                ApkCommon.LogErrors(ex.ToString());
                result.Result = APKERROR.ERR_INTERNAL_EXCEPTION;
                db.Transaction.Rollback();
            }
            finally
            {
                db.Connection.Close();
            }

            return result;
        }
    }
}