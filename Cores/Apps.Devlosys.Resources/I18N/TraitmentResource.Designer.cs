﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Apps.Devlosys.Resources.I18N {
    using System;
    
    
    /// <summary>
    ///   Une classe de ressource fortement typée destinée, entre autres, à la consultation des chaînes localisées.
    /// </summary>
    // Cette classe a été générée automatiquement par la classe StronglyTypedResourceBuilder
    // à l'aide d'un outil, tel que ResGen ou Visual Studio.
    // Pour ajouter ou supprimer un membre, modifiez votre fichier .ResX, puis réexécutez ResGen
    // avec l'option /str ou régénérez votre projet VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class TraitmentResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TraitmentResource() {
        }
        
        /// <summary>
        ///   Retourne l'instance ResourceManager mise en cache utilisée par cette classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Apps.Devlosys.Resources.I18N.TraitmentResource", typeof(TraitmentResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Remplace la propriété CurrentUICulture du thread actuel pour toutes
        ///   les recherches de ressources à l'aide de cette classe de ressource fortement typée.
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
        ///   Recherche une chaîne localisée semblable à Product blocked by ITAC!
        ///The pcb has a scrap or fail state..
        /// </summary>
        public static string BlockedProductITACMessage {
            get {
                return ResourceManager.GetString("BlockedProductITACMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Product blocked!
        ///This product : {0} is already declared OR not exists on ITAC.
        /// </summary>
        public static string BlockedProductMessage {
            get {
                return ResourceManager.GetString("BlockedProductMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Bin data.
        /// </summary>
        public static string CheckBinText {
            get {
                return ResourceManager.GetString("CheckBinText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Booking.
        /// </summary>
        public static string CheckBookingText {
            get {
                return ResourceManager.GetString("CheckBookingText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Booking / Labling.
        /// </summary>
        public static string CheckBothText {
            get {
                return ResourceManager.GetString("CheckBothText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Labling.
        /// </summary>
        public static string CheckLablingText {
            get {
                return ResourceManager.GetString("CheckLablingText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à MES / SAP.
        /// </summary>
        public static string CheckMesText {
            get {
                return ResourceManager.GetString("CheckMesText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à MES is not active.
        /// </summary>
        public static string MESDisableMessage {
            get {
                return ResourceManager.GetString("MESDisableMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à No bin reference found !
        ///Try to insert data from bin management model.
        /// </summary>
        public static string NoBinMFoundMessage {
            get {
                return ResourceManager.GetString("NoBinMFoundMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à This serial number has a scrap or fail state in previous step , try to call the quality to unblock the situation.
        /// </summary>
        public static string ScrapPcbDescription {
            get {
                return ResourceManager.GetString("ScrapPcbDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à The pcb state not good , try to check it on iTAC application.
        /// </summary>
        public static string ScrapPcbMessage {
            get {
                return ResourceManager.GetString("ScrapPcbMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Scrap pcb captured.
        /// </summary>
        public static string ScrapPcbTitle {
            get {
                return ResourceManager.GetString("ScrapPcbTitle", resourceCulture);
            }
        }
    }
}
