/*
####################################################################################################################
##
## ModConfig - newmod.config
##   
## Author:		Huw Reddick
## Copyright:	Huw Reddick
## based on code from Snitz Forums 2000 (c) Huw Reddick, Michael Anderson, Pierre Gorissen and Richard Kinser
## Created:		29/07/2013
## 
## The use and distribution terms for this software are covered by the 
## Eclipse License 1.0 (http://opensource.org/licenses/eclipse-1.0)
## which can be found in the file Eclipse.txt at the root of this distribution.
## By using this software in any fashion, you are agreeing to be bound by 
## the terms of this license.
##
## You must not remove this notice, or any other, from this software.  
##
#################################################################################################################### 
*/

using System.Configuration;
using SnitzConfig;


namespace ModConfig
{
    /// <summary>
    /// Summary description for newmod
    /// </summary>
    public class SampleModConfig : SnitzMod
    {
        public string Name
        {
            get { return "SampleModConfig"; }
        }

        public string Description
        {
            get { return "SampleModConfig"; }
        }

        public static string SampleModSomeValue
        {
            get
            {
                return ConfigurationManager.AppSettings["boolShowFileUpload"];
            }
            set
            {
                Config.UpdateConfig("boolShowFileUpload", value);
            }
        }


    } 
}
