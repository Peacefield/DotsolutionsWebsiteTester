﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PageTitles.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.PageTitles" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="PageTitles">
            <div class="panel panel-custom" id="PageTitlesSession" runat="server">
                <div class="panel-heading">
                    <span id="PageTitlesRating" runat="server">?</span><span class="title">Pagina titels</span>
                </div>
                <div class="panel-body">
                    <div id="PageTitleResults" class="results percentageContainer" runat="server"></div>
                    <div class="hidden" id="PageTitlesTableHidden" runat="server">
                        <asp:Table ID="PageTitlesTable" CssClass="table" runat="server">
                            <asp:TableHeaderRow BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Titel" CssClass="col-md-6" />
                                <asp:TableHeaderCell Scope="Column" Text="Pagina" CssClass="col-md-6" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
