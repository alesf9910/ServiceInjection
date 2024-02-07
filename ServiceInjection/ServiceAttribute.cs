using System;

namespace ServiceInjection
{
    [AttributeUsage(AttributeTargets.Class)]   
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(ServiceType serviceType){}
        public ServiceAttribute(ServiceType serviceType, string interfaceName, string key = null){}
    }
}
