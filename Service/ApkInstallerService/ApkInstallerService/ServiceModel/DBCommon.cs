using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Configuration;

namespace ApkInstallerService.ServiceModel
{
    #region Error constants
    public enum APKERROR
    {
        ERR_SUCCESS = 0, //Success
        ERR_LOGIN_INVALIDUSER = 100,    //incorrect user
        ERR_INCORRECT_PASSWORD,    //incorrect password
        ERR_APK_NOTEXIST = 120,
        ERR_INTERNAL_EXCEPTION = 500,
        ERR_PARSE_FAIL,
        ERR_INVALID_USER,
        ERR_ALREADY_EXISTS_TELAPPS,
        ERR_NOTEXIST_UPDATE
    }
    #endregion

    #region [DataContract] - Common
    [DataContract]
    [KnownType(typeof(List<ApkUser>))]
    [KnownType(typeof(List<ApkGroupInfo>))]
    [KnownType(typeof(List<ApkInfo>))]
    [Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class ApkResponseData
    {
        [DataMember(Name = "SVCC_RET", Order = 1)]
        public APKERROR Result { get; set; }

        object retdata = new object();
        [DataMember(Name = "SVCC_DATA", Order = 2), Newtonsoft.Json.JsonProperty]
        public object Data
        {
            get { return retdata; }
            set { retdata = value; }
        }

        String token = "";
        [DataMember(Name = "SVCC_TOKEN", Order = 3)]
        public String Token 
        {
            get { return token; }
            set { token = value; }
        }

        String baseurl = ConfigurationManager.AppSettings["ServerRootUri"];
        [DataMember(Name = "SVCC_BASEURL", Order = 4)]
        public String BaseUrl
        {
            get { return baseurl; }
            set { baseurl = value; }
        }
    }
    #endregion

    #region [DataContract] - Login
    #endregion

    public class DBCommon
    {
        public static String physicalpath = ConfigurationManager.AppSettings["ServerPhysicalPath"];

    }
}