﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SaveAndPlay.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Login {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Login() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SaveAndPlay.Resources.Login", typeof(Login).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please press the sign in button to access your Dropbox account.
        /// </summary>
        public static string DropboxLoginLabel {
            get {
                return ResourceManager.GetString("DropboxLoginLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sign in failed, check your login and password and retry.
        /// </summary>
        public static string LoginFailedLabel {
            get {
                return ResourceManager.GetString("LoginFailedLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Login.
        /// </summary>
        public static string LoginLabel {
            get {
                return ResourceManager.GetString("LoginLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password.
        /// </summary>
        public static string PasswordLabel {
            get {
                return ResourceManager.GetString("PasswordLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your login information has been successfully reset.
        /// </summary>
        public static string ResetCompleted {
            get {
                return ResourceManager.GetString("ResetCompleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Save login and password?.
        /// </summary>
        public static string SaveLogin {
            get {
                return ResourceManager.GetString("SaveLogin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signing in....
        /// </summary>
        public static string SigninginLabel {
            get {
                return ResourceManager.GetString("SigninginLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please press the sign in button to access your SkyDrive account.
        /// </summary>
        public static string SkydriveLoginLabel {
            get {
                return ResourceManager.GetString("SkydriveLoginLabel", resourceCulture);
            }
        }
    }
}