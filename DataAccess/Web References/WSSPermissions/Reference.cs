﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace DataAccess.WSSPermissions {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="PermissionsSoap", Namespace="http://schemas.microsoft.com/sharepoint/soap/directory/")]
    public partial class Permissions : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback GetPermissionCollectionOperationCompleted;
        
        private System.Threading.SendOrPostCallback AddPermissionOperationCompleted;
        
        private System.Threading.SendOrPostCallback AddPermissionCollectionOperationCompleted;
        
        private System.Threading.SendOrPostCallback UpdatePermissionOperationCompleted;
        
        private System.Threading.SendOrPostCallback RemovePermissionOperationCompleted;
        
        private System.Threading.SendOrPostCallback RemovePermissionCollectionOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Permissions() {
            this.Url = global::DataAccess.Properties.Settings.Default.DataAccess_WSSPermissions_Permissions;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event GetPermissionCollectionCompletedEventHandler GetPermissionCollectionCompleted;
        
        /// <remarks/>
        public event AddPermissionCompletedEventHandler AddPermissionCompleted;
        
        /// <remarks/>
        public event AddPermissionCollectionCompletedEventHandler AddPermissionCollectionCompleted;
        
        /// <remarks/>
        public event UpdatePermissionCompletedEventHandler UpdatePermissionCompleted;
        
        /// <remarks/>
        public event RemovePermissionCompletedEventHandler RemovePermissionCompleted;
        
        /// <remarks/>
        public event RemovePermissionCollectionCompletedEventHandler RemovePermissionCollectionCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/directory/GetPermissionCollection", RequestNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", ResponseNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode GetPermissionCollection(string objectName, string objectType) {
            object[] results = this.Invoke("GetPermissionCollection", new object[] {
                        objectName,
                        objectType});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public void GetPermissionCollectionAsync(string objectName, string objectType) {
            this.GetPermissionCollectionAsync(objectName, objectType, null);
        }
        
        /// <remarks/>
        public void GetPermissionCollectionAsync(string objectName, string objectType, object userState) {
            if ((this.GetPermissionCollectionOperationCompleted == null)) {
                this.GetPermissionCollectionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetPermissionCollectionOperationCompleted);
            }
            this.InvokeAsync("GetPermissionCollection", new object[] {
                        objectName,
                        objectType}, this.GetPermissionCollectionOperationCompleted, userState);
        }
        
        private void OnGetPermissionCollectionOperationCompleted(object arg) {
            if ((this.GetPermissionCollectionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetPermissionCollectionCompleted(this, new GetPermissionCollectionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/directory/AddPermission", RequestNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", ResponseNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void AddPermission(string objectName, string objectType, string permissionIdentifier, string permissionType, int permissionMask) {
            this.Invoke("AddPermission", new object[] {
                        objectName,
                        objectType,
                        permissionIdentifier,
                        permissionType,
                        permissionMask});
        }
        
        /// <remarks/>
        public void AddPermissionAsync(string objectName, string objectType, string permissionIdentifier, string permissionType, int permissionMask) {
            this.AddPermissionAsync(objectName, objectType, permissionIdentifier, permissionType, permissionMask, null);
        }
        
        /// <remarks/>
        public void AddPermissionAsync(string objectName, string objectType, string permissionIdentifier, string permissionType, int permissionMask, object userState) {
            if ((this.AddPermissionOperationCompleted == null)) {
                this.AddPermissionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnAddPermissionOperationCompleted);
            }
            this.InvokeAsync("AddPermission", new object[] {
                        objectName,
                        objectType,
                        permissionIdentifier,
                        permissionType,
                        permissionMask}, this.AddPermissionOperationCompleted, userState);
        }
        
        private void OnAddPermissionOperationCompleted(object arg) {
            if ((this.AddPermissionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.AddPermissionCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/directory/AddPermissionCollection", RequestNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", ResponseNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void AddPermissionCollection(string objectName, string objectType, System.Xml.XmlNode permissionsInfoXml) {
            this.Invoke("AddPermissionCollection", new object[] {
                        objectName,
                        objectType,
                        permissionsInfoXml});
        }
        
        /// <remarks/>
        public void AddPermissionCollectionAsync(string objectName, string objectType, System.Xml.XmlNode permissionsInfoXml) {
            this.AddPermissionCollectionAsync(objectName, objectType, permissionsInfoXml, null);
        }
        
        /// <remarks/>
        public void AddPermissionCollectionAsync(string objectName, string objectType, System.Xml.XmlNode permissionsInfoXml, object userState) {
            if ((this.AddPermissionCollectionOperationCompleted == null)) {
                this.AddPermissionCollectionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnAddPermissionCollectionOperationCompleted);
            }
            this.InvokeAsync("AddPermissionCollection", new object[] {
                        objectName,
                        objectType,
                        permissionsInfoXml}, this.AddPermissionCollectionOperationCompleted, userState);
        }
        
        private void OnAddPermissionCollectionOperationCompleted(object arg) {
            if ((this.AddPermissionCollectionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.AddPermissionCollectionCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/directory/UpdatePermission", RequestNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", ResponseNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void UpdatePermission(string objectName, string objectType, string permissionIdentifier, string permissionType, int permissionMask) {
            this.Invoke("UpdatePermission", new object[] {
                        objectName,
                        objectType,
                        permissionIdentifier,
                        permissionType,
                        permissionMask});
        }
        
        /// <remarks/>
        public void UpdatePermissionAsync(string objectName, string objectType, string permissionIdentifier, string permissionType, int permissionMask) {
            this.UpdatePermissionAsync(objectName, objectType, permissionIdentifier, permissionType, permissionMask, null);
        }
        
        /// <remarks/>
        public void UpdatePermissionAsync(string objectName, string objectType, string permissionIdentifier, string permissionType, int permissionMask, object userState) {
            if ((this.UpdatePermissionOperationCompleted == null)) {
                this.UpdatePermissionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdatePermissionOperationCompleted);
            }
            this.InvokeAsync("UpdatePermission", new object[] {
                        objectName,
                        objectType,
                        permissionIdentifier,
                        permissionType,
                        permissionMask}, this.UpdatePermissionOperationCompleted, userState);
        }
        
        private void OnUpdatePermissionOperationCompleted(object arg) {
            if ((this.UpdatePermissionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.UpdatePermissionCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/directory/RemovePermission", RequestNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", ResponseNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void RemovePermission(string objectName, string objectType, string permissionIdentifier, string permissionType) {
            this.Invoke("RemovePermission", new object[] {
                        objectName,
                        objectType,
                        permissionIdentifier,
                        permissionType});
        }
        
        /// <remarks/>
        public void RemovePermissionAsync(string objectName, string objectType, string permissionIdentifier, string permissionType) {
            this.RemovePermissionAsync(objectName, objectType, permissionIdentifier, permissionType, null);
        }
        
        /// <remarks/>
        public void RemovePermissionAsync(string objectName, string objectType, string permissionIdentifier, string permissionType, object userState) {
            if ((this.RemovePermissionOperationCompleted == null)) {
                this.RemovePermissionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRemovePermissionOperationCompleted);
            }
            this.InvokeAsync("RemovePermission", new object[] {
                        objectName,
                        objectType,
                        permissionIdentifier,
                        permissionType}, this.RemovePermissionOperationCompleted, userState);
        }
        
        private void OnRemovePermissionOperationCompleted(object arg) {
            if ((this.RemovePermissionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RemovePermissionCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/sharepoint/soap/directory/RemovePermissionCollection" +
            "", RequestNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", ResponseNamespace="http://schemas.microsoft.com/sharepoint/soap/directory/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void RemovePermissionCollection(string objectName, string objectType, System.Xml.XmlNode memberIdsXml) {
            this.Invoke("RemovePermissionCollection", new object[] {
                        objectName,
                        objectType,
                        memberIdsXml});
        }
        
        /// <remarks/>
        public void RemovePermissionCollectionAsync(string objectName, string objectType, System.Xml.XmlNode memberIdsXml) {
            this.RemovePermissionCollectionAsync(objectName, objectType, memberIdsXml, null);
        }
        
        /// <remarks/>
        public void RemovePermissionCollectionAsync(string objectName, string objectType, System.Xml.XmlNode memberIdsXml, object userState) {
            if ((this.RemovePermissionCollectionOperationCompleted == null)) {
                this.RemovePermissionCollectionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRemovePermissionCollectionOperationCompleted);
            }
            this.InvokeAsync("RemovePermissionCollection", new object[] {
                        objectName,
                        objectType,
                        memberIdsXml}, this.RemovePermissionCollectionOperationCompleted, userState);
        }
        
        private void OnRemovePermissionCollectionOperationCompleted(object arg) {
            if ((this.RemovePermissionCollectionCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RemovePermissionCollectionCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void GetPermissionCollectionCompletedEventHandler(object sender, GetPermissionCollectionCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetPermissionCollectionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetPermissionCollectionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public System.Xml.XmlNode Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((System.Xml.XmlNode)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void AddPermissionCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void AddPermissionCollectionCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void UpdatePermissionCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void RemovePermissionCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2558.0")]
    public delegate void RemovePermissionCollectionCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
}

#pragma warning restore 1591