using System;

namespace ServiceInjection.SourceGenerators.Models
{
    public class Service
    {
        public string ClassName { get; set; }
        public string InterfaceName { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(InterfaceName))
            {
                if (!string.IsNullOrEmpty(Key))
                {
                    return $"services.AddKeyed{Type}<{InterfaceName},{ClassName}>(\"{Key}\");";
                }
                return $"services.Add{Type}<{InterfaceName},{ClassName}>();";
            }
            else
            {
                return $"services.Add{Type}<{ClassName}>();";
            }
        }
    }
}
