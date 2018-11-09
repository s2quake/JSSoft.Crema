﻿//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ntreev.Crema.Services.CremaHostService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.ntreev.com", ConfigurationName="CremaHostService.ICremaHostService", CallbackContract=typeof(Ntreev.Crema.Services.CremaHostService.ICremaHostServiceCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
    internal interface ICremaHostService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/ICremaHostService/GetVersion", ReplyAction="http://www.ntreev.com/ICremaHostService/GetVersionResponse")]
        string GetVersion();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/ICremaHostService/IsOnline", ReplyAction="http://www.ntreev.com/ICremaHostService/IsOnlineResponse")]
        bool IsOnline(string userID, byte[] password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/ICremaHostService/GetDataBaseInfos", ReplyAction="http://www.ntreev.com/ICremaHostService/GetDataBaseInfosResponse")]
        Ntreev.Crema.ServiceModel.DataBaseInfo[] GetDataBaseInfos();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/ICremaHostService/GetServiceInfos", ReplyAction="http://www.ntreev.com/ICremaHostService/GetServiceInfosResponse")]
        Ntreev.Crema.ServiceModel.ServiceInfo[] GetServiceInfos();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/ICremaHostService/Subscribe", ReplyAction="http://www.ntreev.com/ICremaHostService/SubscribeResponse")]
        Ntreev.Crema.ServiceModel.ResultBase<System.Guid> Subscribe(string userID, byte[] password, string version, string platformID, string culture);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/ICremaHostService/Unsubscribe", ReplyAction="http://www.ntreev.com/ICremaHostService/UnsubscribeResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Unsubscribe();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/ICremaHostService/Shutdown", ReplyAction="http://www.ntreev.com/ICremaHostService/ShutdownResponse")]
        Ntreev.Crema.ServiceModel.ResultBase Shutdown(int milliseconds, Ntreev.Crema.ServiceModel.ShutdownType shutdownType, string message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.ntreev.com/ICremaHostService/CancelShutdown", ReplyAction="http://www.ntreev.com/ICremaHostService/CancelShutdownResponse")]
        Ntreev.Crema.ServiceModel.ResultBase CancelShutdown();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal interface ICremaHostServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/ICremaHostService/OnServiceClosed")]
        void OnServiceClosed(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, Ntreev.Crema.ServiceModel.CloseInfo closeInfo);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://www.ntreev.com/ICremaHostService/OnTaskCompleted")]
        void OnTaskCompleted(Ntreev.Crema.ServiceModel.CallbackInfo callbackInfo, System.Guid[] taskIDs);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal interface ICremaHostServiceChannel : Ntreev.Crema.Services.CremaHostService.ICremaHostService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    internal partial class CremaHostServiceClient : System.ServiceModel.DuplexClientBase<Ntreev.Crema.Services.CremaHostService.ICremaHostService>, Ntreev.Crema.Services.CremaHostService.ICremaHostService {
        
        public CremaHostServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public CremaHostServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public CremaHostServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public CremaHostServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public CremaHostServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public string GetVersion() {
            return base.Channel.GetVersion();
        }
        
        public bool IsOnline(string userID, byte[] password) {
            return base.Channel.IsOnline(userID, password);
        }
        
        public Ntreev.Crema.ServiceModel.DataBaseInfo[] GetDataBaseInfos() {
            return base.Channel.GetDataBaseInfos();
        }
        
        public Ntreev.Crema.ServiceModel.ServiceInfo[] GetServiceInfos() {
            return base.Channel.GetServiceInfos();
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase<System.Guid> Subscribe(string userID, byte[] password, string version, string platformID, string culture) {
            return base.Channel.Subscribe(userID, password, version, platformID, culture);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Unsubscribe() {
            return base.Channel.Unsubscribe();
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase Shutdown(int milliseconds, Ntreev.Crema.ServiceModel.ShutdownType shutdownType, string message) {
            return base.Channel.Shutdown(milliseconds, shutdownType, message);
        }
        
        public Ntreev.Crema.ServiceModel.ResultBase CancelShutdown() {
            return base.Channel.CancelShutdown();
        }
    }
}
