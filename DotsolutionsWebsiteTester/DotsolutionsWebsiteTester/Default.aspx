﻿<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DotsolutionsWebsiteTester._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron text-center center-block" id="testsjumbotron">

        <h1><span>Website</span> tester</h1>
        <div class="row urlTextBoxRow">
            <%--<div class="col-md-8 col-xs-10 col-sm-8 col-lg-8 col-md-offset-2 col-xs-offset-1 col-sm-offset-2 col-lg-offset-2">--%>
            <div class="col-md-10 col-xs-10 col-sm-10 col-lg-10 col-md-offset-1 col-xs-offset-1 col-sm-offset-1 col-lg-offset-1">
                <div class="input-group">
                    <span class="input-group-addon">
                        <i class="glyphicon glyphicon-search"></i>
                    </span>
                    <asp:TextBox ID="UrlTextBox"
                        CssClass="form-control"
                        Text="http://www."
                        runat="server" />
                </div>
                <span class="help-block">Het adres van de website, inclusief http://www. Bijvoorbeeld http://www.example.com</span>
                <div class="center-block noselect detailedTestCheckBox">
                    <asp:CheckBox ID="TestCheckBox" runat="server" Text="Gedetailleerde testgegevens weergeven" Value="Freshness" />
                    <br />
                    <asp:CheckBox ID="ThreePageReportCheckBox" runat="server" Text="2-pagina verslag" Value="Freshness" />
                </div>
            </div>
        </div>

        <div class="hidden" id="invalidUrlHidden" runat="server">
            <div class="alert alert-danger col-md-12 col-lg-12 col-xs-12 col-sm-12" role="alert">
                <i class="glyphicon glyphicon-alert glyphicons-lg"></i><span class="" id="invalidUrl" runat="server"></span>
            </div>
        </div>
        <div class="row">
            <div class="col-md-4 col-md-offset-4 col-xs-10 col-xs-offset-1 col-sm-4 col-sm-offset-4 col-lg-4 col-lg-offset-4">
                <asp:Button ID="StartScanBtn"
                    Text="Test"
                    OnClick="StartScanBtn_Click"
                    CssClass="btn btn-custom btn-md"
                    runat="server" />
            </div>
        </div>


        <div class="row">
            <div class="col-md-4 col-md-offset-4 col-xs-10 col-xs-offset-1 col-sm-4 col-sm-offset-4 col-lg-4 col-lg-offset-4">
                <div class="btn btn-white btn-sm" id="ShowCheckboxes">
                    Selecteer tests
                </div>
            </div>
        </div>

        <div id="checkboxHolder" class="noselect">

            <div class="row">
                <div class="col-md-4 col-md-offset-4 col-xs-10 col-xs-offset-1 col-sm-4 col-sm-offset-4 col-lg-4 col-lg-offset-4">
                    <div class="btn btn-white btn-sm" id="CheckAllCheckboxes">Alles deselecteren</div>
                </div>
            </div>

            <div class="row checkboxlistrow text-left">
                <div class="col-md-4 col-xs-12 col-sm-4 col-lg-4">
                    <asp:CheckBoxList ID="TestsCheckBoxList1"
                        runat="server"
                        TextAlign="right">
                        <asp:ListItem Text="Actueelheid" Value="Freshness" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Afbeeldingen" Value="Images" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Analytics" Value="Analytics" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Binnenkomende links" Value="IncomingLinks" Selected="false" Enabled="true"></asp:ListItem>
                        <asp:ListItem Text="Code kwaliteit" Value="CodeQuality" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Facebook Pagina" Value="Facebook" Selected="false"></asp:ListItem>
                    </asp:CheckBoxList>
                </div>
                <div class="col-md-4 col-xs-12 col-sm-4 col-lg-4">
                    <asp:CheckBoxList ID="TestsCheckBoxList2"
                        runat="server">
                        <asp:ListItem Text="Google+ Pagina" Value="GooglePlus" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Headers" Value="Headings" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Hoeveelheid content" Value="AmountOfContent" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Interne links" Value="InternalLinks" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Meta tags" Value="MetaTags" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Mobiele compatibiliteit" Value="MobileCompatibility" Selected="false"></asp:ListItem>

                    </asp:CheckBoxList>
                </div>
                <div class="col-md-4 col-xs-12 col-sm-4 col-lg-4">
                    <asp:CheckBoxList ID="TestsCheckBoxList3"
                        runat="server">
                        <asp:ListItem Text="Pagina titels" Value="PageTitles" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Populariteit" Value="Popularity" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Printbaarheid" Value="Printability" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="Server gedrag" Value="ServerBehaviour" Selected="false"></asp:ListItem>
                        <%--<asp:ListItem Text="Sociale interesse" Value="SocialInterest" Selected="false"></asp:ListItem>--%>
                        <asp:ListItem Text="Twitter Pagina" Value="Twitter" Selected="false"></asp:ListItem>
                        <asp:ListItem Text="URL Formaat" Value="UrlFormat" Selected="false"></asp:ListItem>
                    </asp:CheckBoxList>
                </div>
            </div>
        </div>

    </div>
    <script type="text/javascript" src="Scripts/Custom/default.js"></script>

</asp:Content>
