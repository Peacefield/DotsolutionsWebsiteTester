<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MobileCompatibility.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.MobileCompatibility" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="MobileCompatibility">
            <div class="panel panel-custom" id="MobileCompatibilitySession" runat="server">
                <div class="panel-heading">
                    <span id="MobileCompatibilityRating" runat="server">?</span> Mobiele compatibiliteit
                </div>
                <div class="panel-body">
                    <div id="MobileCompatibilityResults" class="results" runat="server"></div>
                    <div class="row">
                        <div class="col-xs-12 col-sm-12 col-md-6 col-lg-6">
                            <div id="computerImg" runat="server"></div>
                        </div>
                        <div class="col-xs-12 col-sm-12 col-md-6 col-lg-6">
                            <div id="mobileImg" runat="server"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
