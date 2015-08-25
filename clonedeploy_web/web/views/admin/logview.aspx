﻿<%@ Page Title="" Language="C#" MasterPageFile="~/views/masters/Admin.master" AutoEventWireup="true" Inherits="views.admin.Logview" CodeFile="logview.aspx.cs" %>

<%@ MasterType VirtualPath="~/views/masters/Admin.master" %>
<%@ Reference virtualPath="~/views/masters/Site.master" %>
<asp:Content ID="Content" ContentPlaceHolderID="SubContent" runat="Server">
    <script type="text/javascript">
        $(document).ready(function() {
            $('#logSettings').addClass("nav-current");
        });
    </script>
    <div class="size-7 column">
        <asp:DropDownList ID="ddlLog" runat="server" CssClass="ddlist" AutoPostBack="True">
        </asp:DropDownList>
    </div>
    <br class="clear"/>
    <div class="size-4 column" style="float: right; margin: 0;">
        <asp:DropDownList ID="ddlLimit" runat="server" CssClass="ddlist" Style="float: right; width: 75px;" AutoPostBack="true" OnSelectedIndexChanged="ddlLimit_SelectedIndexChanged">
            <asp:ListItem>10</asp:ListItem>
            <asp:ListItem>25</asp:ListItem>
            <asp:ListItem>50</asp:ListItem>
            <asp:ListItem>100</asp:ListItem>
            <asp:ListItem>All</asp:ListItem>
        </asp:DropDownList>
        <br class="clear"/>
        <asp:LinkButton ID="btnExportLog" runat="server" Text="Export Log" CssClass="submits" OnClick="btnExportLog_Click"></asp:LinkButton>
    </div>
    <br class="clear"/>
    <asp:GridView ID="GridView1" runat="server" CssClass="Gridview log" ShowHeader="false">
    </asp:GridView>
</asp:Content>