<%@ Page Title="Pay Supplier" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/MainMaster.Master" AutoEventWireup="true" CodeBehind="SuppPayment2.aspx.cs" Inherits="LiquidC.Supplier.SuppPayment2" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .WindowsStyle .ajax__combobox_itemlist {
            position: inherit !important;
            margin-top: 10px;
        }

        th.num, td.num {
            text-align: left;
        }
    </style>
    <style type="text/css">
        .AutoExtendLeft {
            /* This one goes to the right */
            font-family: Verdana;
            font-size: x-small;
            font-weight: normal;
            border: solid 1px #006699;
            line-height: 20px;
            padding: 10px;
            background-color: White;
            margin-left: 5px;
            width: 250px !important;
            max-height: 150px;
            overflow-y: auto;
            /* prevent horizontal scrollbar */
            overflow-x: hidden;
            /* add padding to account for vertical scrollbar */
            padding-right: 20px;
        }

        .AutoExtendRight {
            /* This one goes to the left rather than right */
            font-family: Verdana;
            font-size: x-small;
            font-weight: normal;
            border: solid 1px #006699;
            line-height: 20px;
            padding: 10px;
            background-color: White;
            margin-left: -200px;
            width: 250px !important;
            max-height: 150px;
            overflow-y: auto;
            /* prevent horizontal scrollbar */
            overflow-x: hidden;
            /* add padding to account for vertical scrollbar */
            padding-right: 20px;
        }

        .style1 {
            width: 100%;
        }

        .wmc {
            font-weight: lighter;
            font-style: italic;
            font-size: smaller;
        }
        .auto-style1 {
            width: 275px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="hidden" id="EmptyData" runat="server">
        <table>
            <tr>
                <td class="td-color">No Supplier Exist on Account
                    <asp:Button ID="btnAddSupplier" runat="server" CssClass="x-btn main" Text="Add Supplier" OnClick="btnAddSupplier_Click" /></td>
            </tr>
        </table>
    </div>
    <div id="Data" runat="server" class="pay_invoices">
        <div class="margin-top-10 pure-form pure-form-stacked">
            <fieldset>
                <legend>Pay Invoices</legend>

                <div class="fl margin-right-10">
                    <label for="cmbSupplier">
                        <asp:Label ID="lblquickSearch" runat="server" Text="Select Supplier"></asp:Label></label>
                    <div class="account-info">
                        <b>Using the Quick Search box: To see all SUPPLIERS enter the symbol %. Or enter parts of the SUPPLIER code or name to search quickly.</b>
                    </div>
                        <div style="display: flex; align-items: center">
                            <label style="margin-right: 1rem">                   
                            Quick Search Text:
                                <asp:TextBox ID="txtSearchKey" ToolTip="Enter parts of the supplier code or name to search quickly" AutoPostBack="true" runat="server" OnTextChanged="txtSearchKey_TextChanged" AutoCompleteType="Disabled" />
                            </label>
                            <div class="fl margin-right-10" style="margin-top:12px">
                        <cc1:AutoCompleteExtender
                            MinimumPrefixLength="0"
                            ID="aceSearchKey"
                            CompletionInterval="200"
                            CompletionSetCount="10"
                            FirstRowSelected="true" runat="server"
                            ContextKey="SUPP"
                            TargetControlID="txtSearchKey" ServiceMethod="GetList" ServicePath="../Autocomplete.asmx"
                            EnableCaching="true"
                            CompletionListCssClass="AutoExtendLeft"
                            CompletionListElementID="divwidth"
                            OnClientItemSelected="GetAcct" UseContextKey="true" />
                        <%--<cc1:AutoCompleteExtender ID="aceContact" runat="server" ServiceMethod="FilterContact" ServicePath="SuppPayment2.aspx" TargetControlID="txtSearchKey" CompletionInterval="1000" MinimumPrefixLength="0" EnableCaching="true" />--%>
                      
                                <cc1:ComboBox ID="cmbSupplier" ToolTip="Use the Quick Search box above to load the supplier you want" CssClass="WindowsStyle" runat="server" DropDownStyle="DropDownList" DataTextField="Business_Name" DataValueField="PL_Code" OnSelectedIndexChanged="cmbSupplier_SelectedIndexChanged" AutoCompleteMode="SuggestAppend" AutoPostBack="True" Width="250px" MaxLength="50"></cc1:ComboBox>
                      </div>
                            <div class="fl margin-right-10" style="">
                        <label for="TxtBalance"> Balance</label>
                        <asp:TextBox ID="TxtBalance" runat="server"  ReadOnly="true"></asp:TextBox>
                    </div>
                     <div class="fl margin-right-10" style="">
                        <label for="TxtCurBalance"> Currency Balance</label>
                        <asp:TextBox ID="TxtCurBalance" runat="server" ReadOnly="true"></asp:TextBox>
                    </div>
                                </div>
                      
                </div>
                <div class="grid-item" style="width:100%">
                    <div class="fl margin-right-10" style="margin-top: 1rem">
                        <label for="ddBill">Billed In:</label>
                        <asp:DropDownList ID="ddBill" runat="server" DataValueField="CurrCode" DataTextField="CurrCode" AppendDataBoundItems="True" AutoPostBack="True" OnSelectedIndexChanged="ddBill_SelectedIndexChanged">
                            <asp:ListItem Value="">Please Select</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="fl margin-right-10" style="margin-top: 1rem">
                        <label for="tbDateFrom">Date From:</label>
                        <asp:TextBox ID="tbDateFrom" runat="server" AutoPostBack="True" OnTextChanged="tbDateFrom_TextChanged"></asp:TextBox>
                    </div>
                    <div class="fl margin-right-10" style="margin-top: 1rem">
                        <label for="tbDateTo">Date To:</label>
                        <asp:TextBox ID="tbDateTo" runat="server" AutoPostBack="True" OnTextChanged="tbDateTo_TextChanged"></asp:TextBox>
                    </div>
                </div>
                <div style="margin-top: 1rem;">
                    <table id="tLevel1" runat="server" class="">
                        <tr>
                            <td>
                                 <div class="grid-item" style="margin: .5rem 0">
                                    <div class="fl margin-right-10">
                                        <label for="txtPayDate">Pay Date:</label>
                                        <asp:TextBox ID="txtPayDate" runat="server" OnTextChanged="txtPayDate_TextChanged" AutoPostBack="True"></asp:TextBox>
                                    </div>
                                    <div class="fl margin-right-10">
                                        <label for="ddBank">Pay From:</label>
                                        <asp:DropDownList ID="ddBank" runat="server" DataValueField="ReceiptsControl" DataTextField="Detail" AppendDataBoundItems="True" OnSelectedIndexChanged="ddBank_SelectedIndexChanged" AutoPostBack="True">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="fl margin-right-10">
                                        <label for="ddBank">Exchange Variance:</label>
                                        <asp:TextBox ID="txtexchVar" runat="server" Text="0.00"></asp:TextBox>
                                    </div>
                                    <div class="fl margin-right-10">
                                        <label for="lblexchrate">Exchange Rate:</label>
                                         <asp:TextBox ID="lblexchrate" Text="1" runat="server"></asp:TextBox>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>

                <div class="row stacked-controls fl margin-right-10">
                    <asp:Button ID="btnPayAll" runat="server" Text="Set Page Transactions" ToolTip="Tick all transactions" CssClass="x-btn main" OnClick="btnPayAll_Click" />
                    <asp:Button ID="btnUnpayAll" runat="server" CssClass="x-btn main" Text="Reset Page Transactions" ToolTip="UnTick and reset all transactions amounts received" OnClick="btnUnpayAll_Click" />
                    <asp:Button ID="btnStatement" runat="server" CssClass="x-btn main" Text=" Print Remittance" OnClick="btnStatement_Click" />
                    <asp:Button ID="btnAmend" runat="server" CssClass="x-btn main" Text="Amend allocations" Visible="True" OnClick="btnAmend_Click" />
                    <asp:Button ID="Close1" runat="server" CssClass="x-btn main" Text="Go To Invoices" OnClick="Close1_Click" /> <asp:Button ID="btnOrders" runat="server"  Text=" Go to Orders" CausesValidation="false" CssClass="x-btn main" OnClick="btnOrders_Click" />
                </div>

            </fieldset>
            <cc1:CalendarExtender ID="CalendarExtender1" runat="server" PopupButtonID="tbDateFrom" TargetControlID="tbDateFrom"></cc1:CalendarExtender>
            <cc1:CalendarExtender ID="CalendarExtender2" runat="server" PopupButtonID="tbDateTo" TargetControlID="tbDateTo"></cc1:CalendarExtender>
        </div>
     
        <div class="pure-form pure-form-aligned">
            <table style="width: 100%">
                <tr>
                    <td>
                        <asp:Panel ID="pnlPayment" runat="server">
                            <div class="grid-item">
                                <div class="fl margin-right-10">
                                    <label>
                                        <asp:HyperLink ID="hlPaymentRecd" runat="server" ToolTip="Click to add a payment type" Font-Bold="true">On Account Payment:</asp:HyperLink>
                                    </label>
                                        &nbsp;<asp:TextBox ID="txtOnAccount" runat="server" ></asp:TextBox>
                                </div>
                                <div class="fl margin-right-10">
                                    <label for="txtDebitNote">Discount:</label>
                                    <asp:TextBox ID="txtDiscOnAccount" runat="server" Text="0.00" ></asp:TextBox>
                                    <cc1:FilteredTextBoxExtender ID="FilteredTextBoxExtender1" runat="server" Enabled="True" ValidChars=".-0123456789" FilterMode="ValidChars" TargetControlID="txtDiscOnAccount"></cc1:FilteredTextBoxExtender>
                                </div>
                                <div class="fl margin-right-10">
                                    <label for="txtDiscountNominal">Discount Nominal:</label>
                                    <asp:TextBox ID="txtDiscNominalOnAccount" runat="server"></asp:TextBox>
                                    <cc1:AutoCompleteExtender
                                        runat="server"
                                        CompletionInterval="200"
                                        CompletionSetCount="5"
                                        EnableCaching="true"
                                        FirstRowSelected="true"
                                        MinimumPrefixLength="0"
                                        ServiceMethod="GetList"
                                        OnClientItemSelected="GetCodeOnAccount"
                                        ServicePath="~/AutoComplete.asmx"
                                        CompletionListCssClass="AutoExtendLeft"
                                        ContextKey="NOM"
                                        ID="aceDiscNominalOnAccount"
                                        TargetControlID="txtDiscNominalOnAccount"
                                        UseContextKey="true" />
                                </div>
                                <div class="fl margin-right-10">
                                        &nbsp; <label>Payment Reference:</label>
                                        &nbsp;<asp:TextBox ID="txtReference" MaxLength="45"  runat="server" ></asp:TextBox>
                                </div>
                            </div>
                                <div class="fl margin-right-10" style="margin-bottom: 1rem">
                                    <asp:Button ID="PaytRecd" runat="server" OnClick="PaytRecd_Click" Text="Accept" CssClass="x-btn" />
                                    <asp:Label ID="lblText" runat="server"></asp:Label>
                                </div>
                        </asp:Panel>
                    </td>
                </tr>
                <tr style="margin: 1rem 0">
                    <td>
                          <b>Note : Use the &quot;Tick Box&quot; to select full amount to pay. You can enter the amount to pay directly in its Box.<u>For transactions with negative amount, ensure you enter a negative amount in its &quot;Amount to pay&quot; box</u></b> 

                    </td>
                </tr>
                <tr>
                    <td class="td-color" colspan="2">
                        <asp:GridView ID="gvInvoices" runat="server"
                            AutoGenerateColumns="False"
                            Width="100%"
                            AllowPaging="True"
                            PageSize="20"
                            AllowSorting="True"
                            CssClass="data-table"
                            AlternatingRowStyle-CssClass="alt"
                            OnPreRender="gvInvoices_PreRender" OnSorting="gvInvoices_Sorting" OnRowDataBound="gvInvoices_RowDataBound" OnPageIndexChanging="gvInvoices_PageIndexChanging">
                            <Columns>
                                <asp:BoundField DataField="NLT_Date" DataFormatString="{0:d}" HtmlEncode="false" HeaderStyle-CssClass="num" ItemStyle-HorizontalAlign="Left"
                                    HeaderText="Date" SortExpression="NLT_Date" />
                                <asp:BoundField DataField="NLT_DeliverDate" DataFormatString="{0:d}" HeaderText="Settle/Due" HeaderStyle-CssClass="num" ItemStyle-HorizontalAlign="Left"
                                    HtmlEncode="False" SortExpression="NLT_DeliverDate" />
                                <asp:BoundField DataField="NLT_Source" HeaderText="Type" SortExpression="NLT_Source" ItemStyle-HorizontalAlign="Left"/>
                                <asp:BoundField DataField="NLT_TheirRef" HeaderText="Reference" SortExpression="NLT_TheirRef" ItemStyle-HorizontalAlign="Left"
                                    HtmlEncode="False" />
                                <asp:BoundField DataField="NLT_InvNo" HeaderText="Transaction No" SortExpression="NLT_InvNo" HeaderStyle-CssClass="num" ItemStyle-HorizontalAlign="Left" />
                                <asp:BoundField DataField="NLT_CurrCode" HeaderText="Currency" SortExpression="NLT_CurrCode" HeaderStyle-CssClass="num">
                                    <FooterStyle HorizontalAlign="Left" />
                                    <ItemStyle HorizontalAlign="Left" />
                                </asp:BoundField>
                                <asp:BoundField DataField="Amount" DataFormatString="{0:n}" HtmlEncode="false" HeaderText="Amount" HeaderStyle-CssClass="numbers"
                                    ReadOnly="True" SortExpression="Amount">
                                    <FooterStyle HorizontalAlign="Right" />
                                    <ItemStyle HorizontalAlign="Right" Width="90px" />
                                </asp:BoundField>
                                <asp:BoundField DataField="Paid" DataFormatString="{0:n}" HtmlEncode="false" HeaderText="Paid" HeaderStyle-CssClass="numbers"
                                    ReadOnly="True" SortExpression="Paid">
                                    <FooterStyle HorizontalAlign="Right" />
                                    <ItemStyle HorizontalAlign="Right" Width="90px" />
                                </asp:BoundField>
                                <asp:TemplateField HeaderText="Balance" HeaderStyle-CssClass="numbers">
                                    <ItemStyle HorizontalAlign="Right" Width="90px" />
                                    <FooterStyle HorizontalAlign="Right" />
                                </asp:TemplateField>
                                <asp:BoundField DataField="GBP" DataFormatString="{0:n}" HtmlEncode="false" HeaderText="Home Currency" HeaderStyle-CssClass="numbers"
                                    SortExpression="GBP">
                                    <FooterStyle HorizontalAlign="Right" />
                                    <ItemStyle HorizontalAlign="Right" Width="90px" />
                                </asp:BoundField>
                                <asp:TemplateField HeaderText="Amount to pay" ItemStyle-Width="130px" HeaderStyle-CssClass="numbers" ItemStyle-HorizontalAlign="Right">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tbToAllocate" runat="server" Width="130px" Text='<%# Bind("ToPay", "{0:N}") %>'></asp:TextBox>
                                    </ItemTemplate>
                                    <FooterStyle HorizontalAlign="Right" />
                                    <FooterTemplate>
                                        <asp:Label ID="lblTotal" runat="server"></asp:Label>
                                    </FooterTemplate>
                                    <ItemStyle Width="90px" />
                                </asp:TemplateField>
                                <asp:BoundField DataField="Disc" DataFormatString="{0:n}" HtmlEncode="false" HeaderText="Discount"
                                    SortExpression="Disc" HeaderStyle-CssClass="hidden" FooterStyle-CssClass="hidden"
                                    ItemStyle-CssClass="hidden">
                                    <FooterStyle HorizontalAlign="Right" />
                                    <HeaderStyle CssClass="hidden" />
                                    <ItemStyle HorizontalAlign="Right" />
                                </asp:BoundField>
                                <asp:TemplateField HeaderText="Pay All">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="cb1" runat="server" AutoPostBack="true" OnCheckedChanged="cb1_CheckedChanged" />
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                </asp:TemplateField>
                                 <asp:HyperLinkField DataNavigateUrlFields="NLT_InvNo,NLT_Ref" DataNavigateUrlFormatString="~/Supplier/suppinvoice.aspx?InvNo={0}&Code={1}&Source=PI"
                                                 Text="View">
                                                <FooterStyle HorizontalAlign="Left" />
                                                <HeaderStyle HorizontalAlign="Left" />
                                            </asp:HyperLinkField>
                            </Columns>
                            <EmptyDataTemplate>
                                There are  no items to pay
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </td>
                </tr>
            </table>
            <table style="width: 100%">
                <tr>
                    <td class="td-color" style="text-align: right">
                        <table id="tcalc" runat="server">
                            <tr>
                                <td class="grid-item" style="align-items: center;">
                                    <div class="td-color" style="text-align: left">
                                        <asp:Button ID="btnGet" runat="server" CssClass="x-btn main" Text="Calculate Total Amounts To Pay" OnClick="btnGet_Click" />
                                    </div>
                                <%-- </tr>
                                <tr>--%>
                                    <div class="td-color" style="text-align: right">Transaction Currency
                                        <asp:Label CssClass="labels" ID="lblCurSel" runat="server" Text=""></asp:Label>
                                  <%--  </div>
                                    <div class="td-color">--%>
                                        <asp:Label CssClass="labels" ID="lblCurTotals" runat="server" Text="0.00"></asp:Label>
                                    </div>
                            <%--    </tr>
                                <tr>--%>
                                    <div class="td-color" style="text-align: right">Base Currency
                                        <asp:Label CssClass="labels" ID="lblDefCurr" runat="server" Text=""></asp:Label>
                                   <%-- </div>
                                    <div class="td-color">--%>
                                        <asp:Label CssClass="labels" ID="lblTotals" runat="server" Text="0.00"></asp:Label>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td class="td-color" colspan="2">
                        <table id="tLevel2" runat="server" class="" style="width: 100%">
                            <tr>
                                <td class="td-color">
                                    <div class="margin-top-10 pure-form pure-form-stacked">
                                        <fieldset class="grid-item">
                                            <div class="fl margin-right-10" style="display: block">
                                                <label for="txtDiscount">Amount Deducted:</label>
                                                <asp:TextBox ID="txtDiscount" AutoPostBack="false" runat="server" Text="0.00" OnTextChanged="txtDiscount_TextChanged"></asp:TextBox>
                                                <cc1:FilteredTextBoxExtender ID="txtDiscount_FilteredTextBoxExtender" ValidChars=".0123456789" FilterMode="ValidChars" runat="server" Enabled="True" TargetControlID="txtDiscount"></cc1:FilteredTextBoxExtender>
                                            </div>
                                            <div class="fl margin-right-10">
                                                <label for="txtPayRef">Payment Reference:</label>
                                                <asp:TextBox ID="txtPayRef" AutoPostBack="false" MaxLength="45" runat="server"></asp:TextBox>
                                            </div>
                                            <div class="labels">
                                                Bank Value:
                                                <asp:Label ID="lblbankcurr" runat="server" Text="£"></asp:Label>
                                                <asp:TextBox ID="lblBankValue" runat="server" AutoPostBack="true" Text="" OnTextChanged="lblBankValue_TextChanged"></asp:TextBox>
                                            </div>                                            
                                            <div class="labels">
                                                Net Value:
                                                <asp:Label ID="lblnetCurr" runat="server" Text="£"></asp:Label>
                                                <asp:Label ID="lblTranValue" runat="server" Text=""></asp:Label>
                                            </div>                                          
                                        </fieldset>
                                        <cc1:CalendarExtender ID="CalendarExtender3" runat="server" PopupButtonID="txtPayDate" TargetControlID="txtPayDate"></cc1:CalendarExtender>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td class="td-color" style="text-align: left">
                                    <table style="width: 50%">
                                        <tr>
                                            <td style="vertical-align: bottom">
                                                <asp:Button ID="bnCalculate" runat="server" Text="Calculate Payment Incl. Deduction" Width="100%" CssClass="x-btn main" OnClick="bnCalculate_Click" />
                                            </td> 
                                            <td style="display: flex; align-items: center; margin-left: 1rem; width: 100%">                                               
                                                <asp:CheckBox ID="chkCalc" Text="Calculate Totals" Checked="true" runat="server" />
                                                <asp:Button ID="btnPay" runat="server" OnClientClick="this.disabled=true;" Width="100%" Style="font-size:0.9rem" Text="Pay Invoices" CssClass="x-btn main" OnClick="btnPay_Click" UseSubmitBehavior="False" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>

                </tr>
            </table>
             <asp:SqlDataSource ID="SqlDSNomUpdate" runat="server" ConnectionString="<%$ ConnectionStrings:LocalConnection %>"
                InsertCommand="INSERT INTO nltran(NL_Code, NLT_YearEnd, NLT_Date, NLT_Period, NLT_Source, NLT_Ref, NLT_Detail, NLT_Net, NLT_VAT, NLT_VAT_Code, NLT_Audit, NLT_CurrCode, NLT_ExchRate, NLT_CurrNet, NLT_CurrVAT, NLT_InvNo, NLT_TheirRef, NLT_Paid, NLT_PaidRef,NLT_CurrPaid,NLT_PayRef) VALUES (@NLCode, @YE, @Date, @Period, 'PP', @Ref, @Detail, @Net, 0, 'NA', @Audit, @CurrCode, @ExchRate, @CurrNet, 0, @InvNo, @TheirRef, @Paid, @PaidRef,@CurrPaid,@PayRef)"
                SelectCommand="SELECT TOP 1 NLT_Key FROM NLTran">
                <InsertParameters>
                    <asp:Parameter Name="NLCode" Type="String" />
                    <asp:Parameter Name="YE" Type="DateTime" />
                    <asp:Parameter Name="Date" Type="DateTime" />
                    <asp:Parameter Name="Period" Type="Int32" />
                    <asp:Parameter Name="Ref" Type="String" />
                    <asp:Parameter Name="Detail" Type="String" />
                    <asp:Parameter Name="Net" Type="Double" />
                    <asp:Parameter Name="Audit" Type="String" />
                    <asp:Parameter DefaultValue="£" Name="CurrCode" Type="String" />
                    <asp:Parameter DefaultValue="" Name="ExchRate" Type="Double" />
                    <asp:Parameter Name="CurrNet" Type="Double" />
                    <asp:Parameter Name="InvNo" Type="String" />
                    <asp:Parameter Name="TheirRef" Type="String" />
                    <asp:Parameter Name="Paid" Type="Double" />
                    <asp:Parameter Name="PaidRef" Type="String" />
                    <asp:Parameter Name="CurrPaid" Type="Double" />
                      <asp:Parameter Name="PayRef" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>
            <script type="text/javascript">

                function GetAcct(sender, eventArgs) {

                    var val = eventArgs.get_value();
                    var sdr = sender.get_id();

                    var tb = sdr.replace("aceSearchKey", "txtSearchKey");

                    document.getElementById(tb).value = val;
                     __doPostBack('', '');
                }
                 function GetCodeOnAccount(sender, eventArgs) {
                    // The sender will be the autoextender and id should be ace...
                    // The textbox id should start tb...        
                    var sdr = sender.get_id();
                    var val = eventArgs.get_value();
                    var obj = val.split(":");
                    if (obj.length > 1) {
                        val = obj[1];
                    }
                    //alert(sdr);
                    // change to TextBox id
                    var tb = sdr.replace("aceDiscNominalOnAccount", "txtDiscNominalOnAccount");
                    // alert(tb);
                    document.getElementById(tb).value = val;
                }
            </script>
        </div>
    </div>
</asp:Content>
