using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bycar;

namespace bycar3.Reporting
{
    public class Reporter
    {
        public static void GenerateRevisionReport(List<SpareView> items)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewRevision r = new ReportViewRevision(items);
            r.ShowDialog();
        }
        public static void GenerateRequestReport()
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewRequest r = new ReportViewRequest();
            r.ShowDialog();
        }
        public static void GenerateRequestReport(List<SpareView> items)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewRequest r = new ReportViewRequest(items);
            r.ShowDialog();
        }
        public static void GenerateOverpricingReport(int ID)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewOverpricing r = new ReportViewOverpricing(ID);
            r.ShowDialog();
        }
        public static void GenerateInvoiceReport(int InvoiceId)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewInvoice r = new ReportViewInvoice(InvoiceId);
            r.ShowDialog();
        }
        public static void GenerateTNFromSpareIncomeId(int SpareIncomeId)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewSpareOutgo r = new ReportViewSpareOutgo(SpareIncomeId);
            r.ShowDialog();
        }
        public static void GenerateSalesCheck(int SpareIncomeId)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewSalesCheck r = new ReportViewSalesCheck(SpareIncomeId);
            r.ShowDialog();
        }
        public static void GenerateSalesCheck(Sale sale)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewSalesCheck r = new ReportViewSalesCheck(sale);
            r.ShowDialog();
        }
        public static void GenerateTTNFromSpareIncomeId(int SpareIncomeId)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewSpareOutgoTTN r = new ReportViewSpareOutgoTTN(SpareIncomeId);
            r.ShowDialog();
        }
        public static void GenerateTTNAppendixFromSpareIncomeId(int SpareIncomeId)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewSpareOutgoTTNAppendix r = new ReportViewSpareOutgoTTNAppendix(SpareIncomeId);
            r.ShowDialog();
        }
        public static void GenerateTTNWithAppendixFromSpareIncomeId(int SpareIncomeId)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewSpareOutgoTTNWithAppendix r = new ReportViewSpareOutgoTTNWithAppendix(SpareIncomeId);
            r.ShowDialog();
        }
        public static void GenerateDailySalesReport(DateTime date, DateTime dateTo)
        {
            //TODO: вывести показ отчета в отдельный процесс
            ReportViewDailySales r = new ReportViewDailySales(date, dateTo);
            r.ShowDialog();
        }
        public static void GenerateSpareSalesReportByPeriod(int SpareID, DateTime dateFrom, DateTime dateTo)
        {
            ReportViewSpareSalesByPeriod r = new ReportViewSpareSalesByPeriod(dateFrom, dateTo, SpareID);
            r.ShowDialog();
        }
    }
}
