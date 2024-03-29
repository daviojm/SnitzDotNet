﻿$(document).ready(function () {
    //alert("doc ready");
    $(".minibbcode").each(function () {
        $(this).html(parseBBCode(parseEmoticon($(this).text(), pagetheme)));
    });
    jQuery("abbr.timeago").timeago();
});
function bindForums() {

    $(".bbcode").each(function () {
        $(this).html(parseBBCode(parseEmoticon($(this).text(), pagetheme)));
    });
    jQuery("abbr.timeago").timeago();
};

$.fn.serializeNoViewState = function () {
    return this.find("input,select,hidden,textarea")
        .not("[type=hidden][name^=__]")
        .serialize();
};

function UpdateRoleList(ddlid, hdnid, remove) {
    var rolelist = $get(hdnid).value;
    var newrole = $("#" + ddlid + " option:selected").text();
    alert(rolelist);
    alert(newrole);

    var tbl = $('#roletbl');
    if (remove) {
        $("#roletbl td:contains('" + newrole.toLowerCase() + "')").parent().remove();
        var regx = new RegExp("\\b" + newrole + "(,|$)", "igm");
        rolelist = rolelist.replace(regx, "");
    } else {
        var regx2 = new RegExp("\\b" + newrole + "(,|$)", "igm");

        if (regx2.test(rolelist)) {
            //do nothing 
            alert("Role already added");
        } else {
            rolelist = rolelist + ',' + newrole;
            if (tbl.html() == null) { // no table so create one
                $('<table id="roletbl"><tr><td>' + newrole.toLowerCase() + '</td></tr></table>').appendTo($('#rolelist'));
            } else {
                var rowCount = $('#roletbl tr').length;
                if (rowCount >= 1) {
                    $('#roletbl tr:last').before('<tr><td>' + newrole.toLowerCase() + '</td></tr>');
                } else {
                    $('#roletbl').append('<tr><td>' + newrole.toLowerCase() + '</td></tr>');
                }
            }
        }

    }
    $get(hdnid).value = rolelist;
}
function UpdateModerator(ddlid, hdnid, remove) {
    var modlist = $get(hdnid).value;
    var newmodid = $("#" + ddlid + " option:selected").val();
    var newmod = $("#" + ddlid + " option:selected").text();


    if (remove) {
        $("#roletbl td:contains('" + newmod + "')").parent().remove();
        var regx = new RegExp("\\b" + newmodid + "(,|$)", "igm");
        modlist = modlist.replace(regx, "");
    } else {
        var regx2 = new RegExp("\\b" + newmodid + "(,|$)", "igm");

        if (regx2.test(modlist)) {
            //already in list so do nothing
            alert("Moderator already in list");
        } else {
            var rowCount = $('#roletbl tr').length;
            modlist = modlist + ',' + newmodid;
            var tbl = $('#modtbl');
            if (tbl.html() == null) { // no table so create one
                $('<table id="modtbl"><tr><td>' + newmod + '</td></tr></table>').appendTo($('#modlist'));
            } else {
                if (rowCount >= 1) {
                    $('#modtbl tr:last').after('<tr><td>' + newmod + '</td></tr>');
                } else {
                    $('#modtbl').append('<tr><td>' + newmod + '</td></tr>');
                }
            }
        }
    }
    $get(hdnid).value = modlist;
}
function SaveForum() {
    window.PageMethods.SaveForum($("form").serializeNoViewState());
    var millisecondsToWait = 500;
    setTimeout(function () {
        mainScreen.CancelModal();
        location.reload();
    }, millisecondsToWait);

}
function SaveCategory() {
    window.PageMethods.SaveCategory($("form").serializeNoViewState());
    var millisecondsToWait = 500;
    setTimeout(function () {
        mainScreen.CancelModal();
        location.reload();
    }, millisecondsToWait);
}

function pageLoad() {
    var allBehaviors = window.Sys.Application.getComponents();
    for (var loopIndex = 0; loopIndex < allBehaviors.length; loopIndex++) {
        var currentBehavior = allBehaviors[loopIndex];
        if (currentBehavior.get_name() == "CollapsiblePanelBehavior") {
            allcpe.push(currentBehavior);
        }

    }

    if (getCookie()) {
        expandedIndex = getCookie().split(',');;
        for (var cpeIndex = 0; cpeIndex < expandedIndex.length; cpeIndex++) {
            var expandedcpe = expandedIndex[cpeIndex];
            $find(expandedcpe).set_Collapsed(false);
        }

    } else {
        expandAll();
    }
}
function pageUnload() {
    expandedIndex = null;
    expandedIndex = [];
    for (var cpeIndex = 0; cpeIndex < allcpe.length; cpeIndex++) {
        var currentcpe = allcpe[cpeIndex];
        if (!currentcpe.get_Collapsed()) {
            //save the expanded cpe's index
            expandedIndex.push(currentcpe.get_id());
        }
    }
    setCookie(expandedIndex);
}

function setCookie(cookieValue) {
    var sVar = "cookiename";
    var theCookie = sVar + '=' + cookieValue + '; expires=Fri, 1 Jul 2019 11:11:11 UTC;' + 'path=/';
    document.cookie = theCookie;
}
function getCookie() {
    var sVar = "cookiename";
    var cookies = document.cookie.split('; ');
    for (var i = 1; i <= cookies.length; i++) {
        if (cookies[i - 1].split('=')[0] == sVar) {
            return cookies[i - 1].split('=')[1];
        }
    }
    return "";
}
function expandAll() {
    for (var cpeIndex = 0; cpeIndex < allcpe.length; cpeIndex++) {
        var currentcpe = allcpe[cpeIndex];
        currentcpe.expandPanel(true);
    }

}
//Sender: Reference to the CollapsiblePanelExtender Client Behavior  
//eventArgs: Empty EvenArgs  
function onExpand(sender, eventArgs) {
    //Use sender (instance of CollapsiblePanerExtender client Behavior)  
    //to get ExpandControlID.  
    var expander = $get(sender.get_ExpandControlID());

    //Using RegEx to replace pnlCustomer with hdnCustId.  
    //hdnCustId is a hidden field located within pnlCustomer.  
    //pnlCustomer is a Panel, and Panels are not Naming Container.  
    //So hdnCustId will have the same ID as pnlCustomer but with   
    //'hdnCustId' at the end insted of pnlCustomer.  
    var custId = $get(sender.get_ExpandControlID().replace(/Cat_HeaderPanel/g, 'hdnCatId')).value;
    //Issue AJAX call to PageMethod, and send sender   
    //object as userContext Parameter.  
    PageMethods.GetForums(custId, OnSucceeded, OnFailed, sender);

}
// Callback function invoked on successful   
// completion of the page method.  
function OnSucceeded(result, userContext, methodName) {
    //$get('progress').style.visibility = 'hidden';
    //userContext is sent while issue AJAX call  
    //it is an instance of CollapsiblePanelExtender client Behavior.  
    //Used to get the collapsible element and sent its innerHTML   
    //to the returned result.             
    userContext.get_element().innerHTML = result;
    bindForums();
}
// Callback function invoked on failure   
// of the page method.  
function OnFailed(error, userContext, methodName) {
    //$get('progress').style.visibility = 'hidden';
    alert(error.get_message());
}