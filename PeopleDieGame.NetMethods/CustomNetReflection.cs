using HarmonyLib;
using PeopleDieGame.Reflection;
using SDG.NetPak;
using SDG.Unturned;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PeopleDieGame.NetMethods
{
    public static class CustomNetReflection
    {
        // NetReflection stuff
        private static readonly FieldRef<ClientMethodInfo[]> clientMethods = FieldRef.GetFieldRef<ClientMethodInfo[]>(typeof(NetReflection), "clientMethods");
        private static readonly FieldRef<uint> clientMethodsLength = FieldRef.GetFieldRef<uint>(typeof(NetReflection), "clientMethodsLength");
        private static readonly FieldRef<int> clientMethodsBitCount = FieldRef.GetFieldRef<int>(typeof(NetReflection), "clientMethodsBitCount");
        private static readonly FieldRef<ServerMethodInfo[]> serverMethods = FieldRef.GetFieldRef<ServerMethodInfo[]>(typeof(NetReflection), "serverMethods");
        private static readonly FieldRef<uint> serverMethodsLength = FieldRef.GetFieldRef<uint>(typeof(NetReflection), "serverMethodsLength");
        private static readonly FieldRef<int> serverMethodsBitCount = FieldRef.GetFieldRef<int>(typeof(NetReflection), "serverMethodsBitCount");
        private static readonly FieldRef<int> rateLimitedMethodsCount = FieldRef.GetFieldRef<int>(typeof(NetReflection), "rateLimitedMethodsCount");
        private static readonly MethodRef log = MethodRef.GetMethodRef(typeof(NetReflection), "Log");

        // ClientMethodInfo stuff
        private static readonly FieldInfo client_declaringType = AccessTools.Field(typeof(ClientMethodInfo), "declaringType");
        private static readonly FieldInfo client_debugName = AccessTools.Field(typeof(ClientMethodInfo), "debugName");
        private static readonly FieldInfo client_name = AccessTools.Field(typeof(ClientMethodInfo), "name");
        private static readonly FieldInfo client_customAttribute = AccessTools.Field(typeof(ClientMethodInfo), "customAttribute");
        private static readonly FieldInfo client_readMethod = AccessTools.Field(typeof(ClientMethodInfo), "readMethod");
        private static readonly FieldInfo client_writeMethodInfo = AccessTools.Field(typeof(ClientMethodInfo), "writeMethodInfo");
        private static readonly FieldInfo client_methodIndex = AccessTools.Field(typeof(ClientMethodInfo), "methodIndex");

        // ServerMethodInfo stuff
        private static readonly FieldInfo server_declaringType = AccessTools.Field(typeof(ServerMethodInfo), "declaringType");
        private static readonly FieldInfo server_debugName = AccessTools.Field(typeof(ServerMethodInfo), "debugName");
        private static readonly FieldInfo server_name = AccessTools.Field(typeof(ServerMethodInfo), "name");
        private static readonly FieldInfo server_customAttribute = AccessTools.Field(typeof(ServerMethodInfo), "customAttribute");
        private static readonly FieldInfo server_readMethod = AccessTools.Field(typeof(ServerMethodInfo), "readMethod");
        private static readonly FieldInfo server_writeMethod = AccessTools.Field(typeof(ServerMethodInfo), "writeMethodInfo");
        private static readonly FieldInfo server_rateLimitIndex = AccessTools.Field(typeof(ServerMethodInfo), "rateLimitIndex");
        private static readonly FieldInfo server_methodIndex = AccessTools.Field(typeof(ServerMethodInfo), "methodIndex");

        private static List<Type> processedCustomTypes = new List<Type>();

        public struct GeneratedMethod
        {
            public MethodInfo Info;
            public NetInvokableGeneratedMethodAttribute Attribute;
        }

        public static int RegisterCustomRPCs(Type[] types = null)
        {
            if (types == null)
                types = Assembly.GetExecutingAssembly().GetTypes();

            List<ClientMethodInfo> clientMethodInfos = new List<ClientMethodInfo>();
            List<ServerMethodInfo> serverMethodInfos = new List<ServerMethodInfo>();
            List<GeneratedMethod> readMethods = new List<GeneratedMethod>();
            List<GeneratedMethod> writeMethods = new List<GeneratedMethod>();

            foreach (Type type in types)
            {
                if (processedCustomTypes.Contains(type))
                    continue; // type has already been registered

                if (!type.IsClass || !type.IsAbstract)
                    continue;

                NetInvokableGeneratedClassAttribute classAttribute = type.GetCustomAttribute<NetInvokableGeneratedClassAttribute>();
                if (classAttribute == null)
                    continue;

                readMethods.Clear();
                writeMethods.Clear();

                foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    NetInvokableGeneratedMethodAttribute methodAttribute = methodInfo.GetCustomAttribute<NetInvokableGeneratedMethodAttribute>();
                    if (methodAttribute == null)
                        continue;

                    GeneratedMethod generatedMethod = default(GeneratedMethod);
                    generatedMethod.Info = methodInfo;
                    generatedMethod.Attribute = methodAttribute;
                    ENetInvokableGeneratedMethodPurpose purpose = methodAttribute.purpose;

                    switch (purpose)
                    {
                        case ENetInvokableGeneratedMethodPurpose.Read:
                            readMethods.Add(generatedMethod);
                            break;
                        case ENetInvokableGeneratedMethodPurpose.Write:
                            writeMethods.Add(generatedMethod);
                            break;
                        default:
                            log.Invoke(string.Format("Generated method {0}.{1} unknown purpose {2}", type.Name, methodInfo.Name, methodAttribute.purpose));
                            break;
                    }
                }

                foreach (MethodInfo methodInfo in classAttribute.targetType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
                {
                    SteamCall callAttribute = methodInfo.GetCustomAttribute<SteamCall>();
                    if (callAttribute == null)
                        continue;

                    switch (callAttribute.validation)
                    {
                        case ESteamCallValidation.ONLY_FROM_SERVER:
                            ClientMethodInfo clientMethodInfo = GetClientMethodInfo(type, methodInfo, readMethods, writeMethods);
                            clientMethodInfos.Add(clientMethodInfo);
                            break;
                        case ESteamCallValidation.ONLY_FROM_OWNER:
                        case ESteamCallValidation.SERVERSIDE:
                            ServerMethodInfo serverMethodInfo = GetServerMethodInfo(type, methodInfo, readMethods, writeMethods);
                            serverMethodInfos.Add(serverMethodInfo);
                            break;
                    }
                }

                processedCustomTypes.Add(type);
            }

            // Merge custom RPC calls with stock ones

            if (clientMethodInfos.Count > 0)
            {
                ClientMethodInfo[] clientMethodsArray = clientMethods.Value.AddRangeToArray(clientMethodInfos.ToArray());

                for (uint clientMethodIndex = 0U; clientMethodIndex < clientMethodsArray.Length; clientMethodIndex++)
                {
                    client_methodIndex.SetValue(clientMethodsArray[(int)clientMethodIndex], clientMethodIndex);
                }

                clientMethods.Value = clientMethodsArray;
                clientMethodsLength.Value = (uint)clientMethodsArray.Length;
                clientMethodsBitCount.Value = NetPakConst.CountBits((uint)clientMethodsArray.Length);
            }

            if (serverMethodInfos.Count > 0)
            {
                ServerMethodInfo[] serverMethodsArray = serverMethods.Value.AddRangeToArray(serverMethodInfos.ToArray());

                for (uint serverMethodIndex = 0U; serverMethodIndex < serverMethodsArray.Length; serverMethodIndex++)
                {
                    server_methodIndex.SetValue(serverMethodsArray[serverMethodIndex], serverMethodIndex);
                }

                serverMethods.Value = serverMethodsArray;
                serverMethodsLength.Value = (uint)serverMethodsArray.Length;
                serverMethodsBitCount.Value = NetPakConst.CountBits((uint)serverMethodsArray.Length);
            }

            return clientMethodInfos.Count + serverMethodInfos.Count;
        }

        private static ClientMethodInfo GetClientMethodInfo(Type type, MethodInfo methodInfo, List<GeneratedMethod> readMethods, List<GeneratedMethod> writeMethods)
        {
            SteamCall callAttribute = methodInfo.GetCustomAttribute<SteamCall>();
            if (callAttribute == null)
                throw new Exception("Method did not contain the SteamCall attribute");

            if (callAttribute.validation != ESteamCallValidation.ONLY_FROM_SERVER)
                throw new Exception("Method was not a client method");

            ParameterInfo[] parameters = methodInfo.GetParameters();

            ClientMethodInfo clientMethodInfo = new ClientMethodInfo();
            client_declaringType.SetValue(clientMethodInfo, methodInfo.DeclaringType);
            client_debugName.SetValue(clientMethodInfo, string.Format($"{methodInfo.DeclaringType}.{methodInfo.Name}"));
            client_name.SetValue(clientMethodInfo, methodInfo.Name);
            client_customAttribute.SetValue(clientMethodInfo, callAttribute);

            bool isReadMethod = parameters.Length == 1 && parameters[0].ParameterType.GetElementType() == typeof(ClientInvocationContext);
            if (methodInfo.IsStatic && isReadMethod)
            {
                client_readMethod.SetValue(clientMethodInfo, Delegate.CreateDelegate(typeof(ClientMethodReceive), methodInfo, false) as ClientMethodReceive);
            }
            else
            {
                client_readMethod.SetValue(clientMethodInfo, FindClientReceiveMethod(type, readMethods, methodInfo.Name));
                if (!isReadMethod)
                {
                    GeneratedMethod writeMethod;
                    if (FindAndRemoveGeneratedMethod(writeMethods, methodInfo.Name, out writeMethod))
                    {
                        client_writeMethodInfo.SetValue(clientMethodInfo, writeMethod.Info);
                    }
                    else
                    {
                        log.Invoke(string.Concat(new string[] { "Unable to find client ", type.Name, ".", methodInfo.Name, " write implementation" }));
                    }
                }
            }

            return clientMethodInfo;
        }

        private static ServerMethodInfo GetServerMethodInfo(Type type, MethodInfo methodInfo, List<GeneratedMethod> readMethods, List<GeneratedMethod> writeMethods)
        {
            SteamCall callAttribute = methodInfo.GetCustomAttribute<SteamCall>();
            if (callAttribute == null)
                throw new Exception("Method did not contain the SteamCall attribute");

            if (callAttribute.validation != ESteamCallValidation.ONLY_FROM_OWNER && callAttribute.validation != ESteamCallValidation.SERVERSIDE)
                throw new Exception("Method was not a server method");

            ParameterInfo[] parameters = methodInfo.GetParameters();

            ServerMethodInfo serverMethodInfo = new ServerMethodInfo();
            server_declaringType.SetValue(serverMethodInfo, methodInfo.DeclaringType);
            server_name.SetValue(serverMethodInfo, methodInfo.Name);
            server_debugName.SetValue(serverMethodInfo, $"{methodInfo.DeclaringType}.{methodInfo.Name}");
            server_customAttribute.SetValue(serverMethodInfo, callAttribute);

            bool isReadMethod = parameters.Length == 1 && parameters[0].ParameterType.GetElementType() == typeof(ServerInvocationContext);
            if (methodInfo.IsStatic && isReadMethod)
            {
                server_readMethod.SetValue(serverMethodInfo, Delegate.CreateDelegate(typeof(ServerMethodReceive), methodInfo, false) as ServerMethodReceive);
            }
            else
            {
                server_readMethod.SetValue(serverMethodInfo, FindServerReceiveMethod(type, readMethods, methodInfo.Name));
                if (!isReadMethod)
                {
                    GeneratedMethod writeMethod;
                    if (FindAndRemoveGeneratedMethod(writeMethods, methodInfo.Name, out writeMethod))
                    {
                        server_writeMethod.SetValue(serverMethodInfo, writeMethod.Info);
                    }
                    else
                    {
                        log.Invoke(string.Concat(new string[] { "Unable to find server ", type.Name, ".", methodInfo.Name, " write implementation" }));
                    }
                }
            }

            if (callAttribute.ratelimitHz > 0)
            {
                server_rateLimitIndex.SetValue(serverMethodInfo, rateLimitedMethodsCount.Value);
                callAttribute.rateLimitIndex = rateLimitedMethodsCount.Value;
                callAttribute.ratelimitSeconds = 1f / callAttribute.ratelimitHz;
                rateLimitedMethodsCount.Value++;
            }
            else
            {
                server_rateLimitIndex.SetValue(serverMethodInfo, -1);
            }

            return serverMethodInfo;
        }

        private static bool FindAndRemoveGeneratedMethod(List<GeneratedMethod> generatedMethods, string methodName, out GeneratedMethod foundMethod)
        {
            for (int i = generatedMethods.Count - 1; i >= 0; i--)
            {
                GeneratedMethod generatedMethod = generatedMethods[i];
                if (generatedMethod.Attribute.targetMethodName == methodName)
                {
                    generatedMethods.RemoveAtFast(i);
                    foundMethod = generatedMethod;
                    return true;
                }
            }
            foundMethod = default(GeneratedMethod);
            return false;
        }

        private static ClientMethodReceive FindClientReceiveMethod(Type generatedType, List<GeneratedMethod> generatedMethods, string methodName)
        {
            GeneratedMethod generatedMethod;
            if (FindAndRemoveGeneratedMethod(generatedMethods, methodName, out generatedMethod))
            {
                try
                {
                    return (ClientMethodReceive)generatedMethod.Info.CreateDelegate(typeof(ClientMethodReceive));
                }
                catch
                {
                    log.Invoke(string.Concat(new string[] { "Exception creating delegate for client ", generatedType.Name, ".", methodName, " receive implementation" }));
                    return null;
                }
            }
            log.Invoke(string.Concat(new string[] { "Unable to find client ", generatedType.Name, ".", methodName, " receive implementation" }));
            return null;
        }

        private static ServerMethodReceive FindServerReceiveMethod(Type generatedType, List<GeneratedMethod> generatedMethods, string methodName)
        {
            GeneratedMethod generatedMethod;
            if (FindAndRemoveGeneratedMethod(generatedMethods, methodName, out generatedMethod))
            {
                try
                {
                    return (ServerMethodReceive)generatedMethod.Info.CreateDelegate(typeof(ServerMethodReceive));
                }
                catch
                {
                    log.Invoke(string.Concat(new string[] { "Exception creating delegate for server ", generatedType.Name, ".", methodName, " receive implementation" }));
                    return null;
                }
            }
            log.Invoke(string.Concat(new string[] { "Unable to find server ", generatedType.Name, ".", methodName, " receive implementation" }));
            return null;
        }

        public static ServerMethodInfo GetServerMethodInfo(Type declaringType, string methodName)
        {
            foreach (ServerMethodInfo serverMethodInfo in NetReflection.serverMethods)
            {
                Type type = server_declaringType.GetValue(serverMethodInfo) as Type;
                string name = server_name.GetValue(serverMethodInfo) as string;

                if (type == declaringType && name.Equals(methodName, StringComparison.Ordinal))
                {
                    return serverMethodInfo;
                }
            }
            log.Invoke("Unable to find server method info for " + declaringType.Name + "." + methodName);
            return null;
        }
    }
}
