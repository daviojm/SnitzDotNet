﻿/*
####################################################################################################################
##
## SnitzUI.UserControls.Popups - Confirmation.ascx
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

using System;
using SnitzCommon;

namespace SnitzUI.UserControls.Popups
{
    public partial class Confirmation : TemplateUserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Data != null)
            {
                var action = (string) Data;
                Literal1.Text = action;
            }
        }
    }
}