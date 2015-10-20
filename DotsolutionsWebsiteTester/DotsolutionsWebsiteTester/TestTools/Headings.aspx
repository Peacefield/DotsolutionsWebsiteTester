﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Headings.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Headings" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="Headings">
            <div class="panel panel-custom" id="HeadingsSession" runat="server">
                <div class="panel-heading">
                    <span id="Rating" runat="server">?</span> Headers
                </div>
                <div class="panel-body">
                    <div id="headingMessages" class="results" runat="server"></div>
                    <div class="hidden" id="headingTableHidden" runat="server">
                        <div class="table-responsive">
                            <asp:Table ID="table" CssClass="table table-hover" runat="server">
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

    </div>
</asp:Content>