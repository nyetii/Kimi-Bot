﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kimi.Resources {
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
    internal class Commands_pt_BR {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Commands_pt_BR() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Kimi.Resources.Commands.pt-BR", typeof(Commands_pt_BR).Assembly);
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
        ///   Looks up a localized string similar to Lista o top 10 membros por pontuação.
        /// </summary>
        internal static string ranking_top_description {
            get {
                return ResourceManager.GetString("ranking.top.description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A resposta somente será exibida para você.
        /// </summary>
        internal static string ranking_top_ephemeral_description {
            get {
                return ResourceManager.GetString("ranking.top.ephemeral.description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Seleciona a página - ex. página 3 para a 21ª à 30ª posição.
        /// </summary>
        internal static string ranking_top_page_description {
            get {
                return ResourceManager.GetString("ranking.top.page.description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to pagina.
        /// </summary>
        internal static string ranking_top_page_name {
            get {
                return ResourceManager.GetString("ranking.top.page.name", resourceCulture);
            }
        }
    }
}
