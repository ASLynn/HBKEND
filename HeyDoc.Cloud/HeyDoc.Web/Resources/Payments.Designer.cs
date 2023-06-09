﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HeyDoc.Web.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Payments {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Payments() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("HeyDoc.Web.Resources.Payments", typeof(Payments).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To cash out, the amount must be greater than RM50..
        /// </summary>
        internal static string ErrorCashOutLimitNotReached {
            get {
                return ResourceManager.GetString("ErrorCashOutLimitNotReached", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, we do not accept debit cards at the moment. Please try again with a credit card..
        /// </summary>
        internal static string ErrorDebitCardUnsupported {
            get {
                return ResourceManager.GetString("ErrorDebitCardUnsupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unexpected error with payment gateway. Please try again later..
        /// </summary>
        internal static string ErrorGateway {
            get {
                return ResourceManager.GetString("ErrorGateway", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please add a payment method..
        /// </summary>
        internal static string ErrorNoPaymentMethod {
            get {
                return ResourceManager.GetString("ErrorNoPaymentMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Payment failed, unable to proceed. Please try again..
        /// </summary>
        internal static string ErrorPaymentFailed {
            get {
                return ResourceManager.GetString("ErrorPaymentFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid payment model..
        /// </summary>
        internal static string ErrorPaymentModelInvalid {
            get {
                return ResourceManager.GetString("ErrorPaymentModelInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot find user&apos;s payment request..
        /// </summary>
        internal static string ErrorPaymentRequestNotFound {
            get {
                return ResourceManager.GetString("ErrorPaymentRequestNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing Braintree client token..
        /// </summary>
        internal static string ErrorTokenMissing {
            get {
                return ResourceManager.GetString("ErrorTokenMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transaction not found..
        /// </summary>
        internal static string ErrorTransactionNotFound {
            get {
                return ResourceManager.GetString("ErrorTransactionNotFound", resourceCulture);
            }
        }
    }
}
