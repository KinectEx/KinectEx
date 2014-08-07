using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace KinectEx.Json
{
    public class KinectTypeBinder : SerializationBinder
    {
        private static string _realKinectNamespace;
        private static string _realKinectAssembly;
        private static string _jsonKinectNamespace;
        private static string _jsonKinectAssembly;

        private SerializationBinder _defaultBinder;

        static KinectTypeBinder()
        {
#if NOSDK
            var typeinfo = typeof(KinectEx.KinectSDK.Activity).GetTypeInfo();
#elif NETFX_CORE
            var typeinfo = typeof(WindowsPreview.Kinect.Activity).GetTypeInfo();
#else
            var typeinfo = typeof(Microsoft.Kinect.Activity).GetTypeInfo();
#endif
            _realKinectNamespace = typeinfo.Namespace;
            _realKinectAssembly = typeinfo.Assembly.FullName;
            _jsonKinectNamespace = "_KinectNamespace";
            _jsonKinectAssembly = "_KinectAssembly";

        }

        public KinectTypeBinder(SerializationBinder defaultBinder)
        {
            _defaultBinder = defaultBinder;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            _defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
            if (typeName.Contains(_realKinectNamespace))
            {
                typeName = typeName.Replace(_realKinectAssembly, _jsonKinectAssembly);
                typeName = typeName.Replace(_realKinectNamespace, _jsonKinectNamespace);
            }
            if (assemblyName.Contains(_realKinectNamespace))
            {
                assemblyName = typeName.Replace(_realKinectAssembly, _jsonKinectAssembly);
            }
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName.Contains(_jsonKinectNamespace))
            {
                typeName = typeName.Replace(_jsonKinectNamespace, _realKinectNamespace);
                typeName = typeName.Replace(_jsonKinectAssembly, _realKinectAssembly);
            }
            return _defaultBinder.BindToType(assemblyName, typeName);
        }
    }
}
