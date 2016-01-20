<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Headings.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Headings" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="Headings">
            <div class="panel panel-custom" id="HeadingsSession" runat="server">
                <div class="panel-heading">
                    <span id="HeadingsRating" runat="server">?</span><span class="title">Headers</span>
                </div>
                <div class="panel-body">
                    <div id="headingMessages" class="results percentageContainer" runat="server"></div>
                    <div class="hidden" id="headingTableHidden" runat="server">
                        <asp:Table ID="headingTable" CssClass="table" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow" BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Header" CssClass="col-md-9" />
                                <asp:TableHeaderCell Scope="Column" Text="Type" CssClass="col-md-1" />
                                <asp:TableHeaderCell Scope="Column" Text="URL" CssClass="col-md-2" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
