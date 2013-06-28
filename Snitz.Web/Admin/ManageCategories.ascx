<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ManageCategories.ascx.cs" Inherits="Admin_ManageCategories" %>

<asp:Panel ID="Panel2" runat="server" CssClass="clearfix">
        <table border="0" cellpadding="2" cellspacing="0" style="width: 100%;" class="forumtable white">
            <tr>
                <td colspan="2" >
                    <asp:Label ID="errLbl" runat="server" Visible="False" ForeColor="Red"></asp:Label>
                </td>
            </tr>
            
            <tr>
                <td colspan="2" class="tableheader">
                    <asp:Label ID="Label2" runat="server" Text="Category Manager"></asp:Label>
                </td>
            </tr>
             
            <tr>
                <asp:HiddenField ID="catID" runat="server" />
                <td style="width: 35%;text-align:right;">
                <asp:Label runat="server" ID="statusLbl" Text="Category Status:&nbsp;"></asp:Label></td><td style="text-align:left">
                    <asp:DropDownList ID="catStatus"  runat="server">
                    <asp:ListItem Value="0" Text="Locked"></asp:ListItem>
                    <asp:ListItem Value="1" Text="UnLocked" Selected="True"></asp:ListItem>
                    </asp:DropDownList></td>
            </tr> 
            <tr>
                <td style="width: 35%;text-align:right;">
                <asp:Label runat="server" ID="modLable" Text="Category Moderation Level:&nbsp;"></asp:Label></td><td style="text-align:left">
                <asp:DropDownList runat="server" ID="catMod" Enabled="False">
                <asp:ListItem  Value="0" Selected="True" Text="Moderation not Allowed for this Category: "></asp:ListItem>
                <asp:ListItem Value="1" Text="Moderation Allowed for this Category: "></asp:ListItem>
                </asp:DropDownList></td>
            </tr>
             <tr>
                <td style="width: 35%;text-align:right;">
                <asp:Label runat="server" ID="subLabel" Text="Category Subscription Level:&nbsp;"></asp:Label></td><td style="text-align:left">
                <asp:DropDownList runat="server" ID="catSub" Enabled="False">
                <asp:ListItem  Value="0" Selected="True" Text="No Subscriptions Allowed"></asp:ListItem>
                <asp:ListItem Value="1" Text="Category Subscriptions Allowed"></asp:ListItem>
                <asp:ListItem Value="2" Text="Forum Subscriptions Allowed"></asp:ListItem>
                <asp:ListItem Value="3" Text="Topic Subscriptions Allowed"></asp:ListItem>
                </asp:DropDownList></td>
            </tr>
            <tr>
                <td style="width: 35%;text-align:right" valign="top">
                    <asp:Label runat="server" ID="nameLbl" Text="Category Name:&nbsp;"></asp:Label></td><td style="text-align:left">
                    <asp:TextBox ID="txtName" runat="server" Rows="1" Width="95%"></asp:TextBox></td>
            </tr>                    
            <tr>
                <td style="width: 35%;text-align:right" valign="top">
                    <asp:Label runat="server" ID="orderLbl" Text="Category Order:&nbsp;"></asp:Label></td><td style="text-align:left">
                    <asp:TextBox ID="txtOrder" runat="server" Rows="1" Width="10%"></asp:TextBox></td>
            </tr>   
            <tr>
                <td colspan="2" style="text-align:center;">
                    <asp:Button ID="btnSubmit" runat="server" Text="Update"  ValidationGroup="G1" OnClick="btnSubmit_Click"  />
                    <asp:Button ID="btnReset" runat="server" Text="Cancel" OnClick="btnReset_Click" CausesValidation="False"  />
                </td>
            </tr>
</table>
    <asp:RequiredFieldValidator ID="ReqNameID" runat="server"   ValidationGroup="G1" SetFocusOnError="true" ControlToValidate="txtName" Text="&nbsp" ErrorMessage="You need to specify a Category Name"></asp:RequiredFieldValidator></asp:Panel>
     <asp:RequiredFieldValidator runat="server" ValidationGroup="G1" ID="ReqOrder" ControlToValidate="txtOrder" SetFocusOnError="true" Text="&nbsp" ErrorMessage="You need to specify a Category Order"></asp:RequiredFieldValidator>
    <asp:RegularExpressionValidator ID="OrderVal"  ControlToValidate="txtOrder" runat="server" Text="&nbsp" ErrorMessage="Please specify a number for category order" ValidationExpression="[1-9][0-9]{0,3}"></asp:RegularExpressionValidator>
    <asp:ValidationSummary ID="ValidationSummary"    ShowMessageBox="true"  ShowSummary="false" DisplayMode="List" runat="server" />
 
 
 
 
 <asp:Panel ID="Panel1" runat="server" CssClass="ConfigForm clearfix" style="width:100%;" Width="100%" Wrap="False">
        <table border="0" cellpadding="0" cellspacing="0" style="width: 100%;" class="ForumList">
            <tr>
                <td class="tableheader" >
                    <asp:Label ID="Label1" runat="server" Text="Category Manager"></asp:Label>
                </td>
                <td class="tableheader" align="right">
                   <asp:CheckBox runat="server"  ID="chkDelForums" Checked="false" Text="Delete categories containg forums and posts&nbsp" />
                </td>
            </tr>
            <tr>
                <td colspan="2" >
                    <asp:Label ID="errLbl2" runat="server" Visible="False" ForeColor="Red"></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="2">

                    <asp:GridView ID="GridView1"  runat="server" AutoGenerateColumns="False" DataKeyNames="CAT_ID" DataSourceID="dsCats" CssClass="TopicsTable"  Width="100%" AllowPaging="True" EnableViewState="False" OnRowCommand="GridView1_RowCommand">
                        <Columns>
                             <asp:BoundField DataField="CAT_NAME" HeaderText="Name">
                                <ItemStyle HorizontalAlign="Center" VerticalAlign="Top" />
                                 <HeaderStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:BoundField DataField="CAT_ORDER" HeaderText="Order">
                                <ItemStyle HorizontalAlign="Center" VerticalAlign="Top" />
                                <HeaderStyle HorizontalAlign="Center" />
                            </asp:BoundField>
                            <asp:TemplateField>
                            <ItemTemplate>
                            <asp:ImageButton ID="editBtn" ImageUrl="~/images/icon_pencil.gif" runat="server" AlternateText="Edit Category" CommandName="EditClick"
                            CommandArgument='<%#Eval("CAT_ID") %>' ImageAlign="AbsMiddle" />
                            </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                             <ItemTemplate>
                              <asp:ImageButton  ID="lockBtn" ImageUrl="~/images/icon_lock.gif" runat="server" AlternateText="Lock Category" CommandName="LockClick"
                               CommandArgument='<%#Eval("CAT_ID") %>'  Visible='<%# (Eval("CAT_STATUS").ToString() == "1") %>' ImageAlign="AbsMiddle" /> 
                              
                            <asp:ImageButton  ID="unlockBtn" ImageUrl="~/images/icon_unlock.gif" runat="server" AlternateText="Unlock Category" CommandName="UnlockClick"
                            CommandArgument='<%#Eval("CAT_ID") %>'  Visible='<%# (Eval("CAT_STATUS").ToString() != "1") %>' ImageAlign="AbsMiddle" />
                            </ItemTemplate>
                            </asp:TemplateField>
                                   <asp:TemplateField>
                            <ItemTemplate>
                            <asp:ImageButton ID="delBtn" ImageUrl="~/images/icon_trashcan.gif" runat="server" AlternateText="Delete Category" CommandName="DeleteClick"
                            CommandArgument='<%#Eval("CAT_ID") %>' ImageAlign=AbsMiddle />
                            </ItemTemplate>
                            </asp:TemplateField>
                            </Columns>
                        <HeaderStyle CssClass="category" />
                        <EmptyDataTemplate>
                            No Categories defined
                        </EmptyDataTemplate>
                        <SelectedRowStyle BackColor="#FFE0C0" />
                        <PagerSettings Mode="NumericFirstLast" />
                        <PagerStyle CssClass="NoBorder" />
                    </asp:GridView>
                    <asp:ObjectDataSource ID="dsCats" runat="server"  SelectMethod="GetAllCatsInfoForCatManagement" TypeName="Snitz.DataBaseLayer.ForumDatasource">
                    </asp:ObjectDataSource>

                </td>
            </tr>            
            
        </table>

    </asp:Panel>