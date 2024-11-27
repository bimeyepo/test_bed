using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LiquidC.Business_Layer.Shared;
using System.Data;
using LiquidC.Business_Layer.User;
using System.Configuration;
using LiquidC.Business_Layer.Company;
using System.Web.Services;
using System.Data.SqlClient;
using System.Drawing;

namespace LiquidC.Supplier
{
    public partial class SuppPayment2 : System.Web.UI.Page
    {
        MainMaster mainMaster;
        UserObject loggedUser = new UserObject();
        CompanySystem compUser = new CompanySystem();
        Business_Layer.Company.Company companyData = new Business_Layer.Company.Company();

        double[] dblTotal = new double[11];
        string strSessionCurr = null;

        String conceptdb;
        ConceptDBMethods sv;
        string compConnect;
        AccountsDBMethods accountdb;
        protected void txtSearchKey_TextChanged(object sender, EventArgs e)
        {
            if (txtSearchKey.Text.Trim() == "")
                txtSearchKey.Text = "%";

            String[] search = txtSearchKey.Text.Split(':');
            string dSearch = "";
            if (search.Length > 1)
            {
                dSearch = search[1].Trim();
            }
            else { dSearch = txtSearchKey.Text; }
            txtSearchKey.Text = dSearch;
            SqlParameter[] SqlParams = { new SqlParameter("@dSearch", dSearch) };
            DataTable dtcontacts = accountdb.GeneralQueryTableWithParams("Select top 50 PL_Code,RTRIM(LTRIM(ISNULL(Business_Name,PL_Code)))  + ' - ' + PL_Code + '' as Business_Name from Contacts where ISNULL(Rtrim(Ltrim(SL_Code)),'')='' and (ISNULL(PL_Code,'') like '%' + @dSearch + '%' or  ISNULL(Business_Name,'') like '%' + @dSearch + '%') and ISNULL(Rtrim(Ltrim(PL_Code)),'')<>'' order by RTRIM(LTRIM(Business_Name))", SqlParams);
            cmbSupplier.Items.Clear();
            cmbSupplier.DataSource = dtcontacts;
            cmbSupplier.DataBind();
            if (dtcontacts.Rows.Count > 0)
            {
                cmbSupplier.SelectedValue = dtcontacts.Rows[0]["PL_Code"].ToString();
                Session["SelectedSupplier"] = cmbSupplier.SelectedValue;
                cmbSupplier_SelectedIndexChanged(sender, e);
            }
            else
            {
                dSearch = "%";
                dtcontacts = accountdb.GeneralQueryTableWithParams("Select top 50 PL_Code,RTRIM(LTRIM(ISNULL(Business_Name,PL_Code)))  + ' - ' + PL_Code + '' as Business_Name from Contacts where ISNULL(Rtrim(Ltrim(SL_Code)),'')='' and (ISNULL(PL_Code,'') like '%' + @dSearch + '%' or  ISNULL(Business_Name,'') like '%' + @dSearch + '%') and ISNULL(Rtrim(Ltrim(PL_Code)),'')<>'' order by RTRIM(LTRIM(Business_Name))", SqlParams);
                cmbSupplier.Items.Clear();
                cmbSupplier.DataSource = dtcontacts;
                cmbSupplier.DataBind();
                if (dtcontacts.Rows.Count > 0)
                {
                    cmbSupplier.SelectedValue = dtcontacts.Rows[0]["PL_Code"].ToString();
                    Session["SelectedSupplier"] = cmbSupplier.SelectedValue;
                    cmbSupplier_SelectedIndexChanged(sender, e);
                }
                txtSearchKey.Text = dSearch;
            }
        }
        [WebMethod]
        public static String[] FilterContact(string prefixText, int count)
        {
            if (HttpContext.Current.Session["LoggedUser"] == null || HttpContext.Current.Session["LoggedUserActiveCompanyData"] == null)
            {
                HttpContext.Current.Response.Redirect("~/user/login.aspx", false);
            }

            UserObject loggedUser = (UserObject)HttpContext.Current.Session["LoggedUser"];
            CompanySystem compUser = (CompanySystem)HttpContext.Current.Session["LoggedUserActiveCompanyData"];

            String conceptdb = ConfigurationManager.ConnectionStrings["ConceptSystem"].ToString();
            ConceptDBMethods sv = new ConceptDBMethods(conceptdb);
            string compConnect = conceptdb.Replace("ConceptSystem", compUser.CS_CoCode.ToUpper());
            AccountsDBMethods accountdb = new AccountsDBMethods(compConnect);
            DataTable dtcontacts = accountdb.GeneralQueryTable("Select top 5 PL_Code,RTRIM(LTRIM(ISNULL(Business_Name,PL_Code)))  + ' : ' + PL_Code + '' as Business_Name from Contacts where ISNULL(Rtrim(Ltrim(SL_Code)),'')='' and (ISNULL(PL_Code,'') like'%" + prefixText + "%' or  ISNULL(Business_Name,'') like'%" + prefixText + "%') order by RTRIM(LTRIM(Business_Name))");

            List<String> returnedList = new List<String>();

            count = 20;
            foreach (DataRow company in dtcontacts.Rows)
            {
                returnedList.Add(company["Business_Name"].ToString());
            }

            return returnedList.ToArray();
        }
        protected void Page_PreLoad(object sender, EventArgs e)
        {
            if (Session["LoggedUser"] == null || Session["LoggedUserActiveCompanyData"] == null)
            {
                Response.Redirect("~/user/sessionlogout.aspx"); ;
            }
            if (Session["DateFormat"] != null)
            {
                //CalendarExtender1.Format = Session["DateFormat"].ToString();
                //CalendarExtender2.Format = Session["DateFormat"].ToString();
                //CalendarExtender3.Format = Session["DateFormat"].ToString();
                CalendarExtender1.Format = "dd/MM/yyyy";
                CalendarExtender2.Format = "dd/MM/yyyy";
                CalendarExtender3.Format = "dd/MM/yyyy";
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            string code = Request.QueryString["code"];
            mainMaster = Master as MainMaster;
            mainMaster.LabelTitle = "";
            mainMaster.LabelValue = "";
            mainMaster.ShowMsgClass = "hidden";
            if (Session["LoggedUser"] == null || Session["LoggedUserActiveCompanyData"] == null)
            {
                Response.Redirect("~/user/sessionlogout.aspx"); ;
            }
            loggedUser = (UserObject)Session["LoggedUser"];
            compUser = (CompanySystem)Session["LoggedUserActiveCompanyData"];

            conceptdb = ConfigurationManager.ConnectionStrings["ConceptSystem"].ToString();
            sv = new ConceptDBMethods(conceptdb);
            compConnect = conceptdb.Replace("ConceptSystem", compUser.CS_CoCode.ToUpper());
            accountdb = new AccountsDBMethods(compConnect);

            if (Session["CompanyData"] == null)
            {
                companyData = accountdb.AccCompanyData();
                Session["CompanyData"] = companyData;
            }
            else
            {
                companyData = (Business_Layer.Company.Company)Session["CompanyData"];
            }
            SqlDSNomUpdate.ConnectionString = compConnect;
            if (!IsPostBack)
            {
                btnOrders.Visible = false;
                if (sv.getModule(compUser.CS_CoCode.ToUpper(), "POP") == "POP")
                {
                    btnOrders.Visible = true;
                }

                Session["SortDirection"] = null;
                Session["SortExpression"] = null;
                DataTable dtcurrency = accountdb.GeneralQueryTable("SELECT CurrCode FROM Currency ORDER BY CurrCode");
                ddBill.DataSource = dtcurrency;
                ddBill.DataBind();
                DataTable dtBank = accountdb.GeneralQueryTable("SELECT BankAcct.ReceiptsControl, BankAcct.ReceiptsControl + ' - ' + nominal.NL_Description AS Detail, BankAcct.CurrCode FROM nominal RIGHT OUTER JOIN BankAcct ON nominal.NL_Code = BankAcct.ReceiptsControl ORDER BY BankAcct.Dorder");
                ddBank.DataSource = dtBank;
                ddBank.DataBind();
                if (dtBank.Rows.Count <= 0)
                {
                    ddBank.Items.Add("No Valid Bank");
                    btnGet.Enabled = false;
                    btnGet.ToolTip = "No Valid Bank to pay to.";
                    btnPay.Enabled = false;
                    btnPay.ToolTip = "No Valid Bank to pay to.";
                    bnCalculate.Enabled = false;
                    bnCalculate.ToolTip = "No Valid Bank to pay to.";
                }

                if (Request.QueryString["code"] != null)
                { txtSearchKey.Text = Request.QueryString["code"].ToString(); }
                else if (Session["SelectedSupplier"] != null)
                {
                    txtSearchKey.Text = Session["SelectedSupplier"].ToString();
                }
                else
                {
                    txtSearchKey.Text = "";
                }
                txtSearchKey_TextChanged(sender, e);
                if (cmbSupplier.DataSource == null)
                {
                    DataTable dtcontacts = accountdb.GeneralQueryTable("Select Top 50 PL_Code,RTRIM(LTRIM(ISNULL(Business_Name,PL_Code)))  + ' - ' + PL_Code + '' as Business_Name from Contacts where ISNULL(PL_Code,'') <>'' order by RTRIM(LTRIM(Business_Name))");

                    cmbSupplier.DataSource = dtcontacts;
                    cmbSupplier.DataBind();
                }
                if (cmbSupplier.DataSource == null)
                {
                    EmptyData.Attributes.Add("class", "dash-header");
                    Data.Attributes.Add("class", "hidden");
                }
                else
                {
                    //cmbSupplier.DataSource = dtcontacts;
                    //cmbSupplier.DataBind();
                    if (Request.QueryString["code"] != null)
                    {
                        DataTable dtCustomer = accountdb.GeneralQueryTable("Select Top 1 PL_Code,RTRIM(LTRIM(ISNULL(Business_Name,PL_Code))) as Business_Name  From Contacts  where PL_Code='" + Request.QueryString["code"].ToString() + "'");
                        if (dtCustomer.Rows.Count > 0)
                        {
                            cmbSupplier.SelectedValue = dtCustomer.Rows[0]["PL_Code"].ToString();
                        }

                    }
                    else
                    {
                        if (Session["SelectedSupplier"] == null)
                        {
                            String DefaultSupplier = accountdb.GeneralQuery("Select Top 1 PL_Code from Contacts where ISNULL(PL_Code,'') <>'' order by PL_Code ");
                            cmbSupplier.SelectedValue = DefaultSupplier;
                        }
                        else
                        {
                            String DefaultSupplier = "";

                            DefaultSupplier = accountdb.GeneralQuery("Select Top 1 isnull(PL_Code,'') as PL_Code from Contacts where PL_Code='" + Session["SelectedSupplier"].ToString() + "'");
                            if (DefaultSupplier.Trim() != "")
                            {
                                cmbSupplier.SelectedValue = DefaultSupplier;
                            }
                            else
                            {
                                DefaultSupplier = accountdb.GeneralQuery("Select Top 1 PL_Code from Contacts where ISNULL(PL_Code,'') <>''  order by PL_Code ");
                                cmbSupplier.SelectedValue = DefaultSupplier;
                            }

                        }
                    }

                    DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                    DateTime dto = DateTime.Today;
                    //tbDateFrom.Text = dFrom.ToString(Session["DateFormat"].ToString());
                    //tbDateTo.Text = dto.ToString(Session["DateFormat"].ToString());
                    tbDateFrom.Text = dFrom.ToString("dd/MM/yyyy");
                    tbDateTo.Text = dto.ToString("dd/MM/yyyy");
                    txtPayDate.Text = tbDateTo.Text;
                    Session["SelectedSupplier"] = cmbSupplier.SelectedValue;
                    if (Request.QueryString["currcode"] != null)
                    {
                        strSessionCurr = Request.QueryString["currcode"].ToString();
                    }
                    else
                    {

                        string BaseCurr = accountdb.DefaultCurr(companyData.CO_Ctry);// sv.SVGet(Session.SessionID, "Curr");
                        string strQ = "SELECT ISNULL(CN_Currency,'') As Curr, Discount,Cn_Salesman,CN_User1 FROM Contacts ";
                        strQ += "WHERE PL_Code = '" + cmbSupplier.SelectedValue + "'";
                        DataTable dtcontacts = accountdb.GeneralQueryTable(strQ);
                        if (dtcontacts.Rows.Count > 0)
                        {
                            BaseCurr = dtcontacts.Rows[0]["Curr"].ToString();
                        }
                        strSessionCurr = BaseCurr;
                    }
                    if (String.IsNullOrEmpty(strSessionCurr))
                    {
                        strSessionCurr = accountdb.DefaultCurr(companyData.CO_Ctry);
                        if (String.IsNullOrEmpty(strSessionCurr))
                        {
                            strSessionCurr = "£";
                        }
                    }
                    try
                    {
                        ddBill.SelectedValue = strSessionCurr;
                    }
                    catch
                    {
                        //ignore error 
                    }
                    SqlParameter[] sqlParamquerydetail = { new SqlParameter("@Bill", ddBill.SelectedValue), new SqlParameter("@supplier", cmbSupplier.SelectedValue), new SqlParameter("@pl_control", companyData.CO_PL_Control), new SqlParameter("@vl_control", companyData.CO_VAT_Control) };

                    String strToPay = "SELECT " +
                        "Isnull((SELECT Top 1 NLT_Date as Dfrom  FROM nltran  WHERE (NL_Code = @pl_control) AND (NLT_Ref = @supplier) AND (NLT_Source <> 'PO') AND (ISNULL(NLT_CurrCode, '£') LIKE @Bill + '%') " +
                        " GROUP BY NLT_Date,NLT_InvNo HAVING (ROUND(SUM(NLT_Net) - SUM(ISNULL(NLT_Paid, 0)), 2) <> 0) ORDER BY NLT_Date desc) , NLT_Date) as Dto, " +
                        "Isnull(( SELECT Top 1 NLT_Date as Dfrom  FROM nltran  WHERE (NL_Code = @pl_control) AND (NLT_Ref = @supplier) AND (NLT_Source <> 'PO') AND (ISNULL(NLT_CurrCode, '£') LIKE @Bill + '%') " +
                        " GROUP BY NLT_Date,NLT_InvNo HAVING (ROUND(SUM(NLT_Net) - SUM(ISNULL(NLT_Paid, 0)), 2) <> 0) ORDER BY NLT_Date asc),NLT_Date) as Dfrom, " +
                                " NLT_Date, NLT_Source, " +
                                " case isnull(NLT_TheirRef,'') when '' then case isnull(NLT_Payref,'') when '' then '' else NLT_Payref end else case isnull(NLT_Payref,'') when '' then NLT_TheirRef else NLT_TheirRef + '_' + NLT_Payref end end as NLT_TheirRef " +
                                ", ISNULL(NLT_InvNo, 'No ref') AS NLT_InvNo, SUM(NLT_CurrNet) AS Amount, " +
                                " SUM(ISNULL(NLT_CurrPaid, 0)) AS Paid, SUM(ISNULL(NLT_ToPayCurr, 0)) AS ToPay, SUM(NLT_Net - ISNULL(NLT_Paid, 0)) AS GBP," +
                                " NLT_Ref, NLT_CurrCode, NLT_DueDate,NLT_DeliverDate, (SELECT TOP (1) SettDisc FROM NLTranAdd " +
                                " WHERE (InvNo = ISNULL(nltran.NLT_InvNo, 'No ref')) AND (InvType = 'PI')) AS Disc,NLT_Payref FROM nltran " +
                                " WHERE (isnull(Nlt_Paidref,'') not Like '%OnHold%')  and (NL_Code = @pl_control) AND (NLT_Ref = @supplier) AND (NLT_Source <> 'PO') AND (ISNULL(NLT_CurrCode, '£') LIKE @Bill + '%') " +
                                // " AND (nltran.NLT_Date >=  Cast('" + dFrom.ToString("yyyyMMdd") + "' as Datetime)) AND (nltran.NLT_Date <= Cast('" + dto.ToString("yyyyMMdd") + "' as Datetime)) " +
                                " GROUP BY NLT_Date, NLT_Source, NLT_TheirRef, ISNULL(NLT_InvNo, 'No ref'), NLT_Ref, NLT_CurrCode, NLT_DueDate,NLT_DeliverDate,NLT_Payref " +
                                " HAVING (ROUND(SUM(NLT_Net) - SUM(ISNULL(NLT_Paid, 0)), 2) <> 0)" +
                                " ORDER BY NLT_Date";
                    DataTable dtToPay = accountdb.GeneralQueryTableWithParams(strToPay, sqlParamquerydetail);
                    if (dtToPay != null && dtToPay.Rows.Count > 0)
                    {
                        double amount = Convert.ToDouble(dtToPay.Compute("SUM(Amount)", string.Empty));
                        double paid = Convert.ToDouble(dtToPay.Compute("SUM(Paid)", string.Empty));
                        double curTotalPrice = Convert.ToDouble(dtToPay.Compute("SUM(GBP)", string.Empty));

                        //Decimal CurTotalPrice = dtToPay.Compute("sum([" + TotalItem + "])", "");

                        TxtBalance.Text = Math.Round(amount - paid, 2).ToString("N");
                        TxtCurBalance.Text = Math.Round(curTotalPrice, 2).ToString("N");

                        gvInvoices.DataSource = dtToPay;
                        gvInvoices.DataBind();
                        tLevel1.Attributes.Add("class", "");
                        tLevel2.Attributes.Add("class", "");
                        tcalc.Attributes.Add("class", "");
                        lblCurSel.Text = ddBill.SelectedValue;
                        lblDefCurr.Text = accountdb.DefaultCurr(companyData.CO_Ctry);
                        if (lblDefCurr.Text == "")
                        { lblDefCurr.Text = "£"; }
                        //tbDateFrom.Text = Convert.ToDateTime(dtToPay.Rows[0]["Dfrom"].ToString()).ToString(Session["DateFormat"].ToString());
                        //tbDateTo.Text = Convert.ToDateTime(dtToPay.Rows[0]["Dto"].ToString()).ToString(Session["DateFormat"].ToString());
                        tbDateFrom.Text = Convert.ToDateTime(dtToPay.Rows[0]["Dfrom"].ToString()).ToString("dd/MM/yyyy");
                        tbDateTo.Text = Convert.ToDateTime(dtToPay.Rows[0]["Dto"].ToString()).ToString("dd/MM/yyyy");

                    }
                    else
                    {
                        gvInvoices.DataSource = dtToPay;
                        gvInvoices.DataBind();
                        //tLevel1.Attributes.Add("class", "hidden");
                        tLevel2.Attributes.Add("class", "hidden");
                        tcalc.Attributes.Add("class", "hidden");
                    }
                }
                btnGet_Click(sender, e);
            }
            StatementUpdate();
            pnlPayment.Visible = true;
            hlPaymentRecd.NavigateUrl = "~/Bank/bankAcct.aspx";
        }

        protected void BindPageCurrency()
        {
            DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            DateTime dto = DateTime.Today;
            DateTime.TryParse(tbDateFrom.Text, out dFrom);
            DateTime.TryParse(tbDateTo.Text, out dto);
            Session["SelectedSupplier"] = cmbSupplier.SelectedValue;
            SqlParameter[] sqlParamquerydetail = { new SqlParameter("@Bill", ddBill.SelectedValue), new SqlParameter("@supplier", cmbSupplier.SelectedValue), new SqlParameter("@pl_control", companyData.CO_PL_Control), new SqlParameter("@vl_control", companyData.CO_VAT_Control) };

            String strToPay = "SELECT " +
                        "isnull((SELECT Top 1 NLT_Date as Dfrom  FROM nltran  WHERE (NL_Code = @pl_control) AND (NLT_Ref = @supplier) AND (NLT_Source <> 'PO') AND (ISNULL(NLT_CurrCode, '£') LIKE @Bill '%') " +
                        " GROUP BY NLT_Date,NLT_InvNo HAVING (ROUND(SUM(NLT_Net) - SUM(ISNULL(NLT_Paid, 0)), 2) <> 0) ORDER BY NLT_Date desc),NLT_Date) as Dto, " +
                        "isnull(( SELECT Top 1 NLT_Date as Dfrom  FROM nltran  WHERE (NL_Code = @pl_control) AND (NLT_Ref = @supplier) AND (NLT_Source <> 'PO') AND (ISNULL(NLT_CurrCode, '£') LIKE @Bill '%') " +
                        " GROUP BY NLT_Date,NLT_InvNo HAVING (ROUND(SUM(NLT_Net) - SUM(ISNULL(NLT_Paid, 0)), 2) <> 0) ORDER BY NLT_Date asc),NLT_Date) as Dfrom, " +
                                " NLT_Date, NLT_Source, " +
                                " case isnull(NLT_TheirRef,'') when '' then case isnull(NLT_Payref,'') when '' then '' else NLT_Payref end else case isnull(NLT_Payref,'') when '' then NLT_TheirRef else NLT_TheirRef + '_' + NLT_Payref end end as NLT_TheirRef" +
                                ", ISNULL(NLT_InvNo, 'No ref') AS NLT_InvNo, SUM(NLT_CurrNet) AS Amount, " +
                                " SUM(ISNULL(NLT_CurrPaid, 0)) AS Paid, SUM(ISNULL(NLT_ToPayCurr, 0)) AS ToPay, SUM(NLT_Net - ISNULL(NLT_Paid, 0)) AS GBP," +
                                " NLT_Ref, NLT_CurrCode, NLT_DueDate,NLT_DeliverDate, (SELECT TOP (1) SettDisc FROM NLTranAdd " +
                                " WHERE (InvNo = ISNULL(nltran.NLT_InvNo, 'No ref')) AND (InvType = 'PI')) AS Disc,NLT_Payref FROM nltran " +
                                " WHERE (isnull(Nlt_Paidref,'') not Like '%OnHold%')  and (NL_Code = @pl_control) AND (NLT_Ref = @supplier) AND (NLT_Source <> 'PO') AND (ISNULL(NLT_CurrCode, '£') LIKE @Bill '%') " +
                                " GROUP BY NLT_Date, NLT_Source, NLT_TheirRef, ISNULL(NLT_InvNo, 'No ref'), NLT_Ref, NLT_CurrCode, NLT_DueDate,NLT_DeliverDate,NLT_Payref " +
                                " HAVING (ROUND(SUM(NLT_Net) - SUM(ISNULL(NLT_Paid, 0)), 2) <> 0)" +
                                " ORDER BY NLT_Date";
            DataTable dtToPay = accountdb.GeneralQueryTableWithParams(strToPay, sqlParamquerydetail);
            if (dtToPay != null && dtToPay.Rows.Count > 0)
            {
                double amount = Convert.ToDouble(dtToPay.Compute("SUM(Amount)", string.Empty));
                double paid = Convert.ToDouble(dtToPay.Compute("SUM(Paid)", string.Empty));
                double curTotalPrice = Convert.ToDouble(dtToPay.Compute("SUM(GBP)", string.Empty));

                //Decimal CurTotalPrice = dtToPay.Compute("sum([" + TotalItem + "])", "");

                TxtBalance.Text = Math.Round(amount - paid, 2).ToString("N");
                TxtCurBalance.Text = Math.Round(curTotalPrice, 2).ToString("N");

                gvInvoices.DataSource = dtToPay;
                gvInvoices.DataBind();
                tLevel1.Attributes.Add("class", "");
                tLevel2.Attributes.Add("class", "");
                tcalc.Attributes.Add("class", "");
                lblCurSel.Text = ddBill.SelectedValue;
                lblDefCurr.Text = accountdb.DefaultCurr(companyData.CO_Ctry);
                if (lblDefCurr.Text == "")
                { lblDefCurr.Text = "£"; }

                //tbDateFrom.Text = Convert.ToDateTime(dtToPay.Rows[0]["Dfrom"].ToString()).ToString(Session["DateFormat"].ToString());
                //tbDateTo.Text = Convert.ToDateTime(dtToPay.Rows[0]["Dto"].ToString()).ToString(Session["DateFormat"].ToString());
                tbDateFrom.Text = Convert.ToDateTime(dtToPay.Rows[0]["Dfrom"].ToString()).ToString("dd/MM/yyyy");
                tbDateTo.Text = Convert.ToDateTime(dtToPay.Rows[0]["Dto"].ToString()).ToString("dd/MM/yyyy");

            }
            else
            {
                gvInvoices.DataSource = dtToPay;
                gvInvoices.DataBind();
                // tLevel1.Attributes.Add("class", "hidden");
                tLevel2.Attributes.Add("class", "hidden");
                tcalc.Attributes.Add("class", "hidden");
            }
        }
        protected DataTable BindPage()
        {
            StatementUpdate();

            DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            DateTime dto = DateTime.Today;
            if (tbDateFrom.Text.Trim() != "")
            {
                DateTime.TryParse(tbDateFrom.Text, out dFrom);
            }
            else
            {
                tbDateFrom.Text = dFrom.ToShortDateString();
            }
            if (tbDateTo.Text.Trim() != "")
            {
                DateTime.TryParse(tbDateTo.Text, out dto);
            }
            else
            {
                tbDateTo.Text = dto.ToShortDateString();
            }
            if (Session["suppSelected"] != null)
            {
                if (Session["suppSelected"].ToString() == "true")
                {
                    tbDateFrom.Text = "01 Jan 2000";
                    DateTime.TryParse(tbDateFrom.Text, out dFrom);
                    dto = DateTime.Today;
                    tbDateTo.Text = dto.ToShortDateString();
                }
            }

            Session["SelectedSupplier"] = cmbSupplier.SelectedValue;

            SqlParameter[] sqlParamquerydetail = { new SqlParameter("@Bill", ddBill.SelectedValue), new SqlParameter("@supplier", cmbSupplier.SelectedValue), new SqlParameter("@pl_control", companyData.CO_PL_Control), new SqlParameter("@datefrom", dFrom.ToString("yyyyMMdd")), new SqlParameter("@dTo", dto.ToString("yyyyMMdd")) };

            String strToPay = "SELECT NLT_Date, NLT_Source, " +
                " case isnull(NLT_TheirRef,'') when '' then case isnull(NLT_Payref,'') when '' then '' else NLT_Payref end else case isnull(NLT_Payref,'') when '' then NLT_TheirRef else NLT_TheirRef + '_' + NLT_Payref end end as NLT_TheirRef" +
                ", ISNULL(NLT_InvNo, 'No ref') AS NLT_InvNo, SUM(NLT_CurrNet) AS Amount, " +
                        " SUM(ISNULL(NLT_CurrPaid, 0)) AS Paid, SUM(ISNULL(NLT_ToPayCurr, 0)) AS ToPay, SUM(NLT_Net - ISNULL(NLT_Paid, 0)) AS GBP," +
                        " NLT_Ref, NLT_CurrCode, NLT_DueDate,NLT_DeliverDate, (SELECT TOP (1) SettDisc FROM NLTranAdd " +
                        " WHERE (InvNo = ISNULL(nltran.NLT_InvNo, 'No ref')) AND (InvType = 'PI')) AS Disc,NLT_Payref FROM nltran " +
                        " WHERE (isnull(Nlt_Paidref,'') not Like '%OnHold%')  and (NL_Code = @pl_control) AND (NLT_Ref = @supplier) AND (NLT_Source <> 'PO') AND (ISNULL(NLT_CurrCode, '£') LIKE @Bill + '%') " +
                        " AND (nltran.NLT_Date >=  Cast(@datefrom as Datetime)) AND (nltran.NLT_Date <= Cast(@dTo as Datetime)) " +
                        " GROUP BY NLT_Date, NLT_Source, NLT_TheirRef, ISNULL(NLT_InvNo, 'No ref'), NLT_Ref, NLT_CurrCode, NLT_DueDate,NLT_DeliverDate,NLT_Payref " +
                        " HAVING (ROUND(SUM(NLT_Net) - SUM(ISNULL(NLT_Paid, 0)), 2) <> 0)" +
                        " ORDER BY NLT_Date";
            DataTable dtToPay = accountdb.GeneralQueryTableWithParams(strToPay, sqlParamquerydetail);
            if (dtToPay != null && dtToPay.Rows.Count > 0)
            {
                double amount = Convert.ToDouble(dtToPay.Compute("SUM(Amount)", string.Empty));
                double paid = Convert.ToDouble(dtToPay.Compute("SUM(Paid)", string.Empty));
                double curTotalPrice = Convert.ToDouble(dtToPay.Compute("SUM(GBP)", string.Empty));

                //Decimal CurTotalPrice = dtToPay.Compute("sum([" + TotalItem + "])", "");

                TxtBalance.Text = Math.Round(amount - paid, 2).ToString("N");
                TxtCurBalance.Text = Math.Round(curTotalPrice, 2).ToString("N");

                gvInvoices.DataSource = dtToPay;
                gvInvoices.DataBind();
                tLevel1.Attributes.Add("class", "");
                tLevel2.Attributes.Add("class", "");
                tcalc.Attributes.Add("class", "");
                lblCurSel.Text = ddBill.SelectedValue;
                lblDefCurr.Text = accountdb.DefaultCurr(companyData.CO_Ctry);
                if (lblDefCurr.Text == "")
                { lblDefCurr.Text = "£"; }
            }
            else
            {
                gvInvoices.DataSource = dtToPay;
                gvInvoices.DataBind();
                // tLevel1.Attributes.Add("class", "hidden");
                tLevel2.Attributes.Add("class", "hidden");
                tcalc.Attributes.Add("class", "hidden");
            }
            return dtToPay;
        }

        protected void btnAddSupplier_Click(object sender, EventArgs e)
        {
            Response.Redirect("suppmaster.aspx", false);
        }

        protected void gvInvoices_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if(e.Row.RowType == DataControlRowType.Header)
            {
                //e.Row.Cells[9].Text = accountdb.DefaultCurr(companyData.CO_Ctry);

                gvInvoices.Columns[9].HeaderText = accountdb.DefaultCurr(companyData.CO_Ctry);
                if (gvInvoices.Columns[9].HeaderText == "")
                { gvInvoices.Columns[9].HeaderText = "£"; }
            }
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                double dbl6 = 0;
                double dbl7 = 0;
                double dbl8 = 0;
                double dbl9 = 0;
                double.TryParse(e.Row.Cells[6].Text, out dbl6);
                double.TryParse(e.Row.Cells[7].Text, out dbl7);
                double.TryParse(e.Row.Cells[8].Text, out dbl8);
                double.TryParse(e.Row.Cells[9].Text, out dbl9);
                double dblValue = dbl6 - dbl7;
                e.Row.Cells[8].Text = Math.Round(dblValue, 2).ToString("N");
                if (ddBill.SelectedValue == "%")
                {
                    dblValue = dbl9;
                }
                dblTotal[8] += dbl8;
                if (ddBill.SelectedValue != strSessionCurr)
                {
                    dblTotal[9] += dbl9;
                }

                double topaycurr = 0;
                double.TryParse(((TextBox)e.Row.FindControl("tbToAllocate")).Text, out topaycurr);
                if (topaycurr != 0)
                {
                    ((CheckBox)e.Row.FindControl("cb1")).Checked = true;
                }

                if (e.Row.Cells[2].Text == "PP")
                {

                    string strQ = "SELECT TOP 1 NLT_Audit FROM NLTran ";
                    strQ += "WHERE (NLT_Source = 'PP')  ";

                    string strInvNo = e.Row.Cells[4].Text;


                    if (!String.IsNullOrEmpty(strInvNo) && strInvNo != "No ref")
                    {
                        strQ += "AND NLT_InvNo = '" + strInvNo + "' ";
                    }
                    else
                    {
                        if (AccountsDBMethods.IsDate(e.Row.Cells[0].Text))
                        {
                            strQ += "AND NLT_Date = Cast('" + Convert.ToDateTime(e.Row.Cells[0].Text).ToString("yyyyMMdd") + "' as Datetime)  ";
                            strQ += "AND ISNULL(NLT_InvNo,'') = '' ";
                        }
                    }
                    string strAudit = accountdb.GeneralQuery(strQ);


                    String prefix = "";
                    if (strInvNo.Length >= 4)
                        prefix = strInvNo.Substring(0, 4);
                    switch (prefix)
                    {
                        case "CTRA":
                            ((HyperLink)e.Row.Cells[13].Controls[0]).NavigateUrl = "~/Supplier/ContraTransaction.aspx?InvNo=" + e.Row.Cells[4].Text + "&Source=P&Code=" + cmbSupplier.SelectedValue + "&Audit=" + strAudit;
                            break;
                        default:
                            ((HyperLink)e.Row.Cells[13].Controls[0]).NavigateUrl = "~/Supplier/PurchaseReceipt.aspx?InvNo=" + e.Row.Cells[4].Text + "&Source=PP&Code=" + cmbSupplier.SelectedValue + "&Audit=" + strAudit;
                            break;
                    }
                }

            }
            if (e.Row.RowType == DataControlRowType.Footer)
            {
                e.Row.Cells[8].Text = Math.Round(dblTotal[8], 2).ToString("N");
                if (ddBill.SelectedValue != strSessionCurr)
                {
                    e.Row.Cells[9].Text = Math.Round(dblTotal[9], 2).ToString("N");
                }
            }
        }

        protected void cmbSupplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["SelectedSupplier"] = cmbSupplier.SelectedValue;
            lblTranValue.Text = "0.00";
            lblnetCurr.Text = "";
            lblbankcurr.Text = "";
            lblCurTotals.Text = "0.00";
            lblBankValue.Text = "0.00";
            lblTotals.Text = "0.00";
            lblexchrate.Text = "1";
            txtDiscount.Text = "0.00";
            txtexchVar.Text = "0.00";
            Session["suppSelected"] = "true";
            BindPage();
            Session["suppSelected"] = "false";
        }

        protected void gvInvoices_PreRender(object sender, EventArgs e)
        {
            if (Session["CompanyData"] == null)
            {
                companyData = accountdb.AccCompanyData();
                Session["CompanyData"] = companyData;
            }
            else
            {
                companyData = (Business_Layer.Company.Company)Session["CompanyData"];
            }

            Array.Clear(dblTotal, 0, 10);
            gvInvoices.Columns[9].HeaderText = accountdb.DefaultCurr(companyData.CO_Ctry);
            if (gvInvoices.Columns[9].HeaderText == "")
            { gvInvoices.Columns[9].HeaderText = "£"; }

            gvInvoices.Columns[11].FooterStyle.CssClass = "hidden";
            gvInvoices.Columns[11].HeaderStyle.CssClass = "hidden";
            gvInvoices.Columns[11].ItemStyle.CssClass = "hidden";

        }

        protected void bnCalculate_Click(object sender, EventArgs e)
        {
            btnGet_Click(sender, e);
            Compute();
        }

        protected void Compute(double bankValue = 0)
        {
            if (Session["CompanyData"] == null)
            {
                companyData = accountdb.AccCompanyData();
                Session["CompanyData"] = companyData;
            }
            else
            {
                companyData = (Business_Layer.Company.Company)Session["CompanyData"];
            }


            DateTime datePay = DateTime.Today;
            if (AccountsDBMethods.IsDate(txtPayDate.Text))
            {
                datePay = Convert.ToDateTime(txtPayDate.Text);
            }

            string defaultCurr = accountdb.DefaultCurr(companyData.CO_Ctry);
            if (string.IsNullOrEmpty(defaultCurr))
            { defaultCurr = "£"; }
            lblnetCurr.Text = defaultCurr;
            lblbankcurr.Text = accountdb.GeneralQuery("SELECT isnull(BankAcct.CurrCode,'" + defaultCurr + "') FROM nominal RIGHT OUTER JOIN BankAcct ON nominal.NL_Code = BankAcct.ReceiptsControl where ReceiptsControl='" + ddBank.SelectedValue + "'");

            double dblOnAcct = 0; // cf.CNo(tbOnAcct.Text);
            double dblCurrOnAcct = 0; // cf.CNo(tbCurrOnAcct.Text);

            double dblRate = 0;
            if (accountdb.GVGet("ECBRate") != "True")
            {
                dblRate = Math.Round(accountdb.GetExchRate(lblbankcurr.Text.Trim(), datePay, "", defaultCurr), 4);
            }
            else
            {
                dblRate = Math.Round(accountdb.GetECBRates(lblbankcurr.Text.Trim(), datePay, "", defaultCurr), 4);
            }

            if (dblRate <= 0) { dblRate = 1; lblexchrate.Text = "1"; }
            lblexchrate.Text = dblRate.ToString();

            double dblAlloc = 0; double.TryParse(lblTotals.Text, out dblAlloc);
            double dblCurrAlloc = 0; dblCurrAlloc = Math.Round((dblRate * dblAlloc), 2);
            double dblrealCurrValue = 0; double.TryParse(lblCurTotals.Text, out dblrealCurrValue);
            if (lblnetCurr.Text.Trim() != lblbankcurr.Text.Trim())
            {
                double realdiff = dblrealCurrValue - dblCurrAlloc;
                if (realdiff < 0.5 && realdiff > 0)
                {
                    dblCurrAlloc = dblrealCurrValue;
                }
                if (lblCurSel.Text.Trim() == lblbankcurr.Text.Trim())
                {
                    dblCurrAlloc = dblrealCurrValue;
                }
            }
            double dblEV = 0;
            dblEV = (dblCurrAlloc / dblRate) - dblAlloc;
            double dblExchVar = 0.00;
            Double.TryParse(txtexchVar.Text, out dblExchVar);
            if (dblExchVar != 0 || dblEV != 0)
            {
                txtexchVar.Text = Math.Round(dblEV, 2).ToString();
                dblExchVar = dblEV;
            }

            double dblDisc = 0; double.TryParse(txtDiscount.Text, out dblDisc);
            double dblCurrDisc = 0; dblCurrDisc = Math.Round((dblRate * dblDisc), 2);

            double dblCIS = 0;// double.TryParse(txtCIS.Text, out dblCIS);
            double dblCurrCIS = 0; dblCurrCIS = Math.Round((dblRate * dblCIS), 2);

            double debitnote = 0; //double.TryParse(txtDebitNote.Text, out debitnote);
            double dblCurrdebitnote = 0; dblCurrdebitnote = Math.Round((dblRate * debitnote), 2);
            if (dblDisc < 0)
            {
                dblDisc = -dblDisc;
            }
            if (dblCurrDisc < 0)
            {
                dblCurrDisc = -dblCurrDisc;
            }
            double dblTotal = dblAlloc + dblOnAcct - (dblCIS + dblDisc + debitnote);
            double dblCurrTotal = dblCurrAlloc + dblCurrOnAcct - (dblCurrCIS + dblCurrDisc + dblCurrdebitnote);

            dblTotal = dblTotal + dblEV;

            lblTranValue.Text = Math.Round(dblTotal, 2).ToString();
            if (bankValue != 0 || isBankValueEdit)
            {

                //txtexchVar.Text = Math.Round(dblCurrTotal - bankValue,2).ToString();
                double exchR = AccountsDBMethods.CNo(lblexchrate.Text) == 0 ? 1 : AccountsDBMethods.CNo(lblexchrate.Text);
                txtexchVar.Text = Math.Round((bankValue / exchR) - dblTotal, 2).ToString();
                lblBankValue.Text = Math.Round(bankValue, 2).ToString();

                //IF recalcualte echange rate is active
                //double exchR = Math.Round(bankValue / dblTotal, 2);
                //lblexchrate.Text = exchR.ToString();
                //txtexchVar.Text = Math.Round(dblTotal - (bankValue / exchR), 2).ToString();

                isBankValueEdit = false;

            }
            else
            {
                lblBankValue.Text = Math.Round(dblCurrTotal, 2).ToString();
            }
        }

        protected void ddBank_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnGet_Click(sender, e);
            Compute();
        }

        protected void txtPayDate_TextChanged(object sender, EventArgs e)
        {
            if (!AccountsDBMethods.IsDate(txtPayDate.Text))
            {
                //txtPayDate.Text = DateTime.Today.ToString(Session["DateFormat"].ToString());
                txtPayDate.Text = DateTime.Today.ToString("dd/MM/yyyy");
            }
            btnGet_Click(sender, e);
            Compute();
        }
        protected void tbDateFrom_TextChanged(object sender, EventArgs e)
        {
            DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            if (!AccountsDBMethods.IsDate(txtPayDate.Text))
            {
                //tbDateFrom.Text = dFrom.ToString(Session["DateFormat"].ToString());
                tbDateFrom.Text = dFrom.ToString("dd/MM/yyyy");
            }
            DateTime.TryParse(tbDateFrom.Text, out dFrom);
            BindPage();
        }
        protected void tbDateTo_TextChanged(object sender, EventArgs e)
        {
            DateTime dto = DateTime.Today;
            if (!AccountsDBMethods.IsDate(txtPayDate.Text))
            {
                //tbDateTo.Text = DateTime.Today.ToString(Session["DateFormat"].ToString());
                tbDateTo.Text = DateTime.Today.ToString("dd/MM/yyyy");
            }
            DateTime.TryParse(tbDateTo.Text, out dto);
            BindPage();
        }

        protected void btnGet_Click(object sender, EventArgs e)
        {
            String strUpdateQuery = "";

            double dbl11Summ = 0;
            foreach (GridViewRow r in gvInvoices.Rows)
            {
                double topay = 0;
                double exchrate = 1;
                double topaycurr = 0;

                double dbl6 = 0;
                double dbl7 = 0;
                double dbl8 = 0;
                double dbl9 = 0;
                double dbl11 = 0;
                double.TryParse(r.Cells[6].Text, out dbl6);
                double.TryParse(r.Cells[7].Text, out dbl7);
                double.TryParse(r.Cells[8].Text, out dbl8);
                double.TryParse(r.Cells[9].Text, out dbl9);
                double.TryParse(r.Cells[11].Text, out dbl11);

                dbl11Summ += dbl11;
                exchrate = dbl8 / dbl9;
                double.TryParse(((TextBox)r.FindControl("tbToAllocate")).Text, out topaycurr);
                if (topaycurr != 0)
                {
                    topay = topaycurr / exchrate;
                }
                else
                {
                    topay = 0;
                    dbl11Summ -= dbl11;
                }
                String topRecord = accountdb.GeneralQuery("select top 1 NLT_Key from nltran  Where nlt_invno ='" + r.Cells[4].Text + "' and nlt_source='" + r.Cells[2].Text + "' and nlt_ref='" + cmbSupplier.SelectedValue + "' and nl_code='" + companyData.CO_PL_Control + "' Order by NLT_Key asc");
                int topRec = 0;
                int.TryParse(topRecord, out topRec);
                strUpdateQuery += " Update nltran set nlt_topay='" + Math.Round(topay, 2).ToString() + "', nlt_topaycurr='" + Math.Round(topaycurr, 2).ToString() + "' Where nlt_key = " + topRec;

            }
            int retValue = accountdb.CommandWithRollBack(strUpdateQuery);
            if (retValue > 0)
            {
                DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                DateTime dto = DateTime.Today;
                DateTime.TryParse(tbDateFrom.Text, out dFrom);
                DateTime.TryParse(tbDateTo.Text, out dto);
                string strQ = "SELECT SUM(-(ISNULL(NLT_ToPayCurr,0))) As ToPayCurr, SUM(-(ISNULL(NLT_ToPay,0))) As ToPay ";
                strQ += "FROM NLTran ";
                strQ += "WHERE NLT_Ref = '" + cmbSupplier.SelectedValue + "' ";
                strQ += "AND NL_Code = '" + companyData.CO_PL_Control + "' ";
                strQ += "AND isnull(NLT_ToPay,0) <> 0 AND isnull(NLT_ToPayCurr,0) <> 0";
                strQ += "AND (NLT_Date >=  Cast('" + dFrom.ToString("yyyyMMdd") + "' as Datetime)) AND (NLT_Date <= Cast('" + dto.ToString("yyyyMMdd") + "' as Datetime)) ";

                DataTable dtTotal = accountdb.GeneralQueryTable(strQ);
                double dblToPayCurr = 0;
                double dblToPay = 0;
                if (dtTotal.Rows.Count > 0)
                {
                    double.TryParse(dtTotal.Rows[0]["ToPayCurr"].ToString(), out dblToPayCurr);
                    double.TryParse(dtTotal.Rows[0]["ToPay"].ToString(), out dblToPay);
                }

                lblTotals.Text = Math.Round(dblToPay, 2).ToString();
                lblCurTotals.Text = Math.Round(dblToPayCurr, 2).ToString();
                double existDisc = 0;
                double.TryParse(txtDiscount.Text, out existDisc);
                if (existDisc == 0)
                {
                    txtDiscount.Text = Math.Round(dbl11Summ, 2).ToString();
                }
                else
                {
                    txtDiscount.Text = Math.Round((-existDisc + dbl11Summ) + dbl11Summ, 2).ToString();
                }
                if (dblToPay == 0 && dblToPayCurr == 0)
                {
                    txtDiscount.Enabled = false;
                    txtDiscount.Text = "0";
                }
                else
                {
                    txtDiscount.Enabled = true;
                }
            }
        }

        protected void ddBill_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblTranValue.Text = "0.00";
            lblnetCurr.Text = "";
            lblbankcurr.Text = "";
            lblCurSel.Text = "";
            lblBankValue.Text = "0.00";
            lblDefCurr.Text = "";
            lblexchrate.Text = "1";
            txtDiscount.Text = "0.00";
            txtexchVar.Text = "0.00";
            BindPage();
        }

        protected void btnPayAll_Click(object sender, EventArgs e)
        {
            bnCalculate_Click(sender, e);

            string strUpdateQuery = "";

            double dbl11Summ = 0;
            foreach (GridViewRow r in gvInvoices.Rows)
            {
                ((CheckBox)r.FindControl("cb1")).Checked = true;
                double topay = 0;
                double exchrate = 1;
                double topaycurr = 0;

                double dbl6 = 0;
                double dbl7 = 0;
                double dbl8 = 0;
                double dbl9 = 0;
                double dbl11 = 0;
                double.TryParse(r.Cells[6].Text, out dbl6);
                double.TryParse(r.Cells[7].Text, out dbl7);
                double.TryParse(r.Cells[8].Text, out dbl8);
                double.TryParse(r.Cells[9].Text, out dbl9);
                double.TryParse(r.Cells[11].Text, out dbl11);

                dbl11Summ += dbl11;
                exchrate = dbl8 / dbl9;
                ((TextBox)r.FindControl("tbToAllocate")).Text = Math.Round(dbl8, 2).ToString();
                topaycurr = Math.Round(dbl8, 2);
                if (topaycurr != 0)
                {
                    topay = topaycurr / exchrate;
                }
                else
                {
                    topay = 0;
                    dbl11Summ -= dbl11;
                }
                String topRecord = accountdb.GeneralQuery("select top 1 NLT_Key from nltran  Where nlt_invno ='" + r.Cells[4].Text + "' and nlt_source='" + r.Cells[2].Text + "' and nlt_ref='" + cmbSupplier.SelectedValue + "' and nl_code='" + companyData.CO_PL_Control + "' Order by NLT_Key asc");
                int topRec = 0;
                int.TryParse(topRecord, out topRec);
                strUpdateQuery += " Update nltran set nlt_topay='" + Math.Round(topay, 2).ToString() + "', nlt_topaycurr='" + Math.Round(topaycurr, 2).ToString() + "' Where nlt_key = " + topRec;
            }
            int retValue = accountdb.CommandWithRollBack(strUpdateQuery);
            if (retValue > 0)
            {
                DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                DateTime dto = DateTime.Today;
                DateTime.TryParse(tbDateFrom.Text, out dFrom);
                DateTime.TryParse(tbDateTo.Text, out dto);
                //string strQ = "SELECT SUM(ISNULL(NLT_ToPayCurr,0)) As ToPayCurr, SUM(ISNULL(NLT_ToPay,0)) As ToPay ";
                string strQ = "SELECT SUM(-(ISNULL(NLT_ToPayCurr,0))) As ToPayCurr, SUM(-(ISNULL(NLT_ToPay,0))) As ToPay ";
                strQ += "FROM NLTran ";
                strQ += "WHERE NLT_Ref = '" + cmbSupplier.SelectedValue + "' ";
                strQ += "AND NL_Code = '" + companyData.CO_PL_Control + "' ";
                strQ += "AND isnull(NLT_ToPay,0) <> 0 AND isnull(NLT_ToPayCurr,0) <> 0";
                strQ += "AND (NLT_Date >=  Cast('" + dFrom.ToString("yyyyMMdd") + "' as Datetime)) AND (NLT_Date <= Cast('" + dto.ToString("yyyyMMdd") + "' as Datetime)) ";


                DataTable dtTotal = accountdb.GeneralQueryTable(strQ);
                double dblToPayCurr = 0;
                double dblToPay = 0;
                if (dtTotal.Rows.Count > 0)
                {
                    double.TryParse(dtTotal.Rows[0]["ToPayCurr"].ToString(), out dblToPayCurr);
                    double.TryParse(dtTotal.Rows[0]["ToPay"].ToString(), out dblToPay);
                }

                lblTotals.Text = Math.Round(dblToPay, 2).ToString();
                lblCurTotals.Text = Math.Round(dblToPayCurr, 2).ToString();
                double existDisc = 0;
                double.TryParse(txtDiscount.Text, out existDisc);
                if (existDisc == 0)
                {
                    txtDiscount.Text = Math.Round(dbl11Summ, 2).ToString();
                }
                else
                {
                    txtDiscount.Text = Math.Round((existDisc - dbl11Summ) + dbl11Summ, 2).ToString();
                }
                if (dblToPay == 0 && dblToPayCurr == 0)
                {
                    txtDiscount.Enabled = false;
                    txtDiscount.Text = "0";
                }
                else
                {
                    txtDiscount.Enabled = true;
                }
                bnCalculate_Click(sender, e);
            }
        }

        protected void btnUnpayAll_Click(object sender, EventArgs e)
        {
            String strUpdateQuery = "";
            double dbl11Summ = 0;
            foreach (GridViewRow r in gvInvoices.Rows)
            {
                ((CheckBox)r.FindControl("cb1")).Checked = false;
                double topay = 0;
                double topaycurr = 0;
                double dbl11 = 0;
                double.TryParse(r.Cells[11].Text, out dbl11);
                dbl11Summ += dbl11;
                ((TextBox)r.FindControl("tbToAllocate")).Text = topaycurr.ToString();
                if (topaycurr == 0)
                {
                    dbl11Summ -= dbl11;
                }

                strUpdateQuery += " Update nltran set nlt_topay='" + Math.Round(topay, 2).ToString() + "', nlt_topaycurr='" + Math.Round(topaycurr, 2).ToString() + "' Where nlt_invno ='" + r.Cells[4].Text + "' and nlt_source='" + r.Cells[2].Text + "' and nlt_ref='" + cmbSupplier.SelectedValue + "' and nl_code='" + companyData.CO_PL_Control + "'";
            }
            int retValue = accountdb.CommandWithRollBack(strUpdateQuery);
            if (retValue > 0)
            {
                DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                DateTime dto = DateTime.Today;
                DateTime.TryParse(tbDateFrom.Text, out dFrom);
                DateTime.TryParse(tbDateTo.Text, out dto);
                string strQ = "SELECT SUM(-(ISNULL(NLT_ToPayCurr,0))) As ToPayCurr, SUM(-(ISNULL(NLT_ToPay,0))) As ToPay ";
                strQ += "FROM NLTran ";
                strQ += "WHERE NLT_Ref = '" + cmbSupplier.SelectedValue + "' ";
                strQ += "AND NL_Code = '" + companyData.CO_PL_Control + "' ";
                strQ += "AND isnull(NLT_ToPay,0) <> 0 AND isnull(NLT_ToPayCurr,0) <> 0";
                strQ += "AND (NLT_Date >=  Cast('" + dFrom.ToString("yyyyMMdd") + "' as Datetime)) AND (NLT_Date <= Cast('" + dto.ToString("yyyyMMdd") + "' as Datetime)) ";

                DataTable dtTotal = accountdb.GeneralQueryTable(strQ);
                double dblToPayCurr = 0;
                double dblToPay = 0;
                if (dtTotal.Rows.Count > 0)
                {
                    double.TryParse(dtTotal.Rows[0]["ToPayCurr"].ToString(), out dblToPayCurr);
                    double.TryParse(dtTotal.Rows[0]["ToPay"].ToString(), out dblToPay);
                }

                lblTotals.Text = Math.Round(dblToPay, 2).ToString();
                lblCurTotals.Text = Math.Round(dblToPayCurr, 2).ToString();
                txtDiscount.Text = "0.00";
                double existDisc = 0;
                double.TryParse(txtDiscount.Text, out existDisc);
                if (existDisc != 0)
                {
                    txtDiscount.Text = Math.Round(-(existDisc - dbl11Summ), 2).ToString();
                }
                if (dblToPay == 0 && dblToPayCurr == 0)
                {
                    txtDiscount.Enabled = false;
                    txtDiscount.Text = "0";
                }
                else
                {
                    txtDiscount.Enabled = true;
                }
                bnCalculate_Click(sender, e);
            }
        }

        protected void gvInvoices_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInvoices.PageIndex = e.NewPageIndex;
            if (Session["SortExpression"] != null)
            {
                String sDirect = "ASC";
                if (Session["SortDirection"] != null)
                {
                    sDirect = Session["SortDirection"].ToString();
                }
                switch (sDirect)
                {
                    case "DESC":
                        sDirect = "ASC";
                        break;
                    case "ASC":
                        sDirect = "DESC";
                        break;
                }
                Session["SortDirection"] = sDirect;
                SortGrid(Session["SortExpression"].ToString());
            }
            else
            {
                BindPage();
            }
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                DateTime dto = DateTime.Today;
                DateTime.TryParse(tbDateFrom.Text, out dFrom);
                DateTime.TryParse(tbDateTo.Text, out dto);
                if (tbDateFrom.Text.Trim() != "")
                {
                    DateTime.TryParse(tbDateFrom.Text, out dFrom);
                }
                else
                {
                    tbDateFrom.Text = dFrom.ToShortDateString();
                }
                if (tbDateTo.Text.Trim() != "")
                {
                    DateTime.TryParse(tbDateTo.Text, out dto);
                }
                else
                {
                    tbDateTo.Text = dto.ToShortDateString();
                }

                if (chkCalc.Checked)
                {
                    bnCalculate_Click(sender, e);
                }

                String strUpdateQuery = "";
                String TransactionRef = DateTime.Now.ToString("yyyyMMdd HHmmss").Replace(" ", "") + "/";


                DataTable dtTopay = accountdb.GeneralQueryTable(@"Select Nlt_Invno,Nlt_key,Nlt_ToPay,Nlt_ToPayCurr from nltran where 
                            Isnull(nlt_toPay,'0') <> 0 and   (NL_Code = '" + companyData.CO_PL_Control + "') AND(NLT_Ref = '" + cmbSupplier.SelectedValue + "') AND(NLT_Source <> 'SO') AND(ISNULL(NLT_CurrCode, '£') LIKE '" + ddBill.SelectedValue + "' + '%') " +
                       " AND (nltran.NLT_Date >=  Cast('" + dFrom.ToString("yyyyMMdd") + "' as Datetime)) AND (nltran.NLT_Date <= Cast('" + dto.ToString("yyyyMMdd") + "' as Datetime)) "
                        );
                foreach (DataRow r in dtTopay.Rows)
                {
                    double topaycurr = 0;
                    //double.TryParse(((TextBox)r.FindControl("tbToAllocate")).Text, out topaycurr);
                    topaycurr = AccountsDBMethods.CNo(r["Nlt_ToPayCurr"].ToString());
                    if (topaycurr != 0)
                    {
                        if (TransactionRef.Length < 28)
                        {
                            //TransactionRef += r.Cells[4].Text;
                            TransactionRef += r["Nlt_Invno"].ToString();
                        }
                        else
                            break;


                    }
                }
                if (TransactionRef.Length > 45)
                {
                    TransactionRef = TransactionRef.Substring(0, 45);
                }
                String PaidRef = DateTime.Now.ToString("yyyyMMdd HHmmss").Replace(" ", "") + "/" + TransactionRef;
                if (PaidRef.Length > 40)
                {
                    PaidRef = PaidRef.Substring(0, 40);
                }

                DateTime dPay = DateTime.Today;
                DateTime datYE = Convert.ToDateTime(companyData.CO_Current_YE);
                DateTime.TryParse(txtPayDate.Text, out dPay);
                while (dPay > datYE)
                {
                    datYE = datYE.AddYears(1);
                }
                while (dPay <= datYE.AddYears(-1))
                {
                    datYE = datYE.AddYears(-1);
                }
                int intPeriod = accountdb.GetPeriod(dPay, companyData);

                String Audit = "PP/" + TransactionRef;
                if (Audit.Length > 50)
                {
                    Audit = Audit.Substring(0, 49);
                }
                String DebitCode = accountdb.GVGet("DebitNoteNom");
                if (string.IsNullOrEmpty(DebitCode))
                {
                    DebitCode = "DB01";
                }
                String CISNom = "CIS-SALES";
                CISNom = accountdb.GVGet("CISSALES");
                string strForex = accountdb.GVGet("FOREX");
                string strF = accountdb.GeneralQuery("SELECT Forex FROM BankAcct WHERE AcctCode = '" + ddBank.SelectedValue + "' ");
                if (!string.IsNullOrEmpty(strF))
                {
                    strForex = strF;
                }
                if (string.IsNullOrEmpty(strForex))
                {
                    strForex = "FOREX";
                }

                double dblTotal = 0;
                double dblCurrTotal = 0;
                double dblDiscount = 0;
                double dblExchRate = 0;
                double dblExchVar = 0;
                double tranCurrTotal = 0;
                double DefCurrTotal = 0;
                double.TryParse(lblCurTotals.Text, out tranCurrTotal);
                double.TryParse(lblTotals.Text, out DefCurrTotal);
                double.TryParse(lblTranValue.Text, out dblTotal);
                double.TryParse(lblBankValue.Text, out dblCurrTotal);
                double.TryParse(txtDiscount.Text, out dblDiscount);
                double.TryParse(lblexchrate.Text, out dblExchRate);
                double.TryParse(txtexchVar.Text, out dblExchVar);
                if (dblDiscount > 0)
                {
                    dblDiscount = -dblDiscount;
                }
                double controlExch = 0;
                double.TryParse(Math.Round(tranCurrTotal / DefCurrTotal, 2).ToString(), out controlExch);
                if (controlExch.ToString().Contains("NaN"))
                {
                    controlExch = 0;
                }
                string receiptCurr = lblbankcurr.Text.Trim();

                if (dblDiscount == 0)
                {
                    if (DefCurrTotal == 0 && tranCurrTotal == 0 && dblTotal == 0 && dblCurrTotal == 0)
                    {
                        //do not create new payment
                    }
                    else
                    {
                        //control account
                        strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                            " Values ('" + TransactionRef + "','PP','" + PaidRef + "','" + cmbSupplier.SelectedValue + "','" + Math.Round(DefCurrTotal, 2).ToString() + "','" + Math.Round(tranCurrTotal, 2).ToString() + "','" + Math.Round(DefCurrTotal, 2).ToString() + "','" + Math.Round(tranCurrTotal, 2).ToString() + "','Payment','" + lblCurSel.Text + "','" + Math.Round(controlExch, 2).ToString() + "','" + companyData.CO_PL_Control + "' , Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                        // bank account
                        if (dblTotal != 0 && dblCurrTotal != 0)
                        {
                            double newTotal = dblTotal;
                            if (chkCalc.Checked == false)
                            {
                                newTotal = dblCurrTotal / dblExchRate;
                                dblTotal = newTotal;
                            }
                            strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                              " Values ('" + TransactionRef + "','PP','','" + cmbSupplier.SelectedValue + "','" + Math.Round(-dblTotal, 2).ToString() + "','" + Math.Round(-dblCurrTotal, 2).ToString() + "','0','0','Payment','" + receiptCurr + "','" + Math.Round(dblExchRate, 2).ToString() + "','" + ddBank.SelectedValue + "', Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                        }
                        else
                        {
                            strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                              " Values ('" + TransactionRef + "','PP','','" + cmbSupplier.SelectedValue + "','0','0','0','0','Payment','" + receiptCurr + "','" + Math.Round(dblExchRate, 2).ToString() + "','" + ddBank.SelectedValue + "', Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                        }

                        if (dblExchVar != 0)
                        {
                            strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                              " Values ('" + TransactionRef + "','FX','','" + cmbSupplier.SelectedValue + "','" + Math.Round(dblExchVar, 2).ToString() + "','" + Math.Round(dblExchVar, 2).ToString() + "','0','0','FX on Payment','" + receiptCurr + "','1','" + strForex + "', Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                        }
                    }
                }
                else
                {
                    //control account
                    strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                        " Values ('" + TransactionRef + "','PP','" + PaidRef + "','" + cmbSupplier.SelectedValue + "','" + Math.Round(DefCurrTotal, 2).ToString() + "','" + Math.Round(tranCurrTotal, 2).ToString() + "','" + Math.Round(DefCurrTotal, 2).ToString() + "','" + Math.Round(tranCurrTotal, 2).ToString() + "','Payment','" + lblCurSel.Text + "','" + Math.Round(controlExch, 2).ToString() + "','" + companyData.CO_PL_Control + "' , Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                    // bank account
                    if (dblTotal != 0 && dblCurrTotal != 0)
                    {
                        double newTotal = dblTotal;
                        if (chkCalc.Checked == false)
                        {
                            newTotal = dblCurrTotal / dblExchRate;
                            dblTotal = newTotal;
                        }
                        strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                          " Values ('" + TransactionRef + "','PP','','" + cmbSupplier.SelectedValue + "','" + Math.Round(-dblTotal, 2).ToString() + "','" + Math.Round(-dblCurrTotal, 2).ToString() + "','0','0','Payment','" + receiptCurr + "','" + Math.Round(dblExchRate, 2).ToString() + "','" + ddBank.SelectedValue + "', Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                    }
                    else
                    {
                        strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                          " Values ('" + TransactionRef + "','PP','','" + cmbSupplier.SelectedValue + "','0','0','0','0','Payment','" + receiptCurr + "','" + Math.Round(dblExchRate, 2).ToString() + "','" + ddBank.SelectedValue + "', Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                    }

                    if (dblDiscount != 0)
                    {
                        strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                          " Values ('" + TransactionRef + "','PP','','" + cmbSupplier.SelectedValue + "','" + Math.Round(dblDiscount, 2).ToString() + "','" + Math.Round(dblDiscount * dblExchRate, 2).ToString() + "','0','0','Discount','" + receiptCurr + "','" + Math.Round(dblExchRate, 2).ToString() + "','" + companyData.CO_Disc_Control + "', Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                    }
                    if (dblExchVar != 0)
                    {
                        strUpdateQuery += " Insert into nltran(NLT_Invno,NLT_Source,NLT_PaidRef,NLT_Ref,NLT_Net,NLT_CurrNet,NLT_Paid,nlt_currpaid,nlt_detail,nlt_currcode,nlt_exchrate,nl_code,nlt_date,nlt_yearend,nlt_period,nlt_audit,NLT_PayRef,NLT_TheirRef)" +
                          " Values ('" + TransactionRef + "','FX','','" + cmbSupplier.SelectedValue + "','" + Math.Round(dblExchVar, 2).ToString() + "','" + Math.Round(dblExchVar, 2).ToString() + "','0','0','FX on Payment','" + receiptCurr + "','1','" + strForex + "', Cast('" + dPay.ToString("yyyyMMdd") + "' as Datetime), Cast('" + datYE.ToString("yyyyMMdd") + "' as Datetime),'" + intPeriod.ToString() + "','" + Audit + "','" + txtPayRef.Text + "','" + txtPayRef.Text + "') ";
                    }
                }
                foreach (DataRow r in dtTopay.Rows)
                {
                    double topaycurr = 0;
                    //double.TryParse(((TextBox)r.FindControl("tbToAllocate")).Text, out topaycurr);
                    topaycurr = AccountsDBMethods.CNo(r["Nlt_ToPayCurr"].ToString());
                    if (topaycurr != 0)
                    {
                        //String topRecord = accountdb.GeneralQuery("select top 1 NLT_Key from nltran  Where nlt_invno ='" + r.Cells[4].Text + "' and nlt_source='" + r.Cells[2].Text + "' and nlt_ref='" + cmbSupplier.SelectedValue + "' and nl_code='" + companyData.CO_PL_Control + "' Order by NLT_Key asc");
                        String topRecord = r["Nlt_key"].ToString();
                        int topRec = 0;
                        int.TryParse(topRecord, out topRec);
                        String existPaidRef = accountdb.GeneralQuery("Select isnull(nlt_Paidref,'') from nltran where nlt_key = " + topRec);
                        if (existPaidRef.Trim() == "")
                        {
                            strUpdateQuery += " Update nltran Set nlt_paid=nlt_topay,nlt_currpaid=nlt_topaycurr,nlt_topay='0',nlt_topaycurr='0', nlt_paidref='" + PaidRef + "' where nlt_key = " + topRec;
                        }
                        else
                        {
                            strUpdateQuery += " Update nltran Set nlt_paid=isnull(nlt_paid,0) + isnull(nlt_topay,0),nlt_currpaid=isnull(nlt_currpaid,0) + isnull(nlt_topaycurr,0),nlt_topay='0',nlt_topaycurr='0' where nlt_key = " + topRec;
                            strUpdateQuery += " Update nltran Set  nlt_paidref='" + PaidRef + "' where nlt_paidref = '" + existPaidRef + "'";
                        }
                    }
                }
                String Changes = "Amount: " + Math.Round(DefCurrTotal, 2).ToString() + Environment.NewLine + "Supplier Ref:" + cmbSupplier.SelectedValue + Environment.NewLine + "Transaction Ref: " + TransactionRef;
                strUpdateQuery += " insert into audit(AuditRef,UserID,Date,Changes)values('" + Audit + "','" + loggedUser.UserID.ToString() + "',GetDate(),'" + Changes + "') ";
                int retValue = accountdb.CommandWithRollBack(strUpdateQuery.Replace("\r\n", " "));
                if (retValue > 0)
                {
                    mainMaster.LabelTitle = "Info!";
                    mainMaster.LabelValue = "Payment Saved ";
                    mainMaster.ShowMsgClass = "message";
                    //ScriptManager.RegisterStartupScript(this, typeof(string), "alert", "alert('Payment Saved');", true);
                    lblTranValue.Text = "0.00";
                    lblnetCurr.Text = "";
                    lblbankcurr.Text = "";
                    lblTotals.Text = "0.00";
                    lblBankValue.Text = "0.00";
                    lblCurTotals.Text = "0.00";
                    lblexchrate.Text = "1";
                    txtDiscount.Text = "0.00";
                    txtexchVar.Text = "0.00";
                    txtPayRef.Text = "";
                    BindPage();
                }
                else
                {
                    mainMaster.LabelTitle = "OOps!";
                    mainMaster.LabelValue = "Payment Failed.";
                    mainMaster.ShowMsgClass = "alert";
                    //ScriptManager.RegisterStartupScript(this, typeof(string), "alert", "alert('Payment Failed');", true);
                }
            }
            finally
            {
                btnPay.Enabled = true;
            }
        }
        protected void StatementUpdate()
        {
            //DebtChase/StatementCall.aspx
            string strStatement = "../Supplier/SLInvPrePrint.aspx";
            string strURL = strStatement + "?code=" + cmbSupplier.SelectedValue;
            btnStatement.Attributes.Clear();
            this.btnStatement.Attributes.Add("onclick", "window.open('" + strURL + "')");
        }
        protected void btnStatement_Click(object sender, EventArgs e)
        {
            BindPage();
        }

        protected void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            if (txtDiscount.Text == "")
            {
                txtDiscount.Text = "0";
            }
            else
            {
                Double dblToPay = AccountsDBMethods.CNo(lblTotals.Text);
                Double dblToPayCurr = AccountsDBMethods.CNo(lblCurTotals.Text);
                Double existDisc = AccountsDBMethods.CNo(txtDiscount.Text);

                if (existDisc > dblToPay)
                {
                    txtDiscount.Text = (-existDisc).ToString();
                    //ScriptManager.RegisterStartupScript(this, typeof(string), "alert", "alert('Discount must be less than total amount.');", true);
                }
            }
        }

        protected void Close1_Click(object sender, EventArgs e)
        {
            Response.Redirect("SuppTran.aspx", false);
        }

        protected void cb1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = ((CheckBox)sender);
            GridViewRow r = ((GridViewRow)chk.NamingContainer);
            //Double dbl8 = 0;
            //try
            //{
            //    dbl8 = AccountsDBMethods.CNo(r.Cells[8].Text);
            //}
            //catch { dbl8 = 0; }
            //((TextBox)r.FindControl("tbToAllocate")).Text = Math.Round(dbl8, 2).ToString();
            if (chk.Checked)
            {
                double topay = 0;
                double exchrate = 1;
                double topaycurr = 0;

                double dbl6 = 0;
                double dbl7 = 0;
                double dbl8 = 0;
                double dbl9 = 0;
                double dbl11 = 0;
                double.TryParse(r.Cells[6].Text, out dbl6);
                double.TryParse(r.Cells[7].Text, out dbl7);
                double.TryParse(r.Cells[8].Text, out dbl8);
                double.TryParse(r.Cells[9].Text, out dbl9);
                double.TryParse(r.Cells[11].Text, out dbl11);

                exchrate = dbl8 / dbl9;
                topaycurr = dbl8;
                //double.TryParse(((TextBox)r.FindControl("tbToAllocate")).Text, out topaycurr);
                if (topaycurr != 0)
                {
                    topay = topaycurr / exchrate;
                }
                else
                {
                    topay = 0;
                }
                String topRecord = accountdb.GeneralQuery("select top 1 NLT_Key from nltran  Where nlt_invno ='" + r.Cells[4].Text + "' and nlt_source='" + r.Cells[2].Text + "' and nlt_ref='" + cmbSupplier.SelectedValue + "' and nl_code='" + companyData.CO_PL_Control + "' Order by NLT_Key asc");
                int topRec = 0;
                int.TryParse(topRecord, out topRec);
                string strUpdateQuery = " Update nltran set nlt_topay='" + Math.Round(topay, 2).ToString() + "', nlt_topaycurr='" + Math.Round(topaycurr, 2).ToString() + "' Where nlt_key = " + topRec;
                int i = accountdb.GeneralCommand(strUpdateQuery);
                if (i > 0)
                {
                    ((TextBox)r.FindControl("tbToAllocate")).Text = Math.Round(dbl8, 2).ToString();

                }
            }
            else
            {
                String topRecord = accountdb.GeneralQuery("select top 1 NLT_Key from nltran  Where nlt_invno ='" + r.Cells[4].Text + "' and nlt_source='" + r.Cells[2].Text + "' and nlt_ref='" + cmbSupplier.SelectedValue + "' and nl_code='" + companyData.CO_PL_Control + "' Order by NLT_Key asc");
                int topRec = 0;
                int.TryParse(topRecord, out topRec);
                string strUpdateQuery = " Update nltran set nlt_topay='0', nlt_topaycurr='0' Where nlt_key = " + topRec;
                int i = accountdb.GeneralCommand(strUpdateQuery);
                if (i > 0)
                {
                    ((TextBox)r.FindControl("tbToAllocate")).Text = "0";
                }
            }

            DateTime dFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            DateTime dto = DateTime.Today;
            DateTime.TryParse(tbDateFrom.Text, out dFrom);
            DateTime.TryParse(tbDateTo.Text, out dto);
            string strQ = "SELECT SUM(-(ISNULL(NLT_ToPayCurr,0))) As ToPayCurr, SUM(-(ISNULL(NLT_ToPay,0))) As ToPay ";
            strQ += "FROM NLTran ";
            strQ += "WHERE NLT_Ref = '" + cmbSupplier.SelectedValue + "' ";
            strQ += "AND NL_Code = '" + companyData.CO_PL_Control + "' ";
            strQ += "AND isnull(NLT_ToPay,0) <> 0 AND isnull(NLT_ToPayCurr,0) <> 0";
            strQ += "AND (NLT_Date >=  Cast('" + dFrom.ToString("yyyyMMdd") + "' as Datetime)) AND (NLT_Date <= Cast('" + dto.ToString("yyyyMMdd") + "' as Datetime)) ";

            DataTable dtTotal = accountdb.GeneralQueryTable(strQ);
            double dblToPayCurr = 0;
            double dblToPay = 0;
            if (dtTotal.Rows.Count > 0)
            {
                double.TryParse(dtTotal.Rows[0]["ToPayCurr"].ToString(), out dblToPayCurr);
                double.TryParse(dtTotal.Rows[0]["ToPay"].ToString(), out dblToPay);
            }

            lblTotals.Text = Math.Round(dblToPay, 2).ToString();
            lblCurTotals.Text = Math.Round(dblToPayCurr, 2).ToString();
        }
        bool isBankValueEdit = false;
        protected void lblBankValue_TextChanged(object sender, EventArgs e)
        {
            chkCalc.Checked = false;
            double bankvalue = 0.00;
            Double.TryParse(lblBankValue.Text, out bankvalue);
            btnGet_Click(sender, e);
            isBankValueEdit = true;
            Compute(bankvalue);
        }

        protected void btnAmend_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/audit/allocation.aspx?acct=" + companyData.CO_PL_Control + "&ref=" + cmbSupplier.SelectedValue);

        }

        protected void gvInvoices_Sorting(object sender, GridViewSortEventArgs e)
        {
            Session["SortExpression"] = e.SortExpression;
            SortGrid(Session["SortExpression"].ToString());
        }
        private void SortGrid(String SortExpression)
        {
            String sDirect = "ASC";
            if (Session["SortDirection"] != null)
            {
                sDirect = Session["SortDirection"].ToString();
            }
            switch (sDirect)
            {
                case "DESC":
                    sDirect = "ASC";
                    break;
                case "ASC":
                    sDirect = "DESC";
                    break;
            }
            Session["SortDirection"] = sDirect;
            Session["SortExpression"] = SortExpression;
            DataView dvSort = new DataView(BindPage());
            dvSort.Sort = SortExpression + " " + sDirect;

            gvInvoices.DataSource = dvSort;
            gvInvoices.DataBind();
        }

        protected void btnOrders_Click(object sender, EventArgs e)
        {
            Response.Redirect("supporderssumm.aspx");
        }

        protected void PaytRecd_Click(object sender, EventArgs e)
        {
            lblText.Text = "";
            if (ddBank.Items.Count < 1)
            {
                lblText.Text = "To start receiving  payments - Click on the On Account Payment link and setup Bank Account details.";
                lblText.ForeColor = Color.Red;
                return;
            }
            string defaultCurr = accountdb.DefaultCurr(companyData.CO_Ctry);
            if (string.IsNullOrEmpty(defaultCurr))
            { defaultCurr = "£"; }
            lblbankcurr.Text = accountdb.GeneralQuery("SELECT isnull(BankAcct.CurrCode,'" + defaultCurr + "') FROM nominal RIGHT OUTER JOIN BankAcct ON nominal.NL_Code = BankAcct.ReceiptsControl where ReceiptsControl='" + ddBank.SelectedValue + "'");



            DateTime datYE = Convert.ToDateTime(companyData.CO_Current_YE);
            txtPayDate.Text = AccountsDBMethods.IsDate(txtPayDate.Text) == false ? DateTime.Today.ToShortDateString() : txtPayDate.Text;
            DateTime datDate = Convert.ToDateTime(txtPayDate.Text);

            while (datDate.Date > datYE.Date)
            {
                datYE = datYE.AddYears(1);
            }
            while (datDate.Date <= datYE.AddYears(-1))
            {
                datYE = datYE.AddYears(-1);
            }

            int intPeriod = accountdb.GetPeriod(datDate, companyData);

            double dblExchRate = AccountsDBMethods.CNo(lblexchrate.Text);


            if (dblExchRate == 0)
            {
                dblExchRate = 1;                
                if (accountdb.GVGet("ECBRate") != "True")
                {
                    dblExchRate = accountdb.GetExchRate(lblbankcurr.Text.Trim(), datDate, "", accountdb.DefaultCurr(companyData.CO_Ctry));

                }
                else
                {
                    dblExchRate = accountdb.GetECBRates(lblbankcurr.Text.Trim(), datDate, "", accountdb.DefaultCurr(companyData.CO_Ctry));
                }

            }
            if (dblExchRate == 0)
            {
                dblExchRate = 1;
            }

            while (datDate > datYE)
            {
                datYE = datYE.AddYears(1);
            }
            double dblDiscount = 0;
            double dblValue = 0;

            double dblDiscountNet = 0;
            if (txtOnAccount.Text.Trim() != "")
            {
                dblValue = AccountsDBMethods.CNo(txtOnAccount.Text);
            }

            double.TryParse(txtDiscOnAccount.Text, out dblDiscount);
            if (dblDiscount != 0)
            {
                dblDiscountNet = Math.Round(dblDiscount / dblExchRate, 2);
            }
          
            double dblNet = Math.Round((dblValue / dblExchRate), 2);
            double dblCurrNet = Math.Round(dblValue, 2);


            String strInvNo = "OA/" + DateTime.Now.ToString("yyyyMMdd HHmmss").Replace(" ", "") + "/" + cmbSupplier.SelectedValue;
            if (strInvNo.Length > 40)
            {
                strInvNo = strInvNo.Substring(0, 40);
            }

            // Credit
            this.SqlDSNomUpdate.InsertParameters["NLCode"].DefaultValue = ddBank.SelectedValue;
            this.SqlDSNomUpdate.InsertParameters["YE"].DefaultValue = Convert.ToString(datYE);
            this.SqlDSNomUpdate.InsertParameters["Period"].DefaultValue = intPeriod.ToString();
            this.SqlDSNomUpdate.InsertParameters["Detail"].DefaultValue = "Payment";
            this.SqlDSNomUpdate.InsertParameters["Net"].DefaultValue = string.Format("{0:N2}", -(dblNet - dblDiscountNet));
            this.SqlDSNomUpdate.InsertParameters["Audit"].DefaultValue = "PP/" + strInvNo;
            this.SqlDSNomUpdate.InsertParameters["InvNo"].DefaultValue = strInvNo;
            this.SqlDSNomUpdate.InsertParameters["CurrNet"].DefaultValue = string.Format("{0:N2}", -(dblCurrNet - dblDiscount));
            this.SqlDSNomUpdate.InsertParameters["Paid"].DefaultValue = "0";
            this.SqlDSNomUpdate.InsertParameters["CurrPaid"].DefaultValue = "0";
            this.SqlDSNomUpdate.InsertParameters["PaidRef"].DefaultValue = "";
            this.SqlDSNomUpdate.InsertParameters["Date"].DefaultValue = Convert.ToString(datDate);
            this.SqlDSNomUpdate.InsertParameters["Ref"].DefaultValue = cmbSupplier.SelectedValue; // Request.QueryString("Code");
            this.SqlDSNomUpdate.InsertParameters["CurrCode"].DefaultValue = lblbankcurr.Text.Trim();
            this.SqlDSNomUpdate.InsertParameters["ExchRate"].DefaultValue = dblExchRate.ToString();
            this.SqlDSNomUpdate.InsertParameters["TheirRef"].DefaultValue = txtReference.Text;
            this.SqlDSNomUpdate.InsertParameters["PayRef"].DefaultValue = "";
            this.SqlDSNomUpdate.Insert();
            //Debit
            this.SqlDSNomUpdate.InsertParameters["NLCode"].DefaultValue = companyData.CO_PL_Control;
            this.SqlDSNomUpdate.InsertParameters["YE"].DefaultValue = Convert.ToString(datYE);
            this.SqlDSNomUpdate.InsertParameters["Period"].DefaultValue = intPeriod.ToString();
            this.SqlDSNomUpdate.InsertParameters["Detail"].DefaultValue = "Receipt";
            this.SqlDSNomUpdate.InsertParameters["Net"].DefaultValue = string.Format("{0:N2}", dblNet);
            this.SqlDSNomUpdate.InsertParameters["Audit"].DefaultValue = "PP/" + strInvNo;
            this.SqlDSNomUpdate.InsertParameters["InvNo"].DefaultValue = strInvNo;
            this.SqlDSNomUpdate.InsertParameters["CurrNet"].DefaultValue = string.Format("{0:N2}", dblCurrNet);
            this.SqlDSNomUpdate.InsertParameters["Paid"].DefaultValue = (0).ToString();
            this.SqlDSNomUpdate.InsertParameters["PaidRef"].DefaultValue = "";
            this.SqlDSNomUpdate.InsertParameters["CurrPaid"].DefaultValue = (0).ToString();
            this.SqlDSNomUpdate.InsertParameters["TheirRef"].DefaultValue = txtReference.Text;
            this.SqlDSNomUpdate.InsertParameters["PayRef"].DefaultValue = "";
            this.SqlDSNomUpdate.Insert();
            double.TryParse(txtDiscOnAccount.Text, out dblDiscount);
            if (dblDiscount != 0)
            {                

                // Discount
                this.SqlDSNomUpdate.InsertParameters["NLCode"].DefaultValue = txtDiscNominalOnAccount.Text.Trim() == "" ? "DISC" : txtDiscNominalOnAccount.Text; ;
                this.SqlDSNomUpdate.InsertParameters["YE"].DefaultValue = Convert.ToString(datYE);
                this.SqlDSNomUpdate.InsertParameters["Period"].DefaultValue = intPeriod.ToString();
                this.SqlDSNomUpdate.InsertParameters["Detail"].DefaultValue = "Payment";
                this.SqlDSNomUpdate.InsertParameters["Net"].DefaultValue = string.Format("{0:N2}", -dblDiscountNet);
                this.SqlDSNomUpdate.InsertParameters["Audit"].DefaultValue = "PP/" + strInvNo;
                this.SqlDSNomUpdate.InsertParameters["InvNo"].DefaultValue = strInvNo;
                this.SqlDSNomUpdate.InsertParameters["CurrNet"].DefaultValue = string.Format("{0:N2}", -dblDiscount);
                this.SqlDSNomUpdate.InsertParameters["Paid"].DefaultValue = "0";
                this.SqlDSNomUpdate.InsertParameters["CurrPaid"].DefaultValue = "0";
                this.SqlDSNomUpdate.InsertParameters["PaidRef"].DefaultValue = "";
                this.SqlDSNomUpdate.InsertParameters["Date"].DefaultValue = Convert.ToString(datDate);
                this.SqlDSNomUpdate.InsertParameters["Ref"].DefaultValue = cmbSupplier.SelectedValue; // Request.QueryString("Code");
                this.SqlDSNomUpdate.InsertParameters["CurrCode"].DefaultValue = lblbankcurr.Text.Trim();
                this.SqlDSNomUpdate.InsertParameters["ExchRate"].DefaultValue = dblExchRate.ToString();
                this.SqlDSNomUpdate.InsertParameters["TheirRef"].DefaultValue = txtReference.Text;
                this.SqlDSNomUpdate.InsertParameters["PayRef"].DefaultValue = "";
                this.SqlDSNomUpdate.Insert();
            }


            Response.Redirect(Request.Url.AbsoluteUri);
        }
    }
}
