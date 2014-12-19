using System;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace KinectEx.Json
{
    /// <summary>
    /// A <c>SerializationBinder</c> that handles the tricky issue of cross-platform
    /// JSON serialization when storing and retrieving objects that come from
    /// different assemblies and namespaces depending on the platform. In this
    /// case, the Kinect SDK classes need to be able to be easily converted to and from 
    /// WindowsPreview.Kinect, Microsoft.Kinect, or KinectEx.KinectSDK.
    /// </summary>
    public class KinectTypeBinder : SerializationBinder
    {
        private static string _realKinectNamespace;
        private static string _realKinectAssembly;
        private static string _jsonKinectNamespace;
        private static string _jsonKinectAssembly;

        private SerializationBinder _defaultBinder;

        /// <summary>
        /// Initializes the <see cref="KinectTypeBinder"/> class.
        /// </summary>
        static KinectTypeBinder()
        {
#if NOSDK
            var typeinfo = typeof(KinectEx.KinectSDK.JointType).GetTypeInfo();
#elif NETFX_CORE
            var typeinfo = typeof(WindowsPreview.Kinect.JointType).GetTypeInfo();
#else
            var typeinfo = typeof(Microsoft.Kinect.JointType).GetTypeInfo();
#endif
            _realKinectNamespace = typeinfo.Namespace;
            _realKinectAssembly = typeinfo.Assembly.FullName;
            _jsonKinectNamespace = "_KinectNamespace";
            _jsonKinectAssembly = "_KinectAssembly";

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectTypeBinder"/> class.
        /// </summary>
        /// <param name="defaultBinder">The default binder.</param>
        public KinectTypeBinder(SerializationBinder defaultBinder)
        {
            _defaultBinder = defaultBinder;
        }

        /// <summary>
        /// Controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            _defaultBinder.BindToName(serializedType, out assemblyName, out typeName);
            if (typeName.Contains(_realKinectNamespace))
            {
                var types = serializedType.GenericTypeArguments;
                if (types.Length > 0)
                {
                    foreach (var type in types)
                    {
                        if (type.Namespace == _realKinectNamespace)
                        {
                            var replaceWith = _jsonKinectNamespace + "." + type.Name + ", " + _jsonKinectAssembly;
                            typeName = typeName.Replace(type.AssemblyQualifiedName, replaceWith);
                        }
                    }
                }
                else
                {
                    typeName = typeName.Replace(_realKinectAssembly, _jsonKinectAssembly);
                    typeName = typeName.Replace(_realKinectNamespace, _jsonKinectNamespace);
                    assemblyName = assemblyName.Replace(_realKinectAssembly, _jsonKinectAssembly);
                }
            }
        }

        /// <summary>
        /// Controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
        /// <returns>
        /// The type of the object the formatter creates a new instance of.
        /// </returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName.Contains(_jsonKinectAssembly))
            {
                assemblyName = assemblyName.Replace(_jsonKinectAssembly, _realKinectAssembly);
            }
            if (typeName.Contains(_jsonKinectNamespace))
            {
                typeName = typeName.Replace(_jsonKinectNamespace, _realKinectNamespace);
                typeName = typeName.Replace(_jsonKinectAssembly, _realKinectAssembly);
            }
            return _defaultBinder.BindToType(assemblyName, typeName);
        }
    }
}
