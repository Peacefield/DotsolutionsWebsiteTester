﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ServerBehaviour.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.ServerBehaviour" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="ServerBehaviour">
            <div class="panel panel-custom" id="ServerBehaviourSession" runat="server">
                <div class="panel-heading">
                    <span id="ServerBehaviourRating" runat="server">?</span> Server gedrag
                </div>
                <div class="panel-body">
                </div>
            </div>
        </div>

    </div>
</asp:Content>