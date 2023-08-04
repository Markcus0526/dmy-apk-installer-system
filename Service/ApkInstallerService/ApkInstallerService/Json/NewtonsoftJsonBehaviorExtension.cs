using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Configuration;

namespace ApkInstallerService.Json
{
    public class NewtonsoftJsonBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(NewtonsoftJsonBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new NewtonsoftJsonBehavior();
        }
    }
}