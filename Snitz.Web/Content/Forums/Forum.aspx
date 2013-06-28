﻿<%-- 
##############################################################################################################
## Snitz Forums .net
##############################################################################################################
## Copyright (C) 2012 Huw Reddick
## All rights reserved.
## based on code from Snitz Forums 2000 (c) Huw Reddick, Michael Anderson, Pierre Gorissen and Richard Kinser
## http://forum.snitz.com
##############################################################################################################
--%>
<%@ Page Title="" Language="C#" MasterPageFile="~/MasterTemplates/MainMaster.Master" AutoEventWireup="true" Culture="auto"
    UICulture="auto"
    CodeBehind="Forum.aspx.cs" Inherits="SnitzUI.ForumPage" ValidateRequest="false" %>

<%@ MasterType TypeName="BaseMasterPage" %>
<%@ Import Namespace="SnitzCommon" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxtoolkit" %>

<%@ Register Src="~/UserControls/ForumLogin.ascx" TagName="ForumLogin" TagPrefix="uc2" %>
<%@ Register Src="~/UserControls/GridPager.ascx" TagName="GridPager" TagPrefix="uc4" %>
<%@ Register Src="~/UserControls/PagePostButtons.ascx" TagName="PostButtons" TagPrefix="uc5" %>
<%@ Reference Control="~/UserControls/SideColumn.ascx" %>

<asp:Content runat="server" ID="metatag" ContentPlaceHolderID="CPMeta">
    <asp:Literal ID="metadescription" runat="server"></asp:Literal>
</asp:Content>
<asp:Content runat="server" ID="head" ContentPlaceHolderID="CPHead">

    <script src="/scripts/bbcode.js" type="text/javascript"></script>
    <script src="/scripts/smilies.js" type="text/javascript"></script>
    <script type="text/javascript">
        //Parse the bbcode
        $(document).ready(function () {
            $(".bbcode").each(function () {
                var test = $(this).text();
                $(this).html(parseBBCode(parseEmoticon($(this).text())));
            });
        });
        // This section Keeps the memory for the collapsible panels in the menu pane
        var objExtenderMain;

        // this will run automatically when the page has finished loading
        function pageLoad(sender, args) {

            //Main
            objExtenderMain = $find("stickyHide");
            if (objExtenderMain) {
                objExtenderMain.add_expandComplete(getMainNavigationState);
                objExtenderMain.add_collapseComplete(getMainNavigationState);
            }
        }


        // This is for the constantly displayed collapsible panel
        function getMainNavigationState() {
            if (objExtenderMain.get_Collapsed()) {
                //Collapsed
                $get("<%= stickystate.ClientID %>").value = "1";

        }
        else {
            //Expanded
            $get("<%= stickystate.ClientID %>").value = "0";

        }
    }
    function fnClickUpdate(sender, e) {
        __doPostBack(sender, e);
    }
    function HideLoginPopup(sender) {
        //        var modal = $find('mpLogin');
        //        modal.hide();
        location.replace('/default.aspx');
    }
    function ApprovePost(topicid, replyid) {
        PageMethods.Approval(topicid, replyid);
        __doPostBack("", "");
    }
    function OnHold(topicid, replyid) {
        PageMethods.PutOnHold(topicid, replyid);
        __doPostBack("", "");
    }
    </script>
    <style type="text/css">
        .AspNet-Login-UserPanel {
            display: none;
        }

    </style>
</asp:Content>

<asp:Content runat="server" ID="Cph1" ContentPlaceHolderID="CPH1">
    <uc2:ForumLogin ID="fLogin" runat="server"></uc2:ForumLogin>
</asp:Content>
<asp:Content runat="server" ID="Cphr" ContentPlaceHolderID="CPHR">
    <asp:DropDownList CssClass="ddActiveRefresh" runat="server" ID="ddlShowTopicDays"
        AutoPostBack="True" OnSelectedIndexChanged="NumberOfDaysSelectedIndexChanged"
        EnableViewState="False" Style="float: right;">
        <asp:ListItem Value="" meta:resourcekey="ddlShowTopicsSelect">[Select Topics to display]</asp:ListItem>
        <asp:ListItem Value="-1" meta:resourcekey="ddlShowTopicsItem1">Show all topics</asp:ListItem>
        <asp:ListItem Value="0" meta:resourcekey="ddlShowTopicsItem2">Show all open topics</asp:ListItem>
        <asp:ListItem Value="1" meta:resourcekey="ddlShowTopicsItem3">Show topics from last day</asp:ListItem>
        <asp:ListItem Value="2" meta:resourcekey="ddlShowTopicsItem4">Show topics from last 2 days</asp:ListItem>
        <asp:ListItem Value="5" meta:resourcekey="ddlShowTopicsItem5">Show topics from last 5 days</asp:ListItem>
        <asp:ListItem Value="7" meta:resourcekey="ddlShowTopicsItem6">Show topics from last 7 days</asp:ListItem>
        <asp:ListItem Value="14" meta:resourcekey="ddlShowTopicsItem7">Show topics from last 14 days</asp:ListItem>
        <asp:ListItem Value="30" meta:resourcekey="ddlShowTopicsItem8">Show topics from last month</asp:ListItem>
        <asp:ListItem Value="60" meta:resourcekey="ddlShowTopicsItem9">Show topics from last 2 months</asp:ListItem>
        <asp:ListItem Value="183" meta:resourcekey="ddlShowTopicsItem10">Show topics from last 6 months</asp:ListItem>
        <asp:ListItem Value="365" meta:resourcekey="ddlShowTopicsItem11">Show topics from the last year</asp:ListItem>
    </asp:DropDownList><br />
</asp:Content>
<asp:Content runat="server" ID="Cphl" ContentPlaceHolderID="CPHL">
    <uc5:PostButtons ID="pb1" runat="server"></uc5:PostButtons>
</asp:Content>
<asp:Content runat="server" ID="Cpm" ContentPlaceHolderID="CPM">
    <asp:UpdatePanel ID="stickyUPD" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="Sticky_HeaderPanel" runat="server" CssClass="statsPanelHeader" Style="cursor: pointer;">
                &nbsp;<asp:Image ID="stickyExpand" runat="server" ImageAlign="Middle" />
                <asp:Label runat="server" Text='<%$ Resources:webResources, lblStickyTopic %>' ID="lblSicky"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="StickyPanel" runat="server">
                <asp:GridView ID="StickyGrid" runat="server" AutoGenerateColumns="False" Width="100%"
                    DataKeyNames="Id" CellPadding="3" CssClass="stickytable" GridLines="None" EnableViewState="False"
                    OnRowDataBound="ForumTableRowDataBound">
                    <PagerSettings Visible="False" />
                    <HeaderStyle CssClass="category cattitle" />
                    <RowStyle CssClass="rowsticky" />
                    <AlternatingRowStyle CssClass="rowsticky" />
                    <EmptyDataRowStyle CssClass="NoBorder" HorizontalAlign="Center" />
                    <Columns>
                        <asp:TemplateField HeaderText=" ">
                            <ItemTemplate>
                            </ItemTemplate>
                            <ItemStyle CssClass="iconCol" />
                            <HeaderStyle Width="20px"></HeaderStyle>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources:webResources, lblTopic %>" SortExpression="Subject">
                            <ItemTemplate>
                                    <asp:Image ID="imgPosticonSmall" SkinID="PosticonSmall" runat="server" Visible="False"
                                        GenerateEmptyAlternateText="true" />
                                    <a class="bbcode" href="/Content/Forums/topic.aspx?TOPIC=<%# Eval("Id") %>" title="<%# HttpUtility.HtmlDecode(Eval("Subject").ToString()) %>">
                                    <%# HttpUtility.HtmlDecode(Eval("Subject").ToString()) %></a>
                            </ItemTemplate>
                            <ItemStyle VerticalAlign="Top" Width="50%" />
                            <HeaderStyle Width="50%" HorizontalAlign="Left"></HeaderStyle>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources:webResources, lblPostAuthor %>" SortExpression="Author.Name">
                            <ItemTemplate>
                                <a href='<%# Eval("Author.ProfileLink") %>' title='<%# Eval("Author.Name") %>'><%# Eval("Author.Name") %></a>
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Left" Width="80px" VerticalAlign="Top" />
                            <HeaderStyle Width="80px" HorizontalAlign="Left"></HeaderStyle>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources:webResources, lblReplies %>" SortExpression="ReplyCount" Visible="false">
                            <ItemTemplate>
                                <%# Common.TranslateNumerals(Eval("ReplyCount"))%>
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" Width="60px" VerticalAlign="Top" />
                            <HeaderStyle Width="60px"></HeaderStyle>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources:webResources, lblViewCount %>" SortExpression="ViewCount" Visible="true">
                            <ItemTemplate>
                                <%# Common.TranslateNumerals(Eval("ViewCount"))%>
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" Width="60px" VerticalAlign="Top" />
                            <HeaderStyle Width="60px"></HeaderStyle>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources:webResources, lblLastPost %>" SortExpression="LastPostDate">
                            <ItemTemplate>
                                <span class="smallText">by:&nbsp;<asp:Literal ID="popuplink" runat="server" Text='<%# Eval("LastPostAuthor.ProfilePopup") %>'></asp:Literal>&nbsp;<asp:HyperLink ID="lpLnk" runat="server"
                                    CssClass="profilelnk" SkinID="JumpTo" NavigateUrl='<%# String.Format("/Content/Forums/topic.aspx?TOPIC={0}&whichpage=-1#{1}", Eval("Id"),Eval("LastReplyId")) %>'
                                    ToolTip="<%$ Resources:webResources, lblLastPostJump %>" Text="<%$ Resources:webResources, lblLastPostJump %>"></asp:HyperLink></span>
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Left" Width="122px" VerticalAlign="Top" CssClass="nowrap" />
                            <HeaderStyle Width="120px" HorizontalAlign="Left"></HeaderStyle>
                        </asp:TemplateField>
                        <asp:TemplateField InsertVisible="False">
                            <ItemTemplate>
                                <asp:ImageButton ID="Stick" SkinID="StickyTopic" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                    runat="server" ToolTip="<%$ Resources:webResources, lblStick %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Sticky', 'Do you want to make topic sticky?');
mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Make topic sticky ?'},'confirmHandlers.BeginRecieve');
return false;"
                                    CausesValidation="False" EnableViewState="False" OnClick="StickTopic" />
                                <asp:ImageButton ID="UnStick" SkinID="UnStickyTopic" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                    runat="server" ToolTip="<%$ Resources:webResources, lblUnStick %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Sticky', 'Do you want to unstick the topic?');
mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'UnStick topic ?'},'confirmHandlers.BeginRecieve');
return false;"
                                    CausesValidation="False" EnableViewState="False" OnClick="UnStickTopic" />
                                <asp:ImageButton ID="TopicLock" SkinID="LockTopic" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                    runat="server" ToolTip="<%$ Resources:webResources, lbllock %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Lock', 'Do you want to lock the topic?');
mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Lock Topic ?'},'confirmHandlers.BeginRecieve');
return false;"
                                    CausesValidation="False" EnableViewState="False" OnClick="LockTopic" />
                                <asp:ImageButton ID="TopicUnLock" SkinID="UnLockTopic" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                    runat="server" ToolTip="<%$ Resources:webResources, lblUnlock %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm UnLock', 'Do you want to unlock the topic?');
mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'UnLock Topic ?'},'confirmHandlers.BeginRecieve');
return false;"
                                    CausesValidation="False" EnableViewState="False" OnClick="UnLockTopic" />
                                <asp:ImageButton ID="TopicDelete" SkinID="DeleteMessage" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                    runat="server" ToolTip="<%$ Resources:webResources, lblDelPost %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Delete', 'Do you want to delete the topic?');
mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Delete Topic ?'},'confirmHandlers.BeginRecieve');
return false;"
                                    CausesValidation="False" EnableViewState="False" OnClick="DeleteTopic" />
                                <asp:HyperLink ID="hypEditTopic" SkinID="EditTopic" runat="server" Visible="False"
                                    Text="<%$ Resources:webResources, lblEditPost %>" ToolTip="<%$ Resources:webResources, lblEditPost %>"></asp:HyperLink>
                                <asp:HyperLink ID="hypReplyTopic" SkinID="ReplyTopic" runat="server" Text="<%$ Resources:webResources, lblReply %>"
                                    ToolTip="<%$ Resources:webResources, lblReply %>"></asp:HyperLink>
                                <asp:ImageButton ID="TopicSub" SkinID="Subscribe" CommandArgument='<%# Eval("Id")%>' CommandName="sub"
                                    runat="server" ToolTip="<%$ Resources:webResources, lblSubscribeTopic %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Subscribe', 'Do you want to be notified when someone posts a reply?');
mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Subscribe to Topic ?'},'confirmHandlers.BeginRecieve');
return false;"
                                    CausesValidation="False" EnableViewState="False" OnClick="TopicSubscribe" />
                                <asp:ImageButton ID="TopicUnSub" SkinID="UnSubscribe" CommandArgument='<%# Eval("Id")%>' CommandName="unsub"
                                    runat="server" ToolTip="<%$ Resources:webResources, lblUnSubscribeTopic %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Remove', 'Do you want to remove notifications from topic?');
mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Do you want to remove notifications from topic?'},'confirmHandlers.BeginRecieve');
return false;"
                                    CausesValidation="False" EnableViewState="False" OnClick="TopicSubscribe" />

                                <asp:HyperLink ID="hypNoArchiveTopic" SkinID="NoArchiveTopic" runat="server" Text="<%$ Resources:webResources, lblNoArchive %>"
                                    ToolTip="<%$ Resources:webResources, lblNoArchive %>" Visible="false"></asp:HyperLink>
                                <asp:HyperLink ID="hypArchiveTopic" SkinID="ArchiveTopic" runat="server" Text="<%$ Resources:webResources, lblArchive %>"
                                    ToolTip="<%$ Resources:webResources, lblArchive %>" Visible="false"></asp:HyperLink>
                                <asp:ImageButton ID="TopicApprove" SkinID="approve" runat="server" />
                                <%--<asp:ImageButton ID="TopicHold" SkinID="OnHold" runat="server" />--%>
                            </ItemTemplate>
                            <ItemStyle CssClass="buttonCol" Width="40px" HorizontalAlign="Right" />
                            <HeaderStyle Width="40px"></HeaderStyle>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

            </asp:Panel>
            <asp:HiddenField ID="stickystate" runat="server" />
            <ajaxtoolkit:CollapsiblePanelExtender ID="Sticky_Panel_CollapsiblePanelExtender"
                runat="server" Enabled="True" TargetControlID="StickyPanel"
                CollapseControlID="Sticky_HeaderPanel" ExpandControlID="Sticky_HeaderPanel"
                ImageControlID="stickyExpand" SuppressPostBack="True" BehaviorID="stickyHide">
            </ajaxtoolkit:CollapsiblePanelExtender>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:ObjectDataSource ID="TopicODS" runat="server" OldValuesParameterFormatString="original_{0}"
        SelectMethod="GetForumTopicsPaged" SelectCountMethod="GetForumTopicCount" TypeName="SnitzData.PagedObjects"
        EnablePaging="True" OnSelected="TopicOdsSelected"
        OnSelecting="TopicOdsSelecting">
        <SelectParameters>
            <asp:SessionParameter Name="forumid" DefaultValue="-1" SessionField="ForumId" Type="Int32" />
            <asp:SessionParameter ConvertEmptyStringToNull="true" Name="topicstatus" DefaultValue="" SessionField="TopicStatus" Type="Int32" />
            <asp:SessionParameter ConvertEmptyStringToNull="true" Name="fromdate" DefaultValue="" SessionField="LastPostDate" Type="String" />
            <asp:Parameter Name="startRowIndex" Type="Int32" />
            <asp:Parameter Name="maximumRows" Type="Int32" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="topicUPD">
        <ProgressTemplate>
            <asp:Label ID="lblProgress" runat="server" Text="Loading...." Width="200px" BackColor="#FFFF80"
                ForeColor="Maroon" Font-Bold="True" Style="padding: 5px"></asp:Label>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <br style="clear: both;" class="seperator" />
    <asp:UpdatePanel ID="topicUPD" runat="server" ChildrenAsTriggers="true" UpdateMode="Always">
        <ContentTemplate>
            <script type="text/javascript">

                Sys.Application.add_load(BindEvents);

            </script>
            <asp:GridView ID="ForumTable" runat="server" AllowPaging="true"
                AutoGenerateColumns="False" Width="100%" DataKeyNames="Id" CellPadding="3" CssClass="forumtable"
                GridLines="None" EmptyDataText="<%$ Resources:webResources, ErrNoTopics %>" EnableViewState="False"
                OnRowDataBound="ForumTableRowDataBound" DataSourceID="TopicODS" EnableModelValidation="True">
                <Columns>
                    <asp:TemplateField HeaderText=" ">
                        <HeaderTemplate>
                            <asp:HyperLink ID="HyperLink1" NavigateUrl='<%# String.Format("~/Handlers/rss.ashx?id={0}", ForumId ) %>' runat="server" ImageUrl="/images/rss.png" ToolTip="rss feed">RSS</asp:HyperLink>
                        </HeaderTemplate>
                        <HeaderStyle Width="20px" />
                        <ItemStyle HorizontalAlign="Center"></ItemStyle>
                        <ItemTemplate>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField SortExpression="Subject">
                        <ItemStyle Width="50%" CssClass="postCol"></ItemStyle>
                        <ItemTemplate>
                            <span style="width: 100%; overflow: hidden;">
                                <asp:Image ID="imgPosticonSmall" SkinID="PosticonSmall" runat="server" Visible="true"
                                    GenerateEmptyAlternateText="true" />
                                &nbsp;<asp:HyperLink ID="tLink" runat="server" NavigateUrl='<%# String.Format("/Content/Forums/topic.aspx?TOPIC={0}", Eval("Id")) %>'
                                    ToolTip='<%# Eval("Subject")%>' Text='<%# HttpUtility.HtmlDecode(Eval("Subject").ToString()) %>' /></span>
                            <br />

                            <span class="smallText">
                                <%# Eval("PageCount")%>
                                page(s)</span>
                        </ItemTemplate>
                        <HeaderTemplate>
                            <asp:Label ID="Label1" runat="server" Text="<%$ Resources:webResources, lblTopic %>"></asp:Label>
                            <div style="float:right;"><asp:TextBox ID="searchFor" runat="server" Width="175px"></asp:TextBox>
                            <asp:ImageButton ID="open_btn2" ToolTip="Search in Forum" AlternateText="Search in Forum" runat="server" SkinID="Search" OnClick="FilterTopics" />
                            </div>

                        </HeaderTemplate>
                        <HeaderStyle Width="50%" HorizontalAlign="Left"></HeaderStyle>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:webResources, lblPostAuthor %>" SortExpression="Author.Name">
                        <HeaderStyle Width="80px" />
                        <ItemStyle HorizontalAlign="Center"></ItemStyle>
                        <ItemTemplate>
                            <a href='<%# Eval("Author.ProfileLink") %>' title='<%# Eval("Author.Name") %>'><%# Eval("Author.Name") %></a>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:webResources, lblReplies %>" SortExpression="ReplyCount">
                        <HeaderStyle Width="60px" />
                        <ItemStyle HorizontalAlign="Center"></ItemStyle>
                        <ItemTemplate>
                            <%# Eval("ReplyCount") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:webResources, lblViewCount %>" SortExpression="ViewCount">
                        <HeaderStyle Width="60px" />
                        <ItemStyle HorizontalAlign="Center"></ItemStyle>
                        <ItemTemplate>
                            <%# Eval("ViewCount") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="<%$ Resources:webResources, lblLastPost %>" SortExpression="LastPostDate">
                        <HeaderStyle Width="90px" />
                        <ItemStyle HorizontalAlign="Center" CssClass="nowrap"></ItemStyle>
                        <ItemTemplate>
                            <span class="smallText">by:<asp:Literal ID="popuplink" runat="server" Text='<%# Eval("LastPostAuthor.ProfilePopup") %>'></asp:Literal>
                                &nbsp;<asp:HyperLink ID="lpLnk" runat="server"
                                    CssClass="profilelnk" SkinID="JumpTo" NavigateUrl='<%# String.Format("/Content/Forums/topic.aspx?TOPIC={0}&whichpage=-1#{1}", Eval("Id"),Eval("LastReplyId")) %>'
                                    ToolTip="<%$ Resources:webResources, lblLastPostJump %>" Text="<%$ Resources:webResources, lblLastPostJump %>"></asp:HyperLink></span>
                            <br />
                            <%# Common.TimeAgoTag((DateTime)DataBinder.Eval(Container.DataItem, "LastPostDate"),IsAuthenticated,Member.TimeOffset)%>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField InsertVisible="False">
                        <ItemTemplate>
                            <asp:ImageButton ID="Stick" SkinID="StickyTopic" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                runat="server" ToolTip="<%$ Resources:webResources, lblStick %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Sticky', 'Do you want to make topic sticky?');
            mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Make topic sticky ?'},'confirmHandlers.BeginRecieve');
            return false;"
                                CausesValidation="False" EnableViewState="False" OnClick="StickTopic" />
                            <asp:ImageButton ID="UnStick" SkinID="UnStickyTopic" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                runat="server" ToolTip="<%$ Resources:webResources, lblUnStick %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Sticky', 'Do you want to unstick the topic?');
            mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'UnStick topic ?'},'confirmHandlers.BeginRecieve');
            return false;"
                                CausesValidation="False" EnableViewState="False" OnClick="UnStickTopic" />
                            <asp:ImageButton ID="TopicLock" SkinID="LockTopic" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                runat="server" ToolTip="<%$ Resources:webResources, lbllock %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Lock', 'Do you want to lock the topic?');
            mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Lock Topic ?'},'confirmHandlers.BeginRecieve');
            return false;"
                                CausesValidation="False" EnableViewState="False" OnClick="LockTopic" />
                            <asp:ImageButton ID="TopicUnLock" SkinID="UnLockTopic" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                runat="server" ToolTip="<%$ Resources:webResources, lblUnlock %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm UnLock', 'Do you want to unlock the topic?');
            mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'UnLock Topic ?'},'confirmHandlers.BeginRecieve');
            return false;"
                                CausesValidation="False" EnableViewState="False" OnClick="UnLockTopic" />
                            <asp:ImageButton ID="TopicDelete" SkinID="DeleteMessage" Visible='<%# IsAdministrator %>' CommandArgument='<%# Eval("Id")%>'
                                runat="server" ToolTip="<%$ Resources:webResources, lblDelPost %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Delete', 'Do you want to delete the topic?');
            mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Delete Topic ?'},'confirmHandlers.BeginRecieve');
            return false;"
                                CausesValidation="False" EnableViewState="False" OnClick="DeleteTopic" />

                            <asp:HyperLink ID="hypEditTopic" EnableViewState="false" SkinID="EditTopic" runat="server"
                                Visible="False" Text="<%$ Resources:webResources, lblEditPost %>" ToolTip="<%$ Resources:webResources, lblEditPost %>"></asp:HyperLink>
                            <asp:HyperLink ID="hypReplyTopic" EnableViewState="false" SkinID="ReplyTopic" runat="server"
                                Text="<%$ Resources:webResources, lblReply %>" ToolTip="<%$ Resources:webResources, lblReply %>"></asp:HyperLink>
                            <asp:ImageButton ID="TopicSub" SkinID="Subscribe" CommandArgument='<%# Eval("Id")%>'
                                runat="server" ToolTip="<%$ Resources:webResources, lblSubscribeTopic %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Subscribe', 'Do you want to be notified when someone posts a reply?');
            mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Subscribe to Topic ?'},'confirmHandlers.BeginRecieve');
            return false;"
                                CausesValidation="False" EnableViewState="False" OnClick="TopicSubscribe" />
                            <asp:ImageButton ID="TopicUnSub" SkinID="UnSubscribe" CommandArgument='<%# Eval("Id")%>' CommandName="unsub"
                                runat="server" ToolTip="<%$ Resources:webResources, lblUnSubscribeTopic %>" OnClientClick="mainScreen.ShowConfirm(this, 'Confirm Remove', 'Do you want to remove notifications from topic?');
            mainScreen.LoadServerControlHtml(' Confirm Action',{'pageID':3,'data': 'Do you want to remove notifications from topic?'},'confirmHandlers.BeginRecieve');
            return false;"
                                CausesValidation="False" EnableViewState="False" OnClick="TopicSubscribe" />

                            <asp:HyperLink ID="hypNoArchiveTopic" EnableViewState="false" SkinID="NoArchiveTopic"
                                runat="server" Text="<%$ Resources:webResources, lblNoArchive %>" ToolTip="<%$ Resources:webResources, lblNoArchive %>"></asp:HyperLink>
                            <asp:HyperLink ID="hypArchiveTopic" EnableViewState="false" SkinID="ArchiveTopic"
                                runat="server" Text="<%$ Resources:webResources, lblArchive %>" ToolTip="<%$ Resources:webResources, lblArchive %>"></asp:HyperLink>
                            <asp:ImageButton ID="TopicApprove" SkinID="approve" runat="server" ToolTip="Approve Topic" />
                        </ItemTemplate>
                        <ItemStyle CssClass="buttonCol" />
                        <HeaderStyle Width="40px" />
                    </asp:TemplateField>
                </Columns>
                <HeaderStyle CssClass="category cattitle" />
                <RowStyle CssClass="row" />
                <AlternatingRowStyle CssClass="altrow" />
                <EmptyDataRowStyle HorizontalAlign="Center" CssClass="NoBorder"></EmptyDataRowStyle>
                <PagerTemplate>
                    <uc4:GridPager ID="pager" runat="server" PagerStyle="Lnkbutton" />
                </PagerTemplate>
            </asp:GridView>

        </ContentTemplate>
    </asp:UpdatePanel>
    <!-- Profile popup -->
    <asp:Panel ID="MPanel" runat="server" Style="display: none" EnableViewState="false">
        <div class="mainModalPopup mainModalBorder">
            <div class="mainModalInnerDiv mainModalInnerBorder">
                <div id="header" style="width: 100%;" class="clearfix">
                    <div class="mainModalDraggablePanelDiv">
                        <asp:Panel CssClass="mainModalDraggablePanel" ID="MPD" runat="server" EnableViewState="false">
                            <span class="mainModalTitle" id="spanTitle"></span>
                        </asp:Panel>
                    </div>
                    <div class="mainModalDraggablePanelCloseDiv">
                        <asp:ImageButton SkinID="CloseModal" runat="server" ID="clB" CausesValidation="false"
                            EnableViewState="false" />
                    </div>
                </div>
                <div class="mainModalContent">
                    <div id="mainModalContents">
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>
    <ajaxtoolkit:ModalPopupExtender ID="mpeModal" Drag="true" PopupDragHandleControlID="MPD" runat="server" PopupControlID="MPanel"
        TargetControlID="btnHid" BehaviorID="mbMain" BackgroundCssClass="modalBackground"
        CancelControlID="clB" OnCancelScript="mainScreen.CancelModal();" DropShadow="true" />
    <asp:Button runat="server" ID="btnHid" Style="display: none;" />
</asp:Content>
<asp:Content ID="rightcol" ContentPlaceHolderID="RightCol" runat="server">
    <snitz:SideBar runat="server" ID="sidebar" Show="Active,Events" />
</asp:Content>
<asp:Content runat="server" ID="Cpf1" ContentPlaceHolderID="CPF1">
    <div class="table-bottom">
        <div class="table-bottom-left">
            <uc5:PostButtons ID="pbBottom" runat="server"></uc5:PostButtons>
        </div>
        <div class="table-bottom-right">
            <snitz:JumpTo ID="JumpTo1" runat="server" />
        </div>
    </div>
    <div class="table-bottom-footer">
        <asp:Image ID="imgNewPosts" EnableViewState="false" SkinID="FolderNew" runat="server"
            GenerateEmptyAlternateText="true" />
        <asp:Label ID="lblNewPosts" runat="server" Text="<%$ Resources:webResources, lblNewPosts %>"></asp:Label>.
                <br />
        <asp:Image ID="imgOldPosts" EnableViewState="false" SkinID="Folder" runat="server"
            GenerateEmptyAlternateText="true" />
        <asp:Label ID="lblOldPosts" runat="server" Text="<%$ Resources:webResources, lblOldPosts %>"></asp:Label>.
                (<asp:Image ID="imgHotTopic" EnableViewState="false" SkinID="FolderHot" runat="server"
                    GenerateEmptyAlternateText="true" />
        <asp:Label ID="lblHotTopic" runat="server" Text="<%$ Resources:webResources, lblHotTopics %>"></asp:Label>)<br />
        <asp:Image ID="imgfolderNewLocked" EnableViewState="false" SkinID="FolderLocked"
            runat="server" GenerateEmptyAlternateText="true" />
        <asp:Label ID="lblLockedTopic" runat="server" Text="<%$ Resources:webResources, lblLockedTopic %>"></asp:Label>.       
    </div>
</asp:Content>
<asp:Content runat="server" ID="cpf" ContentPlaceHolderID="CPF2">
    <div id="sampleformdiv" title="Search Forum" style="display: none;">
        Search Subject/Message for<br />
        <input type="text" name="content" id="content" />
        <input type="submit" id="search" value="find" onclick="_forumSearch(this);" />
    </div>
</asp:Content>