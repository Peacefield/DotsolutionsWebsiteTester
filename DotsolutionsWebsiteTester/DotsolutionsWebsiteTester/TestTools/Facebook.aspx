﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Facebook.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Facebook" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        
        <div id="Facebook">
            <div class="panel panel-custom" id="FacebookSession" runat="server">
                <div class="panel-heading">
                    <span id="FacebookRating" runat="server">?</span><span class="title">Facebook</span>
                </div>
                <div class="panel-body">
                <div id="FacebookResults" runat="server"></div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>