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
    public class LeakCheckResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LeakCheckResource() {
        }
        
        /// <summary>
        ///   Retourne l'instance ResourceManager mise en cache utilisée par cette classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Apps.Devlosys.Resources.I18N.LeakCheckResource", typeof(LeakCheckResource).Assembly);
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
        ///   Recherche une chaîne localisée semblable à This part already passed in FCT station..
        /// </summary>
        public static string AlreadyInFCTMessage {
            get {
                return ResourceManager.GetString("AlreadyInFCTMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Check the stability of leaks.
        /// </summary>
        public static string HeaderText {
            get {
                return ResourceManager.GetString("HeaderText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Warning! this part is under the leak stablilisation time !.
        /// </summary>
        public static string LeakPartMessage {
            get {
                return ResourceManager.GetString("LeakPartMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Try to scan the room to check the stability of the leak.
        /// </summary>
        public static string MessageText {
            get {
                return ResourceManager.GetString("MessageText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Scan the serial number.
        /// </summary>
        public static string SnrScanText {
            get {
                return ResourceManager.GetString("SnrScanText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Serial number.
        /// </summary>
        public static string SnrText {
            get {
                return ResourceManager.GetString("SnrText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Time spent per hours.
        /// </summary>
        public static string TimeSpentPerHoursText {
            get {
                return ResourceManager.GetString("TimeSpentPerHoursText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Time spent per minutes.
        /// </summary>
        public static string TimeSpentPerMinutesText {
            get {
                return ResourceManager.GetString("TimeSpentPerMinutesText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Try to ckeck the pcb booking in manuelle loading station..
        /// </summary>
        public static string TryManuelleLoadingMessage {
            get {
                return ResourceManager.GetString("TryManuelleLoadingMessage", resourceCulture);
            }
        }
    }
}